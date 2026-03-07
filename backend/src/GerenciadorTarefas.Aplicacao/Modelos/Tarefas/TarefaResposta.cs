using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

public sealed class TarefaResposta
{
    public Guid Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public StatusTarefa Status { get; init; }
    public PrioridadeTarefa Prioridade { get; init; }
    public Guid ProjetoId { get; init; }
    public Guid ResponsavelId { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime DataPrazo { get; init; }
    public DateTime? DataConclusao { get; init; }
    public bool EstaAtrasada { get; init; }
}
