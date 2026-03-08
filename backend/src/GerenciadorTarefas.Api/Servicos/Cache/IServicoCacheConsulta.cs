namespace GerenciadorTarefas.Api.Servicos.Cache;

public interface IServicoCacheConsulta
{
    Task<TValor> ObterOuCriarAsync<TValor>(
        string chave,
        TimeSpan expiracao,
        Func<CancellationToken, Task<TValor>> fabrica,
        CancellationToken cancellationToken = default);

    void Remover(string chave);

    int RemoverPorPrefixo(string prefixo);
}
