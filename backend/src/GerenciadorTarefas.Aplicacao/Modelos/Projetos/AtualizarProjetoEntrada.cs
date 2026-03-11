namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class AtualizarProjetoEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid AreaId { get; init; }
    public Guid? GestorUsuarioId { get; init; }
}
