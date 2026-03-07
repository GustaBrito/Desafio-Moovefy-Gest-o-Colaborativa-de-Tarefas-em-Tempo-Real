using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioTarefa
{
    Task<IReadOnlyCollection<Tarefa>> ListarAsync(CancellationToken cancellationToken = default);
    Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorProjetoIdAsync(Guid projetoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default);
    void Atualizar(Tarefa tarefa);
    void Remover(Tarefa tarefa);
}
