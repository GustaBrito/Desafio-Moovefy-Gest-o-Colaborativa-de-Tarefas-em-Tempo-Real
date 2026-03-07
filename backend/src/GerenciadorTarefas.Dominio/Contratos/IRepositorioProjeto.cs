using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioProjeto
{
    Task<IReadOnlyCollection<Projeto>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    void Atualizar(Projeto projeto);
    void Remover(Projeto projeto);
}
