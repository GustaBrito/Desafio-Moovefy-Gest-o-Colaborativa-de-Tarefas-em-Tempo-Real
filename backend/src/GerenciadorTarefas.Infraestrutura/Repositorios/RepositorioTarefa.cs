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

    public async Task<IReadOnlyCollection<Tarefa>> ListarAsync(
        FiltroConsultaTarefas filtroConsulta,
        CancellationToken cancellationToken = default)
    {
        var consulta = contextoBancoDados.Tarefas
            .AsNoTracking()
            .AsQueryable();

        if (filtroConsulta.ProjetoId.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.ProjetoId == filtroConsulta.ProjetoId.Value);
        }

        if (filtroConsulta.Status.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.Status == filtroConsulta.Status.Value);
        }

        if (filtroConsulta.ResponsavelId.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.ResponsavelId == filtroConsulta.ResponsavelId.Value);
        }

        if (filtroConsulta.DataPrazoInicial.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.DataPrazo >= filtroConsulta.DataPrazoInicial.Value);
        }

        if (filtroConsulta.DataPrazoFinal.HasValue)
        {
            consulta = consulta.Where(tarefa => tarefa.DataPrazo <= filtroConsulta.DataPrazoFinal.Value);
        }

        return await consulta
            .OrderByDescending(tarefa => tarefa.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Tarefas
            .AsNoTracking()
            .FirstOrDefaultAsync(tarefa => tarefa.Id == id, cancellationToken);
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
}
