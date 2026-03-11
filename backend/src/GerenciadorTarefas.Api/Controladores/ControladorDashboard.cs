using GerenciadorTarefas.Aplicacao.Contratos.Dashboard;
using GerenciadorTarefas.Aplicacao.Modelos.Dashboard;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Seguranca;
using GerenciadorTarefas.Api.Servicos.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class ControladorDashboard : ControllerBase
{
    private readonly IConsultaMetricasDashboardCasoDeUso consultaMetricasDashboardCasoDeUso;
    private readonly IServicoCacheConsulta servicoCacheConsulta;

    public ControladorDashboard(
        IConsultaMetricasDashboardCasoDeUso consultaMetricasDashboardCasoDeUso,
        IServicoCacheConsulta servicoCacheConsulta)
    {
        this.consultaMetricasDashboardCasoDeUso = consultaMetricasDashboardCasoDeUso;
        this.servicoCacheConsulta = servicoCacheConsulta;
    }

    [HttpGet("metricas")]
    [ProducesResponseType(typeof(RespostaSucessoApi<MetricasDashboardResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<MetricasDashboardResposta>>> ObterMetricasAsync(
        CancellationToken cancellationToken)
    {
        var contextoUsuario = User.ObterContextoUsuarioAutenticado();
        var metricas = await servicoCacheConsulta.ObterOuCriarAsync(
            $"{ChavesCacheConsulta.ObterMetricasDashboard()}:u:{contextoUsuario.UsuarioId:N}",
            PoliticasCacheConsulta.DuracaoDashboard,
            _ => consultaMetricasDashboardCasoDeUso.ExecutarAsync(
                contextoUsuario.EhSuperAdmin ? null : contextoUsuario.AreaIds,
                cancellationToken),
            cancellationToken);

        var resposta = new RespostaSucessoApi<MetricasDashboardResposta>
        {
            Mensagem = "Metricas do dashboard obtidas com sucesso.",
            Dados = metricas,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
