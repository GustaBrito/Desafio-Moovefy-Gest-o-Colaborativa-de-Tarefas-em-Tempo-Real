using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Api.Hubs;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.AspNetCore.SignalR;

namespace GerenciadorTarefas.Api.Servicos.Notificacoes;

public sealed class NotificadorTempoRealTarefasSignalR : INotificadorTempoRealTarefas
{
    private readonly IHubContext<HubNotificacoes> contextoHubNotificacoes;
    private readonly IRepositorioNotificacao repositorioNotificacao;

    public NotificadorTempoRealTarefasSignalR(
        IHubContext<HubNotificacoes> contextoHubNotificacoes,
        IRepositorioNotificacao repositorioNotificacao)
    {
        this.contextoHubNotificacoes = contextoHubNotificacoes;
        this.repositorioNotificacao = repositorioNotificacao;
    }

    public async Task NotificarAtribuicaoAsync(
        Guid responsavelUsuarioId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default)
    {
        if (responsavelUsuarioId == Guid.Empty || tarefaId == Guid.Empty || projetoId == Guid.Empty)
        {
            return;
        }

        var tituloNormalizado = tituloTarefa?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tituloNormalizado))
        {
            tituloNormalizado = "Tarefa sem titulo";
        }

        var canalResponsavel = HubNotificacoes.ObterNomeCanalResponsavel(responsavelUsuarioId);
        var mensagem = reatribuicao
            ? "Uma tarefa foi reatribuida para voce."
            : "Uma nova tarefa foi atribuida para voce.";
        var dataOcorrencia = DateTime.UtcNow;

        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            ResponsavelUsuarioId = responsavelUsuarioId,
            TarefaId = tarefaId,
            ProjetoId = projetoId,
            TituloTarefa = tituloNormalizado,
            Mensagem = mensagem,
            Reatribuicao = reatribuicao,
            DataCriacao = dataOcorrencia
        };

        await repositorioNotificacao.AdicionarAsync(notificacao, cancellationToken);
        await repositorioNotificacao.SalvarAlteracoesAsync(cancellationToken);

        await contextoHubNotificacoes.Clients
            .Group(canalResponsavel)
            .SendAsync(
                "tarefaAtribuida",
                new
                {
                    TarefaId = tarefaId,
                    ProjetoId = projetoId,
                    ResponsavelUsuarioId = responsavelUsuarioId,
                    TituloTarefa = tituloNormalizado,
                    Reatribuicao = reatribuicao,
                    Mensagem = mensagem,
                    DataOcorrencia = dataOcorrencia
                },
                cancellationToken);
    }
}
