namespace GerenciadorTarefas.Api.Modelos.Autenticacao;

public sealed class ConfiguracaoSegurancaLogin
{
    public const string NomeSecao = "Seguranca:Login";

    public int MaxTentativasFalha { get; init; } = 5;
    public int JanelaFalhasSegundos { get; init; } = 300;
    public int DuracaoBloqueioSegundos { get; init; } = 900;
}
