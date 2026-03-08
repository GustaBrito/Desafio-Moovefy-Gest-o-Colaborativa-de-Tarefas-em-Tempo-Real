using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class CriarTarefaCasoDeUso : ICriarTarefaCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioTarefa repositorioTarefa;
    private readonly INotificadorTempoRealTarefas notificadorTempoRealTarefas;

    public CriarTarefaCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioTarefa repositorioTarefa,
        INotificadorTempoRealTarefas notificadorTempoRealTarefas)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioTarefa = repositorioTarefa;
        this.notificadorTempoRealTarefas = notificadorTempoRealTarefas;
    }

    public async Task<TarefaResposta> ExecutarAsync(
        CriarTarefaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        if (entrada.ProjetoId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do projeto deve ser informado.", nameof(entrada));
        }

        if (entrada.ResponsavelId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do responsavel deve ser informado.", nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Prioridade))
        {
            throw new ArgumentException("A prioridade informada e invalida.", nameof(entrada));
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

        if (entrada.DataPrazo <= DateTime.MinValue)
        {
            throw new ArgumentException("A data de prazo da tarefa deve ser informada.", nameof(entrada));
        }

        var projeto = await repositorioProjeto.ObterPorIdAsync(entrada.ProjetoId, cancellationToken);
        if (projeto is null)
        {
            throw new KeyNotFoundException($"Projeto com id '{entrada.ProjetoId}' nao foi encontrado.");
        }

        var dataAtual = DateTime.UtcNow;
        var tarefa = new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = tituloNormalizado,
            Descricao = descricaoNormalizada,
            Status = StatusTarefa.Pendente,
            Prioridade = entrada.Prioridade,
            ProjetoId = entrada.ProjetoId,
            ResponsavelId = entrada.ResponsavelId,
            DataCriacao = dataAtual,
            DataPrazo = entrada.DataPrazo,
            DataConclusao = null
        };

        await repositorioTarefa.AdicionarAsync(tarefa, cancellationToken);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);
        await notificadorTempoRealTarefas.NotificarAtribuicaoAsync(
            tarefa.ResponsavelId,
            tarefa.Id,
            tarefa.ProjetoId,
            tarefa.Titulo,
            reatribuicao: false,
            cancellationToken);

        return MapearParaResposta(tarefa, dataAtual);
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
