using GerenciadorTarefas.Aplicacao.Contratos.Dashboard;
using GerenciadorTarefas.Aplicacao.Modelos.Dashboard;
using GerenciadorTarefas.Api.Contratos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/dashboard")]
public sealed class ControladorDashboard : ControllerBase
{
    private readonly IConsultaMetricasDashboardCasoDeUso consultaMetricasDashboardCasoDeUso;

    public ControladorDashboard(IConsultaMetricasDashboardCasoDeUso consultaMetricasDashboardCasoDeUso)
    {
        this.consultaMetricasDashboardCasoDeUso = consultaMetricasDashboardCasoDeUso;
    }

    [HttpGet("metricas")]
    [ProducesResponseType(typeof(RespostaSucessoApi<MetricasDashboardResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<MetricasDashboardResposta>>> ObterMetricasAsync(
        CancellationToken cancellationToken)
    {
        var metricas = await consultaMetricasDashboardCasoDeUso.ExecutarAsync(cancellationToken);

        var resposta = new RespostaSucessoApi<MetricasDashboardResposta>
        {
            Mensagem = "Metricas do dashboard obtidas com sucesso.",
            Dados = metricas,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
