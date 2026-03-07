using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Tarefas;

public interface IConsultaTarefasCasoDeUso
{
    Task<IReadOnlyCollection<TarefaResposta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<TarefaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
