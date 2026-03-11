using GerenciadorTarefas.Aplicacao.Contratos.Dashboard;
using GerenciadorTarefas.Aplicacao.Modelos.Dashboard;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Dashboard;

public sealed class ConsultaMetricasDashboardCasoDeUso : IConsultaMetricasDashboardCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;

    public ConsultaMetricasDashboardCasoDeUso(IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioTarefa = repositorioTarefa;
    }

    public async Task<MetricasDashboardResposta> ExecutarAsync(
        IReadOnlyCollection<Guid>? areaIdsPermitidas = null,
        CancellationToken cancellationToken = default)
    {
        var tarefas = areaIdsPermitidas is null
            ? await repositorioTarefa.ListarTodasAsync(cancellationToken)
            : await repositorioTarefa.ListarTodasPorAreasAsync(areaIdsPermitidas, cancellationToken);
        var dataReferencia = DateTime.UtcNow;

        var totaisPorStatus = Enum
            .GetValues<StatusTarefa>()
            .Select(status => new TotalTarefasPorStatusResposta
            {
                Status = status,
                Total = tarefas.Count(tarefa => tarefa.Status == status)
            })
            .ToList();

        var totalTarefas = tarefas.Count;
        var totalConcluidas = tarefas.Count(tarefa => tarefa.Status == StatusTarefa.Concluida);
        var tarefasAtrasadas = tarefas.Count(tarefa => tarefa.EstaAtrasada(dataReferencia));
        var tarefasConcluidasNoPrazo = tarefas.Count(tarefa =>
            tarefa.Status == StatusTarefa.Concluida
            && tarefa.DataConclusao.HasValue
            && tarefa.DataConclusao.Value.Date <= tarefa.DataPrazo.Date);

        var taxaConclusao = totalTarefas == 0
            ? 0
            : Math.Round((totalConcluidas * 100m) / totalTarefas, 2, MidpointRounding.AwayFromZero);

        return new MetricasDashboardResposta
        {
            TotalTarefasPorStatus = totaisPorStatus,
            TarefasAtrasadas = tarefasAtrasadas,
            TarefasConcluidasNoPrazo = tarefasConcluidasNoPrazo,
            TaxaConclusao = taxaConclusao
        };
    }
}
