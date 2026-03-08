using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Paginacao;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/tarefas")]
public sealed class ControladorTarefas : ControllerBase
{
    private readonly IConsultaTarefasCasoDeUso consultaTarefasCasoDeUso;
    private readonly ICriarTarefaCasoDeUso criarTarefaCasoDeUso;
    private readonly IAtualizarTarefaCasoDeUso atualizarTarefaCasoDeUso;
    private readonly IAtualizarStatusTarefaCasoDeUso atualizarStatusTarefaCasoDeUso;
    private readonly IExcluirTarefaCasoDeUso excluirTarefaCasoDeUso;

    public ControladorTarefas(
        IConsultaTarefasCasoDeUso consultaTarefasCasoDeUso,
        ICriarTarefaCasoDeUso criarTarefaCasoDeUso,
        IAtualizarTarefaCasoDeUso atualizarTarefaCasoDeUso,
        IAtualizarStatusTarefaCasoDeUso atualizarStatusTarefaCasoDeUso,
        IExcluirTarefaCasoDeUso excluirTarefaCasoDeUso)
    {
        this.consultaTarefasCasoDeUso = consultaTarefasCasoDeUso;
        this.criarTarefaCasoDeUso = criarTarefaCasoDeUso;
        this.atualizarTarefaCasoDeUso = atualizarTarefaCasoDeUso;
        this.atualizarStatusTarefaCasoDeUso = atualizarStatusTarefaCasoDeUso;
        this.excluirTarefaCasoDeUso = excluirTarefaCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<ResultadoPaginado<TarefaResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<ResultadoPaginado<TarefaResposta>>>> ListarTarefasAsync(
        [FromQuery] Guid? projetoId,
        [FromQuery] StatusTarefa? status,
        [FromQuery] Guid? responsavelId,
        [FromQuery] DateTime? dataPrazoInicial,
        [FromQuery] DateTime? dataPrazoFinal,
        [FromQuery] CampoOrdenacaoTarefa? campoOrdenacao,
        [FromQuery] DirecaoOrdenacaoTarefa? direcaoOrdenacao,
        [FromQuery] int numeroPagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        var filtro = new FiltroConsultaTarefasEntrada
        {
            ProjetoId = projetoId,
            Status = status,
            ResponsavelId = responsavelId,
            DataPrazoInicial = dataPrazoInicial,
            DataPrazoFinal = dataPrazoFinal,
            CampoOrdenacao = campoOrdenacao,
            DirecaoOrdenacao = direcaoOrdenacao,
            NumeroPagina = numeroPagina,
            TamanhoPagina = tamanhoPagina
        };

        var tarefas = await consultaTarefasCasoDeUso.ListarAsync(filtro, cancellationToken);

        var resposta = new RespostaSucessoApi<ResultadoPaginado<TarefaResposta>>
        {
            Mensagem = "Tarefas listadas com sucesso.",
            Dados = tarefas,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<TarefaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<TarefaResposta>>> ObterTarefaPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tarefa = await consultaTarefasCasoDeUso.ObterPorIdAsync(id, cancellationToken);

        var resposta = new RespostaSucessoApi<TarefaResposta>
        {
            Mensagem = "Tarefa obtida com sucesso.",
            Dados = tarefa,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RespostaSucessoApi<TarefaResposta>), StatusCodes.Status201Created)]
    public async Task<ActionResult<RespostaSucessoApi<TarefaResposta>>> CriarTarefaAsync(
        [FromBody] CriarTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = requisicao.Titulo,
            Descricao = requisicao.Descricao,
            Prioridade = requisicao.Prioridade,
            ProjetoId = requisicao.ProjetoId,
            ResponsavelId = requisicao.ResponsavelId,
            DataPrazo = requisicao.DataPrazo
        };

        var tarefaCriada = await criarTarefaCasoDeUso.ExecutarAsync(entrada, cancellationToken);

        var resposta = new RespostaSucessoApi<TarefaResposta>
        {
            Mensagem = "Tarefa criada com sucesso.",
            Dados = tarefaCriada,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return CreatedAtAction(nameof(ObterTarefaPorIdAsync), new { id = tarefaCriada.Id }, resposta);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<TarefaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<TarefaResposta>>> AtualizarTarefaAsync(
        Guid id,
        [FromBody] AtualizarTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = requisicao.Titulo,
            Descricao = requisicao.Descricao,
            Status = requisicao.Status,
            Prioridade = requisicao.Prioridade,
            ResponsavelId = requisicao.ResponsavelId,
            DataPrazo = requisicao.DataPrazo
        };

        var tarefaAtualizada = await atualizarTarefaCasoDeUso.ExecutarAsync(id, entrada, cancellationToken);

        var resposta = new RespostaSucessoApi<TarefaResposta>
        {
            Mensagem = "Tarefa atualizada com sucesso.",
            Dados = tarefaAtualizada,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(RespostaSucessoApi<TarefaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<TarefaResposta>>> AtualizarStatusTarefaAsync(
        Guid id,
        [FromBody] AtualizarStatusTarefaRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var entrada = new AtualizarStatusTarefaEntrada
        {
            Status = requisicao.Status
        };

        var tarefaAtualizada = await atualizarStatusTarefaCasoDeUso.ExecutarAsync(id, entrada, cancellationToken);

        var resposta = new RespostaSucessoApi<TarefaResposta>
        {
            Mensagem = "Status da tarefa atualizado com sucesso.",
            Dados = tarefaAtualizada,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<object?>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<object?>>> ExcluirTarefaAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        await excluirTarefaCasoDeUso.ExecutarAsync(id, cancellationToken);

        var resposta = new RespostaSucessoApi<object?>
        {
            Mensagem = "Tarefa excluida com sucesso.",
            Dados = null,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
