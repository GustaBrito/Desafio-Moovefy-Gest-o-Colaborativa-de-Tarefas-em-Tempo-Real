using GerenciadorTarefas.Aplicacao.Modelos.Dashboard;

namespace GerenciadorTarefas.Aplicacao.Contratos.Dashboard;

public interface IConsultaMetricasDashboardCasoDeUso
{
    Task<MetricasDashboardResposta> ExecutarAsync(CancellationToken cancellationToken = default);
}
