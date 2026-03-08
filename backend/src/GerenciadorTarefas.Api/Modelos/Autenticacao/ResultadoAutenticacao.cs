namespace GerenciadorTarefas.Api.Modelos.Autenticacao;

public sealed class ResultadoAutenticacao
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Perfil { get; init; } = string.Empty;
    public string TokenAcesso { get; init; } = string.Empty;
    public string TipoToken { get; init; } = "Bearer";
    public DateTime ExpiraEmUtc { get; init; }
}
