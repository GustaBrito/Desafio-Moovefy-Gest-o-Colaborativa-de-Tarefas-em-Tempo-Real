using GerenciadorTarefas.Aplicacao.Modelos.Areas;

namespace GerenciadorTarefas.Aplicacao.Contratos.Areas;

public interface ICriarAreaCasoDeUso
{
    Task<AreaResposta> ExecutarAsync(
        CriarAreaEntrada entrada,
        CancellationToken cancellationToken = default);
}
