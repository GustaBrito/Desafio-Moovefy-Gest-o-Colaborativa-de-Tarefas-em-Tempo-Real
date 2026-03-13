using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Servicos.Observabilidade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/observabilidade")]
public sealed class ControladorObservabilidade : ControllerBase
{
    private readonly ServicoMetricasOperacionais servicoMetricasOperacionais;

    public ControladorObservabilidade(ServicoMetricasOperacionais servicoMetricasOperacionais)
    {
        this.servicoMetricasOperacionais = servicoMetricasOperacionais;
    }

    [AllowAnonymous]
    [HttpGet("metricas")]
    [ProducesResponseType(typeof(RespostaSucessoApi<MetricasOperacionaisSnapshot>), StatusCodes.Status200OK)]
    public ActionResult<RespostaSucessoApi<MetricasOperacionaisSnapshot>> ObterMetricas()
    {
        var snapshot = servicoMetricasOperacionais.ObterSnapshot();

        return Ok(new RespostaSucessoApi<MetricasOperacionaisSnapshot>
        {
            Mensagem = "Metricas operacionais obtidas com sucesso.",
            Dados = snapshot,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }
}
