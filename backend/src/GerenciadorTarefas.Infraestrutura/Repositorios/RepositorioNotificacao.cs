using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioNotificacao : IRepositorioNotificacao
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioNotificacao(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyCollection<Notificacao>> ListarRecentesAsync(
        Guid? responsavelId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var limiteNormalizado = limite <= 0 ? 50 : limite;

        var consulta = contextoBancoDados.Notificacoes
            .AsNoTracking()
            .AsQueryable();

        if (responsavelId.HasValue)
        {
            consulta = consulta.Where(notificacao => notificacao.ResponsavelId == responsavelId.Value);
        }

        return await consulta
            .OrderByDescending(notificacao => notificacao.DataCriacao)
            .ThenByDescending(notificacao => notificacao.Id)
            .Take(limiteNormalizado)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Notificacoes.AddAsync(notificacao, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }
}
