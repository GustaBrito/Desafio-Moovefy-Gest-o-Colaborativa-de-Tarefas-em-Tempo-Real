namespace GerenciadorTarefas.Aplicacao.Modelos.Projetos;

public sealed class ProjetoResposta
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public DateTime DataCriacao { get; init; }
}
