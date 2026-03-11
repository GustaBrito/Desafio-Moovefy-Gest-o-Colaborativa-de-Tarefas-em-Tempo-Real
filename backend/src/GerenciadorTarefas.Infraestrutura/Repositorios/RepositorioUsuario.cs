using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Repositorios;

public sealed class RepositorioUsuario : IRepositorioUsuario
{
    private readonly ContextoBancoDados contextoBancoDados;

    public RepositorioUsuario(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyCollection<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Usuario>> ListarPorAreasAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        if (areaIds.Count == 0)
        {
            return [];
        }

        var consulta = contextoBancoDados.Usuarios
            .AsNoTracking()
            .Where(usuario =>
                contextoBancoDados.UsuariosAreas.Any(vinculo =>
                    vinculo.UsuarioId == usuario.Id
                    && areaIds.Contains(vinculo.AreaId)));

        if (somenteAtivos)
        {
            consulta = consulta.Where(usuario => usuario.Ativo);
        }

        return await consulta
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Usuario>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await contextoBancoDados.Usuarios
            .AsNoTracking()
            .Where(usuario => ids.Contains(usuario.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await contextoBancoDados.Usuarios
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        return await contextoBancoDados.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.Email.ToLower() == emailNormalizado,
                cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    public void Atualizar(Usuario usuario)
    {
        contextoBancoDados.Usuarios.Update(usuario);
    }
}
