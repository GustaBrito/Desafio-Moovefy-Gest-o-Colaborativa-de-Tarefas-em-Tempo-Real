using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

public sealed class AtualizarUsuarioEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; init; }
    public bool Ativo { get; init; } = true;
    public string? NovaSenha { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
}
