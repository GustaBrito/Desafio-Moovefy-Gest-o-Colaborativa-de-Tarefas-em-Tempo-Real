using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GerenciadorTarefas.Dominio.Enumeracoes;

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

    public static bool TentarObterPerfilGlobal(
        this ClaimsPrincipal? usuario,
        out PerfilGlobalUsuario perfilGlobal)
    {
        perfilGlobal = default;
        if (usuario is null)
        {
            return false;
        }

        var valorPerfil = usuario.FindFirstValue(TiposClaimsAutenticacao.PerfilGlobal)
            ?? usuario.FindFirstValue(ClaimTypes.Role);

        return Enum.TryParse(valorPerfil, ignoreCase: true, out perfilGlobal);
    }

    public static bool PossuiPerfilSuperAdmin(this ClaimsPrincipal? usuario)
    {
        return usuario?.IsInRole(PerfilGlobalUsuario.SuperAdmin.ToString()) == true;
    }

    public static bool PossuiPerfilAdmin(this ClaimsPrincipal? usuario)
    {
        return usuario?.IsInRole(PerfilGlobalUsuario.Admin.ToString()) == true;
    }

    public static bool PossuiPerfilColaborador(this ClaimsPrincipal? usuario)
    {
        return usuario?.IsInRole(PerfilGlobalUsuario.Colaborador.ToString()) == true;
    }

    public static bool PossuiPerfilAdministrativo(this ClaimsPrincipal? usuario)
    {
        return usuario.PossuiPerfilSuperAdmin() || usuario.PossuiPerfilAdmin();
    }

    public static IReadOnlyCollection<Guid> ObterAreasIds(this ClaimsPrincipal? usuario)
    {
        if (usuario is null)
        {
            return [];
        }

        return usuario.Claims
            .Where(claim => claim.Type == TiposClaimsAutenticacao.AreaId)
            .Select(claim => Guid.TryParse(claim.Value, out var areaId) ? areaId : Guid.Empty)
            .Where(areaId => areaId != Guid.Empty)
            .Distinct()
            .ToList();
    }

    public static ContextoUsuarioAutenticado ObterContextoUsuarioAutenticado(this ClaimsPrincipal? usuario)
    {
        if (usuario is null || !usuario.TentarObterUsuarioId(out var usuarioId))
        {
            throw new UnauthorizedAccessException("Nao foi possivel identificar o usuario autenticado.");
        }

        if (!usuario.TentarObterPerfilGlobal(out var perfilGlobal))
        {
            throw new UnauthorizedAccessException("Nao foi possivel identificar o perfil do usuario autenticado.");
        }

        return new ContextoUsuarioAutenticado
        {
            UsuarioId = usuarioId,
            PerfilGlobal = perfilGlobal,
            AreaIds = usuario.ObterAreasIds()
        };
    }
}
