using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;
using GerenciadorTarefas.Api.Contratos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/projetos")]
public sealed class ControladorProjetos : ControllerBase
{
    private readonly IConsultaProjetosCasoDeUso consultaProjetosCasoDeUso;
    private readonly ICriarProjetoCasoDeUso criarProjetoCasoDeUso;
    private readonly IAtualizarProjetoCasoDeUso atualizarProjetoCasoDeUso;
    private readonly IExcluirProjetoCasoDeUso excluirProjetoCasoDeUso;

    public ControladorProjetos(
        IConsultaProjetosCasoDeUso consultaProjetosCasoDeUso,
        ICriarProjetoCasoDeUso criarProjetoCasoDeUso,
        IAtualizarProjetoCasoDeUso atualizarProjetoCasoDeUso,
        IExcluirProjetoCasoDeUso excluirProjetoCasoDeUso)
    {
        this.consultaProjetosCasoDeUso = consultaProjetosCasoDeUso;
        this.criarProjetoCasoDeUso = criarProjetoCasoDeUso;
        this.atualizarProjetoCasoDeUso = atualizarProjetoCasoDeUso;
        this.excluirProjetoCasoDeUso = excluirProjetoCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>>> ListarProjetosAsync(
        CancellationToken cancellationToken)
    {
        var projetos = await consultaProjetosCasoDeUso.ListarAsync(cancellationToken);

        var resposta = new RespostaSucessoApi<IReadOnlyCollection<ProjetoResposta>>
        {
            Mensagem = "Projetos listados com sucesso.",
            Dados = projetos,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<ProjetoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<ProjetoResposta>>> ObterProjetoPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var projeto = await consultaProjetosCasoDeUso.ObterPorIdAsync(id, cancellationToken);

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
        var entrada = new CriarProjetoEntrada
        {
            Nome = requisicao.Nome,
            Descricao = requisicao.Descricao
        };

        var projetoCriado = await criarProjetoCasoDeUso.ExecutarAsync(entrada, cancellationToken);

        var resposta = new RespostaSucessoApi<ProjetoResposta>
        {
            Mensagem = "Projeto criado com sucesso.",
            Dados = projetoCriado,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return CreatedAtAction(nameof(ObterProjetoPorIdAsync), new { id = projetoCriado.Id }, resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<ProjetoResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<ProjetoResposta>>> AtualizarProjetoAsync(
        Guid id,
        [FromBody] AtualizarProjetoRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var entrada = new AtualizarProjetoEntrada
        {
            Nome = requisicao.Nome,
            Descricao = requisicao.Descricao
        };

        var projetoAtualizado = await atualizarProjetoCasoDeUso.ExecutarAsync(id, entrada, cancellationToken);

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
        await excluirProjetoCasoDeUso.ExecutarAsync(id, cancellationToken);

        var resposta = new RespostaSucessoApi<object?>
        {
            Mensagem = "Projeto excluido com sucesso.",
            Dados = null,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
