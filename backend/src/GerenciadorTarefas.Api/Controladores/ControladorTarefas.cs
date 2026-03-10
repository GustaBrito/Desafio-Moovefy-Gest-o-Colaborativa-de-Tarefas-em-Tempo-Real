using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Paginacao;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Servicos.Cache;
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
    private readonly IServicoCacheConsulta servicoCacheConsulta;

    public ControladorTarefas(
        IConsultaTarefasCasoDeUso consultaTarefasCasoDeUso,
        ICriarTarefaCasoDeUso criarTarefaCasoDeUso,
        IAtualizarTarefaCasoDeUso atualizarTarefaCasoDeUso,
        IAtualizarStatusTarefaCasoDeUso atualizarStatusTarefaCasoDeUso,
        IExcluirTarefaCasoDeUso excluirTarefaCasoDeUso,
        IServicoCacheConsulta servicoCacheConsulta)
    {
        this.consultaTarefasCasoDeUso = consultaTarefasCasoDeUso;
        this.criarTarefaCasoDeUso = criarTarefaCasoDeUso;
        this.atualizarTarefaCasoDeUso = atualizarTarefaCasoDeUso;
        this.atualizarStatusTarefaCasoDeUso = atualizarStatusTarefaCasoDeUso;
        this.excluirTarefaCasoDeUso = excluirTarefaCasoDeUso;
        this.servicoCacheConsulta = servicoCacheConsulta;
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
        var dataPrazoInicialUtc = NormalizarDataFiltroParaUtc(dataPrazoInicial, fimDoDia: false);
        var dataPrazoFinalUtc = NormalizarDataFiltroParaUtc(dataPrazoFinal, fimDoDia: true);

        var filtro = new FiltroConsultaTarefasEntrada
        {
            ProjetoId = projetoId,
            Status = status,
            ResponsavelId = responsavelId,
            DataPrazoInicial = dataPrazoInicialUtc,
            DataPrazoFinal = dataPrazoFinalUtc,
            CampoOrdenacao = campoOrdenacao,
            DirecaoOrdenacao = direcaoOrdenacao,
            NumeroPagina = numeroPagina,
            TamanhoPagina = tamanhoPagina
        };

        var chaveCache = ChavesCacheConsulta.ObterListaTarefas(
            projetoId,
            status,
            responsavelId,
            dataPrazoInicialUtc,
            dataPrazoFinalUtc,
            campoOrdenacao,
            direcaoOrdenacao,
            numeroPagina,
            tamanhoPagina);

        var tarefas = await servicoCacheConsulta.ObterOuCriarAsync(
            chaveCache,
            PoliticasCacheConsulta.DuracaoTarefas,
            _ => consultaTarefasCasoDeUso.ListarAsync(filtro, cancellationToken),
            cancellationToken);

        var resposta = new RespostaSucessoApi<ResultadoPaginado<TarefaResposta>>
        {
            Mensagem = "Tarefas listadas com sucesso.",
            Dados = tarefas,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [HttpGet("{id:guid}", Name = "obter_tarefa_por_id")]
    [ProducesResponseType(typeof(RespostaSucessoApi<TarefaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<TarefaResposta>>> ObterTarefaPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var tarefa = await servicoCacheConsulta.ObterOuCriarAsync(
            ChavesCacheConsulta.ObterTarefaPorId(id),
            PoliticasCacheConsulta.DuracaoTarefas,
            _ => consultaTarefasCasoDeUso.ObterPorIdAsync(id, cancellationToken),
            cancellationToken);

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
            DataPrazo = NormalizarDataRequisicaoParaUtc(requisicao.DataPrazo)
        };

        var tarefaCriada = await criarTarefaCasoDeUso.ExecutarAsync(entrada, cancellationToken);
        InvalidarCacheTarefasEDashboard();

        var resposta = new RespostaSucessoApi<TarefaResposta>
        {
            Mensagem = "Tarefa criada com sucesso.",
            Dados = tarefaCriada,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return CreatedAtRoute("obter_tarefa_por_id", new { id = tarefaCriada.Id }, resposta);
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
            DataPrazo = NormalizarDataRequisicaoParaUtc(requisicao.DataPrazo)
        };

        var tarefaAtualizada = await atualizarTarefaCasoDeUso.ExecutarAsync(id, entrada, cancellationToken);
        InvalidarCacheTarefasEDashboard();

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
        InvalidarCacheTarefasEDashboard();

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
        InvalidarCacheTarefasEDashboard();

        var resposta = new RespostaSucessoApi<object?>
        {
            Mensagem = "Tarefa excluida com sucesso.",
            Dados = null,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    private void InvalidarCacheTarefasEDashboard()
    {
        servicoCacheConsulta.RemoverPorPrefixo(ChavesCacheConsulta.PrefixoTarefas);
        servicoCacheConsulta.Remover(ChavesCacheConsulta.ObterMetricasDashboard());
    }

    private static DateTime NormalizarDataRequisicaoParaUtc(DateTime valor)
    {
        return NormalizarParaUtc(valor);
    }

    private static DateTime? NormalizarDataFiltroParaUtc(DateTime? valor, bool fimDoDia)
    {
        if (!valor.HasValue)
        {
            return null;
        }

        var valorOriginal = valor.Value;
        var valorUtc = NormalizarParaUtc(valorOriginal);

        if (valorOriginal.TimeOfDay != TimeSpan.Zero)
        {
            return valorUtc;
        }

        return fimDoDia
            ? valorUtc.Date.AddDays(1).AddTicks(-1)
            : valorUtc.Date;
    }

    private static DateTime NormalizarParaUtc(DateTime valor)
    {
        return valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };
    }
}
