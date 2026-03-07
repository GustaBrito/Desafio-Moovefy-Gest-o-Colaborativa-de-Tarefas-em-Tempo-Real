using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioTarefa
{
    Task<ResultadoConsultaTarefas> ListarAsync(
        FiltroConsultaTarefas filtroConsulta,
        CancellationToken cancellationToken = default);
    Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorProjetoIdAsync(Guid projetoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    void Atualizar(Tarefa tarefa);
    void Remover(Tarefa tarefa);
}
