using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class ConsultaTarefasCasoDeUso : IConsultaTarefasCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;

    public ConsultaTarefasCasoDeUso(IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioTarefa = repositorioTarefa;
    }

    public async Task<IReadOnlyCollection<TarefaResposta>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var tarefas = await repositorioTarefa.ListarAsync(cancellationToken);
        var dataReferencia = DateTime.UtcNow;

        return tarefas
            .Select(tarefa => MapearParaResposta(tarefa, dataReferencia))
            .ToList();
    }

    public async Task<TarefaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
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

        return MapearParaResposta(tarefa, DateTime.UtcNow);
    }

    private static TarefaResposta MapearParaResposta(Tarefa tarefa, DateTime dataReferencia)
    {
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
            EstaAtrasada = tarefa.EstaAtrasada(dataReferencia)
        };
    }
}
