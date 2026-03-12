using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Usuarios;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Seguranca;
using GerenciadorTarefas.Dominio.Enumeracoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize(Policy = PoliticasAutorizacao.AdministracaoUsuarios)]
[Route("api/usuarios")]
public sealed class ControladorUsuarios : ControllerBase
{
    private readonly IConsultaUsuariosCasoDeUso consultaUsuariosCasoDeUso;
    private readonly ICriarUsuarioCasoDeUso criarUsuarioCasoDeUso;
    private readonly IAtualizarUsuarioCasoDeUso atualizarUsuarioCasoDeUso;
    private readonly IAlterarStatusUsuarioCasoDeUso alterarStatusUsuarioCasoDeUso;

    public ControladorUsuarios(
        IConsultaUsuariosCasoDeUso consultaUsuariosCasoDeUso,
        ICriarUsuarioCasoDeUso criarUsuarioCasoDeUso,
        IAtualizarUsuarioCasoDeUso atualizarUsuarioCasoDeUso,
        IAlterarStatusUsuarioCasoDeUso alterarStatusUsuarioCasoDeUso)
    {
        this.consultaUsuariosCasoDeUso = consultaUsuariosCasoDeUso;
        this.criarUsuarioCasoDeUso = criarUsuarioCasoDeUso;
        this.atualizarUsuarioCasoDeUso = atualizarUsuarioCasoDeUso;
        this.alterarStatusUsuarioCasoDeUso = alterarStatusUsuarioCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<UsuarioResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<UsuarioResposta>>>> ListarAsync(
        [FromQuery] bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();
        var usuarios = await consultaUsuariosCasoDeUso.ListarAsync(
            contexto.EhSuperAdmin ? null : contexto.AreaIds,
            somenteAtivos,
            cancellationToken);

        return Ok(new RespostaSucessoApi<IReadOnlyCollection<UsuarioResposta>>
        {
            Mensagem = "Usuarios listados com sucesso.",
            Dados = usuarios,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("{id:guid}", Name = "obter_usuario_por_id")]
    [ProducesResponseType(typeof(RespostaSucessoApi<UsuarioResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<UsuarioResposta>>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();
        var usuario = await consultaUsuariosCasoDeUso.ObterPorIdAsync(id, cancellationToken);

        if (!contexto.EhSuperAdmin && !PertenceAoEscopo(contexto.AreaIds, usuario.AreaIds))
        {
            return Forbid();
        }

        return Ok(new RespostaSucessoApi<UsuarioResposta>
        {
            Mensagem = "Usuario obtido com sucesso.",
            Dados = usuario,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(RespostaSucessoApi<UsuarioResposta>), StatusCodes.Status201Created)]
    public async Task<ActionResult<RespostaSucessoApi<UsuarioResposta>>> CriarAsync(
        [FromBody] CriarUsuarioRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();

        if (contexto.EhAdmin)
        {
            if (requisicao.PerfilGlobal != PerfilGlobalUsuario.Colaborador)
            {
                return Forbid();
            }

            if (!PertenceAoEscopo(contexto.AreaIds, requisicao.AreaIds))
            {
                return Forbid();
            }
        }

        var usuarioCriado = await criarUsuarioCasoDeUso.ExecutarAsync(new CriarUsuarioEntrada
        {
            Nome = requisicao.Nome,
            Email = requisicao.Email,
            Senha = requisicao.Senha,
            PerfilGlobal = requisicao.PerfilGlobal,
            Ativo = requisicao.Ativo,
            AreaIds = requisicao.AreaIds
        }, cancellationToken);

        return CreatedAtRoute("obter_usuario_por_id", new { id = usuarioCriado.Id }, new RespostaSucessoApi<UsuarioResposta>
        {
            Mensagem = "Usuario criado com sucesso.",
            Dados = usuarioCriado,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<UsuarioResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<UsuarioResposta>>> AtualizarAsync(
        Guid id,
        [FromBody] AtualizarUsuarioRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();
        var usuarioAtual = await consultaUsuariosCasoDeUso.ObterPorIdAsync(id, cancellationToken);

        if (contexto.EhAdmin)
        {
            if (usuarioAtual.PerfilGlobal != PerfilGlobalUsuario.Colaborador
                || requisicao.PerfilGlobal != PerfilGlobalUsuario.Colaborador)
            {
                return Forbid();
            }

            if (!PertenceAoEscopo(contexto.AreaIds, usuarioAtual.AreaIds)
                || !PertenceAoEscopo(contexto.AreaIds, requisicao.AreaIds))
            {
                return Forbid();
            }
        }

        var usuarioAtualizado = await atualizarUsuarioCasoDeUso.ExecutarAsync(id, new AtualizarUsuarioEntrada
        {
            Nome = requisicao.Nome,
            Email = requisicao.Email,
            PerfilGlobal = requisicao.PerfilGlobal,
            Ativo = requisicao.Ativo,
            NovaSenha = requisicao.NovaSenha,
            AreaIds = requisicao.AreaIds
        }, cancellationToken);

        return Ok(new RespostaSucessoApi<UsuarioResposta>
        {
            Mensagem = "Usuario atualizado com sucesso.",
            Dados = usuarioAtualizado,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(RespostaSucessoApi<UsuarioResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<UsuarioResposta>>> AlterarStatusAsync(
        Guid id,
        [FromBody] AlterarStatusUsuarioRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();
        var usuarioAtual = await consultaUsuariosCasoDeUso.ObterPorIdAsync(id, cancellationToken);

        if (contexto.EhAdmin)
        {
            if (usuarioAtual.PerfilGlobal != PerfilGlobalUsuario.Colaborador)
            {
                return Forbid();
            }

            if (!PertenceAoEscopo(contexto.AreaIds, usuarioAtual.AreaIds))
            {
                return Forbid();
            }
        }

        var usuarioAtualizado = await alterarStatusUsuarioCasoDeUso.ExecutarAsync(id, new AlterarStatusUsuarioEntrada
        {
            Ativo = requisicao.Ativo
        }, cancellationToken);

        return Ok(new RespostaSucessoApi<UsuarioResposta>
        {
            Mensagem = "Status do usuario atualizado com sucesso.",
            Dados = usuarioAtualizado,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    private static bool PertenceAoEscopo(
        IReadOnlyCollection<Guid> areasEscopo,
        IReadOnlyCollection<Guid> areasUsuario)
    {
        if (areasUsuario.Count == 0)
        {
            return false;
        }

        return areasUsuario.All(areasEscopo.Contains);
    }
}
