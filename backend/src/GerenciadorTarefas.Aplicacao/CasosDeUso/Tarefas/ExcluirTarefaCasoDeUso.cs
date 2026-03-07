using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class ExcluirTarefaCasoDeUso : IExcluirTarefaCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;

    public ExcluirTarefaCasoDeUso(IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioTarefa = repositorioTarefa;
    }

    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da tarefa deve ser informado.", nameof(id));
        }

        var tarefa = await repositorioTarefa.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null)
        {
            throw new KeyNotFoundException($"Tarefa com id '{id}' nao foi encontrada.");
        }

        if (tarefa.Status == StatusTarefa.EmAndamento)
        {
            throw new InvalidOperationException(
                "Nao e permitido excluir tarefa com status EmAndamento.");
        }

        repositorioTarefa.Remover(tarefa);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);
    }
}
