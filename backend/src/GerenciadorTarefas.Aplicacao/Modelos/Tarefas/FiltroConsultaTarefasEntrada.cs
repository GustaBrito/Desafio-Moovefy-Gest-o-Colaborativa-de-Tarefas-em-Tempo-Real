using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

public sealed class FiltroConsultaTarefasEntrada
{
    public Guid? ProjetoId { get; init; }
    public StatusTarefa? Status { get; init; }
    public Guid? ResponsavelUsuarioId { get; init; }
    public IReadOnlyCollection<Guid>? AreasProjetoPermitidas { get; init; }
    public DateTime? DataPrazoInicial { get; init; }
    public DateTime? DataPrazoFinal { get; init; }
    public CampoOrdenacaoTarefa? CampoOrdenacao { get; init; }
    public DirecaoOrdenacaoTarefa? DirecaoOrdenacao { get; init; }
    public int NumeroPagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 20;
}
