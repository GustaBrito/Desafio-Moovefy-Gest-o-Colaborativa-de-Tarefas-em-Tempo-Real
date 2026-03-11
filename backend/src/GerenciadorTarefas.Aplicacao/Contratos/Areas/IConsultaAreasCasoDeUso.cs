using GerenciadorTarefas.Aplicacao.Modelos.Areas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Areas;

public interface IConsultaAreasCasoDeUso
{
    Task<IReadOnlyCollection<AreaResposta>> ListarAsync(
        bool somenteAtivas = false,
        CancellationToken cancellationToken = default);
    Task<AreaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
