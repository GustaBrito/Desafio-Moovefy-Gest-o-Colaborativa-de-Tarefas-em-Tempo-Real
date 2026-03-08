using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Dashboard;

public sealed class TotalTarefasPorStatusResposta
{
    public StatusTarefa Status { get; init; }
    public int Total { get; init; }
}
