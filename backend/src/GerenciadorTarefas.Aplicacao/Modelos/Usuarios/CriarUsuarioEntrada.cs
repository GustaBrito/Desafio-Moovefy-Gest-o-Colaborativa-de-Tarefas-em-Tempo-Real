using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

public sealed class CriarUsuarioEntrada
{
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; init; }
    public bool Ativo { get; init; } = true;
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];
}
