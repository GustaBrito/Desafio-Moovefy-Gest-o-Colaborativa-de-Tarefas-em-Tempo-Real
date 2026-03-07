using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Tarefas;

public interface IAtualizarTarefaCasoDeUso
{
    Task<TarefaResposta> ExecutarAsync(
        Guid id,
        AtualizarTarefaEntrada entrada,
        CancellationToken cancellationToken = default);
}
