using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

public sealed class CriarTarefaEntrada
{
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public PrioridadeTarefa Prioridade { get; init; }
    public Guid ProjetoId { get; init; }
    public Guid ResponsavelUsuarioId { get; init; }
    public DateTime DataPrazo { get; init; }
}
