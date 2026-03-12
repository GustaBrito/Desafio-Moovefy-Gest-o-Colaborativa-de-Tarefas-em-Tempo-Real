namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class CriarProjetoEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public Guid? AreaIdLegado { get; init; }
    public IReadOnlyCollection<Guid> UsuarioIdsVinculados { get; init; } = [];
    public Guid? GestorUsuarioId { get; init; }
    public Guid? CriadoPorUsuarioId { get; init; }
}
