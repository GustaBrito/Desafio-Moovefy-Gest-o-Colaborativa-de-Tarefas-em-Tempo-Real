using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Dominio.Contratos;

public interface IRepositorioNotificacao
{
    Task<IReadOnlyCollection<Notificacao>> ListarRecentesAsync(
        Guid? responsavelUsuarioId,
        int limite,
        CancellationToken cancellationToken = default);
    Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
