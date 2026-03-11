using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioTarefa : IRepositorioTarefa
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioTarefa(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<ResultadoConsultaTarefas> ListarAsync(
        FiltroConsultaTarefas filtroConsulta,
        CancellationToken cancellationToken = default)
    {
        var consulta = contextoBancoDados.Tarefas
            .AsNoTracking()
            .AsQueryable();

        if (filtroConsulta.AreasProjetoPermitidas is not null)
        {
            if (filtroConsulta.AreasProjetoPermitidas.Count == 0)
            {
                return new ResultadoConsultaTarefas([], 0);
            }

            consulta = consulta.Where(tarefa =>
                contextoBancoDados.Projetos.Any(projeto =>
                    projeto.Id == tarefa.ProjetoId
                    && filtroConsulta.AreasProjetoPermitidas.Contains(projeto.AreaId)));
        }

        if (filtroConsulta.ProjetoId.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.ProjetoId == filtroConsulta.ProjetoId.Value);
        }

        if (filtroConsulta.Status.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.Status == filtroConsulta.Status.Value);
        }

        if (filtroConsulta.ResponsavelUsuarioId.HasValue)
        {
            consulta = consulta.Where(tarefa =>
                tarefa.ResponsavelUsuarioId == filtroConsulta.ResponsavelUsuarioId.Value);
        }

        if (filtroConsulta.DataPrazoInicial.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.DataPrazo >= filtroConsulta.DataPrazoInicial.Value);
        }

        if (filtroConsulta.DataPrazoFinal.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.DataPrazo <= filtroConsulta.DataPrazoFinal.Value);
        }

        var totalRegistros = await consulta.CountAsync(cancellationToken);
        consulta = AplicarOrdenacao(consulta, filtroConsulta);

        var tarefas = await consulta
            .Skip(filtroConsulta.Pular)
            .Take(filtroConsulta.Tomar)
            .ToListAsync(cancellationToken);

        return new ResultadoConsultaTarefas(tarefas, totalRegistros);
    }

    public async Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Tarefas
            .AsNoTracking()
            .FirstOrDefaultAsync(tarefa => tarefa.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tarefa>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Tarefas
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tarefa>> ListarTodasPorAreasAsync(
        IReadOnlyCollection<Guid> areaIds,
        CancellationToken cancellationToken = default)
    {
        if (areaIds.Count == 0)
        {
            return [];
        }

        return await contextoBancoDados.Tarefas
            .AsNoTracking()
            .Where(tarefa => contextoBancoDados.Projetos.Any(projeto =>
                projeto.Id == tarefa.ProjetoId
                && areaIds.Contains(projeto.AreaId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorProjetoIdAsync(Guid projetoId, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Tarefas
            .AsNoTracking()
            .AnyAsync(tarefa => tarefa.ProjetoId == projetoId, cancellationToken);
    }

    public async Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Tarefas.AddAsync(tarefa, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    public void Atualizar(Tarefa tarefa)
    {
        contextoBancoDados.Tarefas.Update(tarefa);
    }

    public void Remover(Tarefa tarefa)
    {
        contextoBancoDados.Tarefas.Remove(tarefa);
    }

    private static IQueryable<Tarefa> AplicarOrdenacao(
        IQueryable<Tarefa> consulta,
        FiltroConsultaTarefas filtroConsulta)
    {
        return (filtroConsulta.CampoOrdenacao, filtroConsulta.DirecaoOrdenacao) switch
        {
            (CampoOrdenacaoTarefa.Titulo, DirecaoOrdenacaoTarefa.Ascendente)
                => consulta.OrderBy(tarefa => tarefa.Titulo).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Titulo, DirecaoOrdenacaoTarefa.Descendente)
                => consulta.OrderByDescending(tarefa => tarefa.Titulo).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataPrazo, DirecaoOrdenacaoTarefa.Ascendente)
                => consulta.OrderBy(tarefa => tarefa.DataPrazo).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataPrazo, DirecaoOrdenacaoTarefa.Descendente)
                => consulta.OrderByDescending(tarefa => tarefa.DataPrazo).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Prioridade, DirecaoOrdenacaoTarefa.Ascendente)
                => consulta.OrderBy(tarefa => tarefa.Prioridade).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Prioridade, DirecaoOrdenacaoTarefa.Descendente)
                => consulta.OrderByDescending(tarefa => tarefa.Prioridade).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Status, DirecaoOrdenacaoTarefa.Ascendente)
                => consulta.OrderBy(tarefa => tarefa.Status).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.Status, DirecaoOrdenacaoTarefa.Descendente)
                => consulta.OrderByDescending(tarefa => tarefa.Status).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataCriacao, DirecaoOrdenacaoTarefa.Ascendente)
                => consulta.OrderBy(tarefa => tarefa.DataCriacao).ThenBy(tarefa => tarefa.Id),
            (CampoOrdenacaoTarefa.DataCriacao, DirecaoOrdenacaoTarefa.Descendente)
                => consulta.OrderByDescending(tarefa => tarefa.DataCriacao).ThenBy(tarefa => tarefa.Id),
            _ => consulta.OrderByDescending(tarefa => tarefa.DataCriacao).ThenBy(tarefa => tarefa.Id)
        };
    }
}
