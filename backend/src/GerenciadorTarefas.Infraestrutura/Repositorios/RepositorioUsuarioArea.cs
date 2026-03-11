using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioUsuarioArea : IRepositorioUsuarioArea
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioUsuarioArea(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyCollection<UsuarioArea>> ListarPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.UsuariosAreas
            .AsNoTracking()
            .Where(vinculo => vinculo.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListarAreaIdsPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.UsuariosAreas
            .AsNoTracking()
            .Where(vinculo => vinculo.UsuarioId == usuarioId)
            .Select(vinculo => vinculo.AreaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Guid>> ListarUsuarioIdsPorAreaIdsAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        if (areaIds.Count == 0)
        {
            return [];
        }

        var consulta = contextoBancoDados.UsuariosAreas
            .AsNoTracking()
            .Where(vinculo => areaIds.Contains(vinculo.AreaId));

        if (!somenteAtivos)
        {
            return await consulta
                .Select(vinculo => vinculo.UsuarioId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        return await consulta
            .Join(
                contextoBancoDados.Usuarios.AsNoTracking(),
                vinculo => vinculo.UsuarioId,
                usuario => usuario.Id,
                (vinculo, usuario) => new { vinculo.UsuarioId, usuario.Ativo })
            .Where(item => item.Ativo)
            .Select(item => item.UsuarioId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UsuarioPertenceAreaAsync(
        Guid usuarioId,
        Guid areaId,
        CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.UsuariosAreas
            .AsNoTracking()
            .AnyAsync(
                vinculo => vinculo.UsuarioId == usuarioId && vinculo.AreaId == areaId,
                cancellationToken);
    }

    public async Task RemoverPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var vinculos = await contextoBancoDados.UsuariosAreas
            .Where(vinculo => vinculo.UsuarioId == usuarioId)
            .ToListAsync(cancellationToken);

        contextoBancoDados.UsuariosAreas.RemoveRange(vinculos);
    }

    public async Task AdicionarEmLoteAsync(
        IReadOnlyCollection<UsuarioArea> vinculos,
        CancellationToken cancellationToken = default)
    {
        if (vinculos.Count == 0)
        {
            return;
        }

        await contextoBancoDados.UsuariosAreas.AddRangeAsync(vinculos, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }
}
