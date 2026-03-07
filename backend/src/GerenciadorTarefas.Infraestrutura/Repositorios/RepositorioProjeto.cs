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

    public async Task<IReadOnlyCollection<Projeto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Projetos
            .AsNoTracking()
            .OrderBy(projeto => projeto.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Projetos
            .AsNoTracking()
            .FirstOrDefaultAsync(projeto => projeto.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Projetos.AddAsync(projeto, cancellationToken);
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
