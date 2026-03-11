using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

namespace GerenciadorTarefas.Aplicacao.Contratos.Usuarios;

public interface IConsultaUsuariosCasoDeUso
{
    Task<IReadOnlyCollection<UsuarioResposta>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsEscopo = null,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default);
    Task<UsuarioResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
