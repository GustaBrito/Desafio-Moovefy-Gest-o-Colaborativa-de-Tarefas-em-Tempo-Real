using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Tarefas;

public interface ICriarTarefaCasoDeUso
{
    Task<TarefaResposta> ExecutarAsync(
        CriarTarefaEntrada entrada,
        CancellationToken cancellationToken = default);
}
