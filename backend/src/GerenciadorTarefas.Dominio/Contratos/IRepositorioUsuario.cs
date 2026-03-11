using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioUsuario
{
    Task<IReadOnlyCollection<Usuario>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Usuario>> ListarPorAreasAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Usuario>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    void Atualizar(Usuario usuario);
}
