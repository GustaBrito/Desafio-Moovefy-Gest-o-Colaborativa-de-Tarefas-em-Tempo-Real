using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioArea
{
    Task<IReadOnlyCollection<Area>> ListarAsync(
        bool somenteAtivas = false,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Area>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    Task<Area?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Area?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<Area?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Area area, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    void Atualizar(Area area);
}
