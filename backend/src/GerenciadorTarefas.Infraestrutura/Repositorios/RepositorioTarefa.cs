using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
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

    public async Task<IReadOnlyCollection<Tarefa>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Tarefas
            .AsNoTracking()
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

    public void Atualizar(Tarefa tarefa)
    {
        contextoBancoDados.Tarefas.Update(tarefa);
    }

    public void Remover(Tarefa tarefa)
    {
        contextoBancoDados.Tarefas.Remove(tarefa);
    }
}
