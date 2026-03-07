using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

public sealed class FiltroConsultaTarefasEntrada
{
    public Guid? ProjetoId { get; init; }
    public StatusTarefa? Status { get; init; }
    public Guid? ResponsavelId { get; init; }
    public DateTime? DataPrazoInicial { get; init; }
    public DateTime? DataPrazoFinal { get; init; }
}
