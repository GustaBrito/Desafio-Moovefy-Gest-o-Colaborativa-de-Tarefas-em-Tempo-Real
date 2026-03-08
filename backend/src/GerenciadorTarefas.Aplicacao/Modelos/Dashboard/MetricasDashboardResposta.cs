namespace GerenciadorTarefas.Aplicacao.Modelos.Dashboard;

public sealed class MetricasDashboardResposta
{
    public IReadOnlyCollection<TotalTarefasPorStatusResposta> TotalTarefasPorStatus { get; init; }
        = Array.Empty<TotalTarefasPorStatusResposta>();
    public int TarefasAtrasadas { get; init; }
    public int TarefasConcluidasNoPrazo { get; init; }
    public decimal TaxaConclusao { get; init; }
}
