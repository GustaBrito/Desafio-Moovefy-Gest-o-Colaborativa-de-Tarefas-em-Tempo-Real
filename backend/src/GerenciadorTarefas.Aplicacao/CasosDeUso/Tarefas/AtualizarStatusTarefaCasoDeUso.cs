using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class AtualizarStatusTarefaCasoDeUso : IAtualizarStatusTarefaCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;

    public AtualizarStatusTarefaCasoDeUso(IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioTarefa = repositorioTarefa;
    }

    public async Task<TarefaResposta> ExecutarAsync(
        Guid id,
        AtualizarStatusTarefaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da tarefa deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Status))
        {
            throw new ArgumentException("O status informado e invalido.", nameof(entrada));
        }

        var tarefa = await repositorioTarefa.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null)
        {
            throw new KeyNotFoundException($"Tarefa com id '{id}' nao foi encontrada.");
        }

        tarefa.Status = entrada.Status;

        repositorioTarefa.Atualizar(tarefa);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);

        return new TarefaResposta
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Status = tarefa.Status,
            Prioridade = tarefa.Prioridade,
            ProjetoId = tarefa.ProjetoId,
            ResponsavelId = tarefa.ResponsavelId,
            DataCriacao = tarefa.DataCriacao,
            DataPrazo = tarefa.DataPrazo,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(DateTime.UtcNow)
        };
    }
}
