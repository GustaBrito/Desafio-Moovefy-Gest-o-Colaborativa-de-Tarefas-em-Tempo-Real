using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

public sealed class UsuarioResposta
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; init; }
    public bool Ativo { get; init; }
    public DateTime DataCriacao { get; init; }
    public DateTime? UltimoAcesso { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
    public IReadOnlyCollection<string> AreaNomes { get; init; } = [];
}
