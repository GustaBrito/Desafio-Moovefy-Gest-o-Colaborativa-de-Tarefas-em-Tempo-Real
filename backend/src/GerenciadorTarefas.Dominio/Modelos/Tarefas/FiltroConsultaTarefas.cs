using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Dominio.Modelos.Tarefas;

public sealed class FiltroConsultaTarefas
{
    public Guid? ProjetoId { get; init; }
    public StatusTarefa? Status { get; init; }
    public Guid? ResponsavelUsuarioId { get; init; }
    public IReadOnlyCollection<Guid>? AreasProjetoPermitidas { get; init; }
    public DateTime? DataPrazoInicial { get; init; }
    public DateTime? DataPrazoFinal { get; init; }
    public CampoOrdenacaoTarefa CampoOrdenacao { get; init; } = CampoOrdenacaoTarefa.DataCriacao;
    public DirecaoOrdenacaoTarefa DirecaoOrdenacao { get; init; } = DirecaoOrdenacaoTarefa.Descendente;
    public int Pular { get; init; }
    public int Tomar { get; init; } = 20;
}
