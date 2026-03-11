namespace GerenciadorTarefas.Api.Modelos.Autenticacao;

public sealed class ConfiguracaoJwt
{
    public const string NomeSecao = "AutenticacaoJwt";

    public string ChaveSecreta { get; init; } = string.Empty;
    public string Emissor { get; init; } = string.Empty;
    public string Publico { get; init; } = string.Empty;
    public int ExpiracaoMinutos { get; init; } = 60;
}
