using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace GerenciadorTarefas.Api.Servicos.Cache;

public sealed class ServicoCacheConsultaMemoria : IServicoCacheConsulta
{
    private readonly IMemoryCache cache;
    private readonly ConcurrentDictionary<string, byte> indiceChaves = new();

    public ServicoCacheConsultaMemoria(IMemoryCache cache)
    {
        this.cache = cache;
    }

    public async Task<TValor> ObterOuCriarAsync<TValor>(
        string chave,
        TimeSpan expiracao,
        Func<CancellationToken, Task<TValor>> fabrica,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(chave, out TValor? valorCache)
            && valorCache is not null)
        {
            return valorCache;
        }

        var valor = await fabrica(cancellationToken);

        var opcoes = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiracao
        };

        opcoes.RegisterPostEvictionCallback((chaveRemovida, _, _, estado) =>
        {
            if (chaveRemovida is string chaveTexto
                && estado is ConcurrentDictionary<string, byte> indice)
            {
                indice.TryRemove(chaveTexto, out _);
            }
        }, indiceChaves);

        cache.Set(chave, valor, opcoes);
        indiceChaves.TryAdd(chave, 0);

        return valor;
    }

    public void Remover(string chave)
    {
        cache.Remove(chave);
        indiceChaves.TryRemove(chave, out _);
    }

    public int RemoverPorPrefixo(string prefixo)
    {
        var chavesParaRemover = indiceChaves.Keys
            .Where(chave => chave.StartsWith(prefixo, StringComparison.Ordinal))
            .ToArray();

        foreach (var chave in chavesParaRemover)
        {
            cache.Remove(chave);
            indiceChaves.TryRemove(chave, out _);
        }

        return chavesParaRemover.Length;
    }
}
