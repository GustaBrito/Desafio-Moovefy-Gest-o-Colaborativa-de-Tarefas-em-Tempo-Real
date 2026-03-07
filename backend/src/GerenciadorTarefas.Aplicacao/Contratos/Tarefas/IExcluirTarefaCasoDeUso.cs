namespace GerenciadorTarefas.Aplicacao.Contratos.Tarefas;

public interface IExcluirTarefaCasoDeUso
{
    Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default);
}
