using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

public sealed class AtualizarTarefaEntrada
{
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public StatusTarefa Status { get; init; }
    public PrioridadeTarefa Prioridade { get; init; }
    public Guid ResponsavelId { get; init; }
    public DateTime DataPrazo { get; init; }
}
