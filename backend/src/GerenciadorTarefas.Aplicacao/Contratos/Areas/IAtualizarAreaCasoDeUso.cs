using GerenciadorTarefas.Aplicacao.Modelos.Areas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Areas;

public interface IAtualizarAreaCasoDeUso
{
    Task<AreaResposta> ExecutarAsync(
        Guid id,
        AtualizarAreaEntrada entrada,
        CancellationToken cancellationToken = default);
}
