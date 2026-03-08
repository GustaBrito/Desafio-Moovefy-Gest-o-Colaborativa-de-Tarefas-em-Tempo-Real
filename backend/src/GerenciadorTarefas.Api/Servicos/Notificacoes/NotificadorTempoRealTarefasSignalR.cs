using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GerenciadorTarefas.Api.Servicos.Notificacoes;

public sealed class NotificadorTempoRealTarefasSignalR : INotificadorTempoRealTarefas
{
    private readonly IHubContext<HubNotificacoes> contextoHubNotificacoes;

    public NotificadorTempoRealTarefasSignalR(IHubContext<HubNotificacoes> contextoHubNotificacoes)
    {
        this.contextoHubNotificacoes = contextoHubNotificacoes;
    }

    public async Task NotificarAtribuicaoAsync(
        Guid responsavelId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default)
    {
        if (responsavelId == Guid.Empty || tarefaId == Guid.Empty || projetoId == Guid.Empty)
        {
            return;
        }

        var tituloNormalizado = tituloTarefa?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tituloNormalizado))
        {
            tituloNormalizado = "Tarefa sem titulo";
        }

        var canalResponsavel = HubNotificacoes.ObterNomeCanalResponsavel(responsavelId);
        var mensagem = reatribuicao
            ? "Uma tarefa foi reatribuida para voce."
            : "Uma nova tarefa foi atribuida para voce.";

        await contextoHubNotificacoes.Clients
            .Group(canalResponsavel)
            .SendAsync(
                "tarefaAtribuida",
                new
                {
                    TarefaId = tarefaId,
                    ProjetoId = projetoId,
                    ResponsavelId = responsavelId,
                    TituloTarefa = tituloNormalizado,
                    Reatribuicao = reatribuicao,
                    Mensagem = mensagem,
                    DataOcorrencia = DateTime.UtcNow
                },
                cancellationToken);
    }
}
