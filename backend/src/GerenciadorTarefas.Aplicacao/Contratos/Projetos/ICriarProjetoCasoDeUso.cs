using GerenciadorTarefas.Aplicacao.Modelos.Projetos;

namespace GerenciadorTarefas.Aplicacao.Contratos.Projetos;

public interface ICriarProjetoCasoDeUso
{
    Task<ProjetoResposta> ExecutarAsync(
        CriarProjetoEntrada entrada,
        CancellationToken cancellationToken = default);
}
