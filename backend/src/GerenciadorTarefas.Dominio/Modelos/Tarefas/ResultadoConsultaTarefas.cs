using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Modelos.Tarefas;

public sealed class ResultadoConsultaTarefas
{
    public ResultadoConsultaTarefas(IReadOnlyCollection<Tarefa> itens, int totalRegistros)
    {
        Itens = itens;
        TotalRegistros = totalRegistros;
    }

    public IReadOnlyCollection<Tarefa> Itens { get; }
    public int TotalRegistros { get; }
}
