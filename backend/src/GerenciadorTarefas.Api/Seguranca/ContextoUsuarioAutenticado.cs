using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Api.Seguranca;

public sealed class ContextoUsuarioAutenticado
{
    public Guid UsuarioId { get; init; }
    public PerfilGlobalUsuario PerfilGlobal { get; init; }
    public IReadOnlyCollection<Guid> AreaIds { get; init; } = [];

    public bool EhSuperAdmin => PerfilGlobal == PerfilGlobalUsuario.SuperAdmin;
    public bool EhAdmin => PerfilGlobal == PerfilGlobalUsuario.Admin;
    public bool EhColaborador => PerfilGlobal == PerfilGlobalUsuario.Colaborador;
    public bool EhAdministrativo => EhSuperAdmin || EhAdmin;
}
