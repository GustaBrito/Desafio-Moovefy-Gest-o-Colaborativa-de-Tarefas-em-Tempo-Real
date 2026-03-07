using GerenciadorTarefas.Aplicacao.Modelos.Projetos;

namespace GerenciadorTarefas.Aplicacao.Contratos.Projetos;

public interface IAtualizarProjetoCasoDeUso
{
    Task<ProjetoResposta> ExecutarAsync(
        Guid id,
        AtualizarProjetoEntrada entrada,
        CancellationToken cancellationToken = default);
}
