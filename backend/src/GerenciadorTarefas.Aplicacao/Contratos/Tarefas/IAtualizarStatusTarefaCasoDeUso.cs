using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Tarefas;

public interface IAtualizarStatusTarefaCasoDeUso
{
    Task<TarefaResposta> ExecutarAsync(
        Guid id,
        AtualizarStatusTarefaEntrada entrada,
        CancellationToken cancellationToken = default);
}
