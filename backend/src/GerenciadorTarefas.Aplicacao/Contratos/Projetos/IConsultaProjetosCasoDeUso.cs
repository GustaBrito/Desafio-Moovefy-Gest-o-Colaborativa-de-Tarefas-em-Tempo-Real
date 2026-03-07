using GerenciadorTarefas.Aplicacao.Modelos.Projetos;

namespace GerenciadorTarefas.Aplicacao.Contratos.Projetos;

public interface IConsultaProjetosCasoDeUso
{
    Task<IReadOnlyCollection<ProjetoResposta>> ListarAsync(CancellationToken cancellationToken = default);
    Task<ProjetoResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
