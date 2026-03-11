using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;

internal static class MapeadorUsuarioResposta
{
    public static async Task<UsuarioResposta> MapearAsync(
        Usuario usuario,
        IReadOnlyCollection<Guid>? areaIdsCache,
        ILookup<Guid, Area> areasPorId,
        Func<Guid, CancellationToken, Task<IReadOnlyCollection<Guid>>> obterAreaIdsUsuarioAsync,
        CancellationToken cancellationToken)
    {
        var areaIds = areaIdsCache ?? await obterAreaIdsUsuarioAsync(usuario.Id, cancellationToken);
        var nomesAreas = areaIds
            .SelectMany(areaId => areasPorId[areaId])
            .Select(area => area.Nome)
            .Distinct()
            .OrderBy(nome => nome)
            .ToList();

        return new UsuarioResposta
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            PerfilGlobal = usuario.PerfilGlobal,
            Ativo = usuario.Ativo,
            DataCriacao = usuario.DataCriacao,
            UltimoAcesso = usuario.UltimoAcesso,
            AreaIds = areaIds.ToList(),
            AreaNomes = nomesAreas
        };
    }
}
