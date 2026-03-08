using GerenciadorTarefas.Api.Modelos.Autenticacao;

namespace GerenciadorTarefas.Api.Servicos.Autenticacao;

public interface IServicoAutenticacao
{
    Task<ResultadoAutenticacao> AutenticarAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default);
}
