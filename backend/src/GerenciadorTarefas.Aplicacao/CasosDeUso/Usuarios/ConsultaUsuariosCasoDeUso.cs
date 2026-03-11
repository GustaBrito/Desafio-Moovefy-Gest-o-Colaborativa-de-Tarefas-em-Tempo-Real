using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;

public sealed class ConsultaUsuariosCasoDeUso : IConsultaUsuariosCasoDeUso
{
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly IRepositorioArea repositorioArea;

    public ConsultaUsuariosCasoDeUso(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioUsuarioArea repositorioUsuarioArea,
        IRepositorioArea repositorioArea)
    {
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioUsuarioArea = repositorioUsuarioArea;
        this.repositorioArea = repositorioArea;
    }

    public async Task<IReadOnlyCollection<UsuarioResposta>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsEscopo = null,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<GerenciadorTarefas.Dominio.Entidades.Usuario> usuarios;
        if (areaIdsEscopo is null)
        {
            usuarios = await repositorioUsuario.ListarAsync(cancellationToken);
            if (somenteAtivos)
            {
                usuarios = usuarios.Where(usuario => usuario.Ativo).ToList();
            }
        }
        else
        {
            usuarios = await repositorioUsuario.ListarPorAreasAsync(
                areaIdsEscopo,
                somenteAtivos,
                cancellationToken);
        }

        var areas = await repositorioArea.ListarAsync(cancellationToken: cancellationToken);
        var areasPorId = areas.ToLookup(area => area.Id);

        var respostas = new List<UsuarioResposta>(usuarios.Count);
        foreach (var usuario in usuarios.OrderBy(usuario => usuario.Nome))
        {
            var areaIdsUsuario = await repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync(
                usuario.Id,
                cancellationToken);

            respostas.Add(await MapeadorUsuarioResposta.MapearAsync(
                usuario,
                areaIdsUsuario,
                areasPorId,
                repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync,
                cancellationToken));
        }

        return respostas;
    }

    public async Task<UsuarioResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do usuario deve ser informado.", nameof(id));
        }

        var usuario = await repositorioUsuario.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException($"Usuario com id '{id}' nao foi encontrado.");
        }

        var areas = await repositorioArea.ListarAsync(cancellationToken: cancellationToken);
        var areasPorId = areas.ToLookup(area => area.Id);
        var areaIdsUsuario = await repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync(id, cancellationToken);

        return await MapeadorUsuarioResposta.MapearAsync(
            usuario,
            areaIdsUsuario,
            areasPorId,
            repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync,
            cancellationToken);
    }
}
