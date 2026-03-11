using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioArea : IRepositorioArea
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioArea(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyCollection<Area>> ListarAsync(
        bool somenteAtivas = false,
        CancellationToken cancellationToken = default)
    {
        var consulta = contextoBancoDados.Areas
            .AsNoTracking()
            .AsQueryable();

        if (somenteAtivas)
        {
            consulta = consulta.Where(area => area.Ativa);
        }

        return await consulta
            .OrderBy(area => area.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Area>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await contextoBancoDados.Areas
            .AsNoTracking()
            .Where(area => ids.Contains(area.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Area?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Areas
            .FirstOrDefaultAsync(area => area.Id == id, cancellationToken);
    }

    public async Task<Area?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        var nomeNormalizado = nome.Trim();
        return await contextoBancoDados.Areas
            .FirstOrDefaultAsync(
                area => area.Nome.ToLower() == nomeNormalizado.ToLower(),
                cancellationToken);
    }

    public async Task<Area?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var codigoNormalizado = codigo.Trim();
        return await contextoBancoDados.Areas
            .FirstOrDefaultAsync(
                area => area.Codigo != null && area.Codigo.ToLower() == codigoNormalizado.ToLower(),
                cancellationToken);
    }

    public async Task AdicionarAsync(Area area, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Areas.AddAsync(area, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    public void Atualizar(Area area)
    {
        contextoBancoDados.Areas.Update(area);
    }
}
