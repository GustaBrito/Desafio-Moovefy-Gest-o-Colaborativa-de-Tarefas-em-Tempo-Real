using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using GerenciadorTarefas.Api.Seguranca;

namespace GerenciadorTarefas.Api.Hubs;

[Authorize]
public sealed class HubNotificacoes : Hub
{
    public async Task EntrarNoCanalResponsavelAsync(Guid responsavelId)
    {
        if (responsavelId == Guid.Empty)
        {
            return;
        }

        if (!Context.User.TentarObterUsuarioId(out var usuarioIdAutenticado))
        {
            throw new HubException("Nao foi possivel identificar o usuario autenticado.");
        }

        var usuarioAdministrador = Context.User.PossuiPerfilAdministrador();
        if (!usuarioAdministrador && usuarioIdAutenticado != responsavelId)
        {
            throw new HubException("Nao e permitido acessar o canal de outro responsavel.");
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

        if (!Context.User.TentarObterUsuarioId(out var usuarioIdAutenticado))
        {
            throw new HubException("Nao foi possivel identificar o usuario autenticado.");
        }

        var usuarioAdministrador = Context.User.PossuiPerfilAdministrador();
        if (!usuarioAdministrador && usuarioIdAutenticado != responsavelId)
        {
            throw new HubException("Nao e permitido acessar o canal de outro responsavel.");
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
