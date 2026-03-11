using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioUsuarioArea
{
    Task<IReadOnlyCollection<UsuarioArea>> ListarPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> ListarAreaIdsPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> ListarUsuarioIdsPorAreaIdsAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default);
    Task<bool> UsuarioPertenceAreaAsync(
        Guid usuarioId,
        Guid areaId,
        CancellationToken cancellationToken = default);
    Task RemoverPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AdicionarEmLoteAsync(
        IReadOnlyCollection<UsuarioArea> vinculos,
        CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
