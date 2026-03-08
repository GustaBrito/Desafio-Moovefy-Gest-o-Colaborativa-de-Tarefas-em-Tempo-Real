using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class AtualizarTarefaCasoDeUso : IAtualizarTarefaCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;
    private readonly INotificadorTempoRealTarefas notificadorTempoRealTarefas;

    public AtualizarTarefaCasoDeUso(
        IRepositorioTarefa repositorioTarefa,
        INotificadorTempoRealTarefas notificadorTempoRealTarefas)
    {
        this.repositorioTarefa = repositorioTarefa;
        this.notificadorTempoRealTarefas = notificadorTempoRealTarefas;
    }

    public async Task<TarefaResposta> ExecutarAsync(
        Guid id,
        AtualizarTarefaEntrada entrada,
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

        var tarefa = await repositorioTarefa.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null)
        {
            throw new KeyNotFoundException($"Tarefa com id '{id}' nao foi encontrada.");
        }

        var tituloNormalizado = entrada.Titulo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tituloNormalizado))
        {
            throw new ArgumentException("O titulo da tarefa deve ser informado.", nameof(entrada));
        }

        if (tituloNormalizado.Length > 200)
        {
            throw new ArgumentException("O titulo da tarefa deve ter no maximo 200 caracteres.", nameof(entrada));
        }

        var descricaoNormalizada = string.IsNullOrWhiteSpace(entrada.Descricao)
            ? null
            : entrada.Descricao.Trim();

        if (descricaoNormalizada is not null && descricaoNormalizada.Length > 2000)
        {
            throw new ArgumentException("A descricao da tarefa deve ter no maximo 2000 caracteres.", nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Status))
        {
            throw new ArgumentException("O status informado e invalido.", nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Prioridade))
        {
            throw new ArgumentException("A prioridade informada e invalida.", nameof(entrada));
        }

        if (entrada.ResponsavelId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do responsavel deve ser informado.", nameof(entrada));
        }

        if (entrada.DataPrazo <= DateTime.MinValue)
        {
            throw new ArgumentException("A data de prazo da tarefa deve ser informada.", nameof(entrada));
        }

        var dataAtual = DateTime.UtcNow;

        var responsavelAnterior = tarefa.ResponsavelId;

        tarefa.Titulo = tituloNormalizado;
        tarefa.Descricao = descricaoNormalizada;
        tarefa.AtualizarStatus(entrada.Status, dataAtual);
        tarefa.Prioridade = entrada.Prioridade;
        tarefa.ResponsavelId = entrada.ResponsavelId;
        tarefa.DataPrazo = entrada.DataPrazo;

        repositorioTarefa.Atualizar(tarefa);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);

        if (responsavelAnterior != tarefa.ResponsavelId)
        {
            await notificadorTempoRealTarefas.NotificarAtribuicaoAsync(
                tarefa.ResponsavelId,
                tarefa.Id,
                tarefa.ProjetoId,
                tarefa.Titulo,
                reatribuicao: true,
                cancellationToken);
        }

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
            EstaAtrasada = tarefa.EstaAtrasada(dataAtual)
        };
    }
}
