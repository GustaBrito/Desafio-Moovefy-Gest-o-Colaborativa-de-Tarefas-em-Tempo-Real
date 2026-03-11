namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class ProjetoResposta
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public Guid AreaId { get; init; }
    public string AreaNome { get; init; } = string.Empty;
    public Guid? GestorUsuarioId { get; init; }
    public string? GestorNome { get; init; }
    public DateTime DataCriacao { get; init; }
}
