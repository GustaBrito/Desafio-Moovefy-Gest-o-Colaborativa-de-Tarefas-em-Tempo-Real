using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Seguranca;
using GerenciadorTarefas.Api.Servicos.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/projetos")]
public sealed class ControladorProjetos : ControllerBase
{
    private readonly IConsultaProjetosCasoDeUso consultaProjetosCasoDeUso;
    private readonly ICriarProjetoCasoDeUso criarProjetoCasoDeUso;
    private readonly IAtualizarProjetoCasoDeUso atualizarProjetoCasoDeUso;
    private readonly IExcluirProjetoCasoDeUso excluirProjetoCasoDeUso;
    private readonly IServicoCacheConsulta servicoCacheConsulta;

    public ControladorProjetos(
        IConsultaProjetosCasoDeUso consultaProjetosCasoDeUso,
        ICriarProjetoCasoDeUso criarProjetoCasoDeUso,
        IAtualizarProjetoCasoDeUso atualizarProjetoCasoDeUso,
        IExcluirProjetoCasoDeUso excluirProjetoCasoDeUso,
        IServicoCacheConsulta servicoCacheConsulta)
    {
        this.consultaProjetosCasoDeUso = consultaProjetosCasoDeUso;
        this.criarProjetoCasoDeUso = criarProjetoCasoDeUso;
        this.atualizarProjetoCasoDeUso = atualizarProjetoCasoDeUso;
        this.excluirProjetoCasoDeUso = excluirProjetoCasoDeUso;
        this.servicoCacheConsulta = servicoCacheConsulta;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>>> ListarProjetosAsync(
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        var chaveCache = $"{ChavesCacheConsulta.ObterListaProjetos()}:u:{contextoUsuario.UsuarioId:N}";

        var projetos = await servicoCacheConsulta.ObterOuCriarAsync(
            chaveCache,
            PoliticasCacheConsulta.DuracaoProjetos,
            _ => consultaProjetosCasoDeUso.ListarAsync(
                contextoUsuario.EhSuperAdmin ? null : contextoUsuario.AreaIds,
                cancellationToken),
            cancellationToken);

        var resposta = new RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>
        {
            Mensagem = "Projetos listados com sucesso.",
            Dados = projetos,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpGet("{id:guid}", Name = "obter_projeto_por_id")]
    [ProducesResponseType(typeof(RespostaSucessoApi<ProjetoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<ProjetoResposta>>> ObterProjetoPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        var projeto = await servicoCacheConsulta.ObterOuCriarAsync(
            $"{ChavesCacheConsulta.ObterProjetoPorId(id)}:u:{contextoUsuario.UsuarioId:N}",
            PoliticasCacheConsulta.DuracaoProjetos,
            _ => consultaProjetosCasoDeUso.ObterPorIdAsync(id, cancellationToken),
            cancellationToken);

        if (!UsuarioPodeAcessarProjeto(contextoUsuario, projeto))
        {
            return Forbid();
        }

        var resposta = new RespostaSucessoApi<ProjetoResposta>
        {
            Mensagem = "Projeto obtido com sucesso.",
            Dados = projeto,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RespostaSucessoApi<ProjetoResposta>), StatusCodes.Status201Created)]
    public async Task<ActionResult<RespostaSucessoApi<ProjetoResposta>>> CriarProjetoAsync(
        [FromBody] CriarProjetoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        var areaIds = NormalizarAreaIds(requisicao.AreaIds, requisicao.AreaId);
        var usuarioIdsVinculados = NormalizarIds(requisicao.UsuarioIdsVinculados);

        if (contextoUsuario.EhColaborador)
        {
            return Forbid();
        }

        if (!contextoUsuario.EhSuperAdmin && areaIds.Any(areaId => !contextoUsuario.AreaIds.Contains(areaId)))
        {
            return Forbid();
        }

        if (!usuarioIdsVinculados.Contains(contextoUsuario.UsuarioId))
        {
            usuarioIdsVinculados.Add(contextoUsuario.UsuarioId);
        }

        var entrada = new CriarProjetoEntrada
        {
            Nome = requisicao.Nome,
            Descricao = requisicao.Descricao,
            AreaIds = areaIds,
            AreaIdLegado = requisicao.AreaId,
            UsuarioIdsVinculados = usuarioIdsVinculados,
            GestorUsuarioId = requisicao.GestorUsuarioId,
            CriadoPorUsuarioId = contextoUsuario.UsuarioId
        };

        var projetoCriado = await criarProjetoCasoDeUso.ExecutarAsync(entrada, cancellationToken);
        InvalidarCacheProjetos();

        var resposta = new RespostaSucessoApi<ProjetoResposta>
        {
            Mensagem = "Projeto criado com sucesso.",
            Dados = projetoCriado,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return CreatedAtRoute("obter_projeto_por_id", new { id = projetoCriado.Id }, resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<ProjetoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<ProjetoResposta>>> AtualizarProjetoAsync(
        Guid id,
        [FromBody] AtualizarProjetoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        var areaIds = NormalizarAreaIds(requisicao.AreaIds, requisicao.AreaId);
        var usuarioIdsVinculados = NormalizarIds(requisicao.UsuarioIdsVinculados);

        if (contextoUsuario.EhColaborador)
        {
            return Forbid();
        }

        var projetoAtual = await consultaProjetosCasoDeUso.ObterPorIdAsync(id, cancellationToken);
        if (!UsuarioPodeAcessarProjeto(contextoUsuario, projetoAtual))
        {
            return Forbid();
        }

        if (!contextoUsuario.EhSuperAdmin && areaIds.Any(areaId => !contextoUsuario.AreaIds.Contains(areaId)))
        {
            return Forbid();
        }

        var entrada = new AtualizarProjetoEntrada
        {
            Nome = requisicao.Nome,
            Descricao = requisicao.Descricao,
            AreaIds = areaIds,
            AreaIdLegado = requisicao.AreaId,
            UsuarioIdsVinculados = usuarioIdsVinculados,
            GestorUsuarioId = requisicao.GestorUsuarioId
        };

        var projetoAtualizado = await atualizarProjetoCasoDeUso.ExecutarAsync(id, entrada, cancellationToken);
        InvalidarCacheProjetos();

        var resposta = new RespostaSucessoApi<ProjetoResposta>
        {
            Mensagem = "Projeto atualizado com sucesso.",
            Dados = projetoAtualizado,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<object?>>> ExcluirProjetoAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        if (contextoUsuario.EhColaborador)
        {
            return Forbid();
        }

        var projetoAtual = await consultaProjetosCasoDeUso.ObterPorIdAsync(id, cancellationToken);
        if (!UsuarioPodeAcessarProjeto(contextoUsuario, projetoAtual))
        {
            return Forbid();
        }

        await excluirProjetoCasoDeUso.ExecutarAsync(id, cancellationToken);
        InvalidarCacheProjetos();

        var resposta = new RespostaSucessoApi<object?>
        {
            Mensagem = "Projeto excluido com sucesso.",
            Dados = null,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    private void InvalidarCacheProjetos()
    {
        servicoCacheConsulta.RemoverPorPrefixo(ChavesCacheConsulta.PrefixoProjetos);
    }

    private static List<Guid> NormalizarAreaIds(IReadOnlyCollection<Guid>? areaIds, Guid areaIdLegado)
    {
        var resultado = areaIds?
            .Where(areaId => areaId != Guid.Empty)
            .Distinct()
            .ToList()
            ?? [];

        if (areaIdLegado != Guid.Empty && !resultado.Contains(areaIdLegado))
        {
            resultado.Insert(0, areaIdLegado);
        }

        return resultado;
    }

    private static List<Guid> NormalizarIds(IReadOnlyCollection<Guid>? ids)
    {
        return ids?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList()
            ?? [];
    }

    private static bool UsuarioPodeAcessarProjeto(
        ContextoUsuarioAutenticado contextoUsuario,
        ProjetoResposta projeto)
    {
        if (contextoUsuario.EhSuperAdmin)
        {
            return true;
        }

        if (contextoUsuario.AreaIds.Contains(projeto.AreaId))
        {
            return true;
        }

        return projeto.AreaIds.Any(areaId => contextoUsuario.AreaIds.Contains(areaId));
    }
}
