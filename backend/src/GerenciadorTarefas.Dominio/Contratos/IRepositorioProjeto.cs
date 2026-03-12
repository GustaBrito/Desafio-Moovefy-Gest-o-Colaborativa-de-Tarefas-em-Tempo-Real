using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioProjeto
{
    Task<IReadOnlyCollection<Projeto>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsPermitidas = null,
        CancellationToken cancellationToken = default);
    Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Projeto>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    Task SincronizarAreasVinculadasAsync(
        Guid projetoId,
        IReadOnlyCollection<Guid> areaIds,
        CancellationToken cancellationToken = default);
    Task SincronizarUsuariosVinculadosAsync(
        Guid projetoId,
        IReadOnlyCollection<Guid> usuarioIds,
        CancellationToken cancellationToken = default);
    Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    void Atualizar(Projeto projeto);
    void Remover(Projeto projeto);
}
