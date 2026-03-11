using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using GerenciadorTarefas.Api.Seguranca;

namespace GerenciadorTarefas.Api.Hubs;

[Authorize]
public sealed class HubNotificacoes : Hub
{
    public override async Task OnConnectedAsync()
    {
        await EntrarNoCanalUsuarioAtualAsync();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await SairDoCanalUsuarioAtualAsync();
        await base.OnDisconnectedAsync(exception);
    }

    public async Task EntrarNoCanalUsuarioAtualAsync()
    {
        if (!Context.User.TentarObterUsuarioId(out var usuarioIdAutenticado))
        {
            throw new HubException("Nao foi possivel identificar o usuario autenticado.");
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            ObterNomeCanalResponsavel(usuarioIdAutenticado));

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ObterNomeCanalResponsavel(usuarioIdAutenticado));
    }

    public async Task SairDoCanalUsuarioAtualAsync()
    {
        if (!Context.User.TentarObterUsuarioId(out var usuarioIdAutenticado))
        {
            return;
        }

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            ObterNomeCanalResponsavel(usuarioIdAutenticado));
    }

    public static string ObterNomeCanalResponsavel(Guid responsavelId)
    {
        return $"responsavel:{responsavelId:D}";
    }
}
