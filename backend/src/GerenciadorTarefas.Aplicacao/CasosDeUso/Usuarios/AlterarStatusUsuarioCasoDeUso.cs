using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;

public sealed class AlterarStatusUsuarioCasoDeUso : IAlterarStatusUsuarioCasoDeUso
{
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly IRepositorioArea repositorioArea;

    public AlterarStatusUsuarioCasoDeUso(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioUsuarioArea repositorioUsuarioArea,
        IRepositorioArea repositorioArea)
    {
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioUsuarioArea = repositorioUsuarioArea;
        this.repositorioArea = repositorioArea;
    }

    public async Task<UsuarioResposta> ExecutarAsync(
        Guid id,
        AlterarStatusUsuarioEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do usuario deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var usuario = await repositorioUsuario.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException($"Usuario com id '{id}' nao foi encontrado.");
        }

        usuario.Ativo = entrada.Ativo;
        repositorioUsuario.Atualizar(usuario);
        await repositorioUsuario.SalvarAlteracoesAsync(cancellationToken);

        var areaIds = await repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync(id, cancellationToken);
        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);
        var areasPorId = areas.ToLookup(area => area.Id);

        return await MapeadorUsuarioResposta.MapearAsync(
            usuario,
            areaIds,
            areasPorId,
            repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync,
            cancellationToken);
    }
}
