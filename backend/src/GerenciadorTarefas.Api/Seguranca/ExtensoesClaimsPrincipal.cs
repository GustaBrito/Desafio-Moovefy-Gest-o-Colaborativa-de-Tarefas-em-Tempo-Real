using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace GerenciadorTarefas.Api.Seguranca;

public static class ExtensoesClaimsPrincipal
{
    public static bool TentarObterUsuarioId(this ClaimsPrincipal? usuario, out Guid usuarioId)
    {
        usuarioId = Guid.Empty;
        if (usuario is null)
        {
            return false;
        }

        var valorUsuarioId = usuario.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? usuario.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? usuario.FindFirstValue("sub");

        return Guid.TryParse(valorUsuarioId, out usuarioId);
    }

    public static bool PossuiPerfilAdministrador(this ClaimsPrincipal? usuario)
    {
        return usuario?.IsInRole("Administrador") == true;
    }
}
