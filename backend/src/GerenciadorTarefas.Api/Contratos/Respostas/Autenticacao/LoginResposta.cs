using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Api.Contratos.Respostas.Autenticacao;

public sealed class LoginResposta
{
    public Guid UsuarioId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public string TokenAcesso { get; init; } = string.Empty;
    public string TipoToken { get; init; } = "Bearer";
    public DateTime ExpiraEmUtc { get; init; }
}
