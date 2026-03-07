namespace GerenciadorTarefas.Aplicacao.Contratos.Projetos;

public interface IExcluirProjetoCasoDeUso
{
    Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default);
}
