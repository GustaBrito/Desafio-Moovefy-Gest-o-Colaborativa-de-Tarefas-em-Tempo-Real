namespace GerenciadorTarefas.Api.Modelos.Autenticacao;

public sealed class UsuarioAutenticacaoConfiguracao
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public string Perfil { get; init; } = "Usuario";
}
