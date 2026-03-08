using Microsoft.AspNetCore.SignalR;

namespace GerenciadorTarefas.Api.Hubs;

public sealed class HubNotificacoes : Hub
{
    public async Task EntrarNoCanalResponsavelAsync(Guid responsavelId)
    {
        if (responsavelId == Guid.Empty)
        {
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ObterNomeCanalResponsavel(responsavelId));
    }

    public async Task SairDoCanalResponsavelAsync(Guid responsavelId)
    {
        if (responsavelId == Guid.Empty)
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            ObterNomeCanalResponsavel(responsavelId));
    }

    public static string ObterNomeCanalResponsavel(Guid responsavelId)
    {
        return $"responsavel:{responsavelId:D}";
    }
}
