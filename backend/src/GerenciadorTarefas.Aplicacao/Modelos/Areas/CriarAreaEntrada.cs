namespace GerenciadorTarefas.Aplicacao.Modelos.Areas;

public sealed class CriarAreaEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string? Codigo { get; init; }
    public bool Ativa { get; init; } = true;
}
