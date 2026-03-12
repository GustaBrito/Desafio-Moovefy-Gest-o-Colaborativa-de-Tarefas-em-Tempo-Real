using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioProjeto : IRepositorioProjeto
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioProjeto(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyCollection<Projeto>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsPermitidas = null,
        CancellationToken cancellationToken = default)
    {
        var consulta = contextoBancoDados.Projetos
            .AsNoTracking()
            .Include(projeto => projeto.AreasVinculadas)
            .Include(projeto => projeto.UsuariosVinculados)
            .AsQueryable();

        if (areaIdsPermitidas is not null)
        {
            if (areaIdsPermitidas.Count == 0)
            {
                return [];
            }

            consulta = consulta.Where(projeto =>
                areaIdsPermitidas.Contains(projeto.AreaId)
                || projeto.AreasVinculadas.Any(vinculo => areaIdsPermitidas.Contains(vinculo.AreaId)));
        }

        return await consulta
            .OrderBy(projeto => projeto.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Projetos
            .AsNoTracking()
            .Include(projeto => projeto.AreasVinculadas)
            .Include(projeto => projeto.UsuariosVinculados)
            .FirstOrDefaultAsync(projeto => projeto.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Projeto>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await contextoBancoDados.Projetos
            .AsNoTracking()
            .Include(projeto => projeto.AreasVinculadas)
            .Include(projeto => projeto.UsuariosVinculados)
            .Where(projeto => ids.Contains(projeto.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task SincronizarAreasVinculadasAsync(
        Guid projetoId,
        IReadOnlyCollection<Guid> areaIds,
        CancellationToken cancellationToken = default)
    {
        var areaIdsNormalizados = areaIds.Distinct().ToHashSet();
        var vinculosExistentes = await contextoBancoDados.ProjetosAreas
            .Where(vinculo => vinculo.ProjetoId == projetoId)
            .ToListAsync(cancellationToken);

        var vinculosParaRemover = vinculosExistentes
            .Where(vinculo => !areaIdsNormalizados.Contains(vinculo.AreaId))
            .ToArray();
        if (vinculosParaRemover.Length > 0)
        {
            contextoBancoDados.ProjetosAreas.RemoveRange(vinculosParaRemover);
        }

        var areaIdsExistentes = vinculosExistentes
            .Select(vinculo => vinculo.AreaId)
            .ToHashSet();
        var vinculosParaAdicionar = areaIdsNormalizados
            .Where(areaId => !areaIdsExistentes.Contains(areaId))
            .Select(areaId => new ProjetoArea
            {
                ProjetoId = projetoId,
                AreaId = areaId
            })
            .ToArray();

        if (vinculosParaAdicionar.Length > 0)
        {
            await contextoBancoDados.ProjetosAreas.AddRangeAsync(vinculosParaAdicionar, cancellationToken);
        }
    }

    public async Task SincronizarUsuariosVinculadosAsync(
        Guid projetoId,
        IReadOnlyCollection<Guid> usuarioIds,
        CancellationToken cancellationToken = default)
    {
        var usuarioIdsNormalizados = usuarioIds.Distinct().ToHashSet();
        var vinculosExistentes = await contextoBancoDados.ProjetosUsuariosVinculados
            .Where(vinculo => vinculo.ProjetoId == projetoId)
            .ToListAsync(cancellationToken);

        var vinculosParaRemover = vinculosExistentes
            .Where(vinculo => !usuarioIdsNormalizados.Contains(vinculo.UsuarioId))
            .ToArray();
        if (vinculosParaRemover.Length > 0)
        {
            contextoBancoDados.ProjetosUsuariosVinculados.RemoveRange(vinculosParaRemover);
        }

        var usuarioIdsExistentes = vinculosExistentes
            .Select(vinculo => vinculo.UsuarioId)
            .ToHashSet();
        var vinculosParaAdicionar = usuarioIdsNormalizados
            .Where(usuarioId => !usuarioIdsExistentes.Contains(usuarioId))
            .Select(usuarioId => new ProjetoUsuarioVinculado
            {
                ProjetoId = projetoId,
                UsuarioId = usuarioId
            })
            .ToArray();

        if (vinculosParaAdicionar.Length > 0)
        {
            await contextoBancoDados.ProjetosUsuariosVinculados.AddRangeAsync(vinculosParaAdicionar, cancellationToken);
        }
    }

    public async Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Projetos.AddAsync(projeto, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    public void Atualizar(Projeto projeto)
    {
        contextoBancoDados.Projetos.Update(projeto);
    }

    public void Remover(Projeto projeto)
    {
        contextoBancoDados.Projetos.Remove(projeto);
    }
}
