namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class CriarProjetoEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid AreaId { get; init; }
    public Guid? GestorUsuarioId { get; init; }
    public Guid? CriadoPorUsuarioId { get; init; }
}
