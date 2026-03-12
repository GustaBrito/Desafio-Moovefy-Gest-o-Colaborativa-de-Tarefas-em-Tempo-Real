namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class ProjetoResposta
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid AreaId { get; init; }
    public string AreaNome { get; init; } = string.Empty;
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public IReadOnlyCollection<string> AreasNomes { get; init; } = [];
    public Guid? GestorUsuarioId { get; init; }
    public string? GestorNome { get; init; }
    public IReadOnlyCollection<Guid> UsuarioIdsVinculados { get; init; } = [];
    public IReadOnlyCollection<string> UsuariosNomesVinculados { get; init; } = [];
    public DateTime DataCriacao { get; init; }
}
