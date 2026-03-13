using GerenciadorTarefas.Api.Modelos.Autenticacao;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GerenciadorTarefas.Api.Servicos.Autenticacao;

public sealed class ControleTentativasLoginMemoria : IControleTentativasLogin
{
    private readonly IMemoryCache cache;
    private readonly int maxTentativasFalha;
    private readonly TimeSpan janelaFalhas;
    private readonly TimeSpan duracaoBloqueio;

    public ControleTentativasLoginMemoria(
        IMemoryCache cache,
        IOptions<ConfiguracaoSegurancaLogin> opcoesConfiguracaoSegurancaLogin)
    {
        this.cache = cache;

        var configuracao = opcoesConfiguracaoSegurancaLogin.Value;
        maxTentativasFalha = configuracao.MaxTentativasFalha > 0
            ? configuracao.MaxTentativasFalha
            : 5;
        janelaFalhas = TimeSpan.FromSeconds(
            configuracao.JanelaFalhasSegundos > 0
                ? configuracao.JanelaFalhasSegundos
                : 300);
        duracaoBloqueio = TimeSpan.FromSeconds(
            configuracao.DuracaoBloqueioSegundos > 0
                ? configuracao.DuracaoBloqueioSegundos
                : 900);
    }

    public bool EstaBloqueado(string emailNormalizado, out TimeSpan tempoRestante)
    {
        tempoRestante = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(emailNormalizado))
        {
            return false;
        }

        var chave = ObterChaveCache(emailNormalizado);
        if (!cache.TryGetValue<EstadoTentativas>(chave, out var estado) || estado is null)
        {
            return false;
        }

        if (estado.BloqueadoAteUtc is null)
        {
            return false;
        }

        var agora = DateTimeOffset.UtcNow;
        if (estado.BloqueadoAteUtc <= agora)
        {
            cache.Remove(chave);
            return false;
        }

        tempoRestante = estado.BloqueadoAteUtc.Value - agora;
        return true;
    }

    public void RegistrarFalha(string emailNormalizado)
    {
        if (string.IsNullOrWhiteSpace(emailNormalizado))
        {
            return;
        }

        var chave = ObterChaveCache(emailNormalizado);
        var agora = DateTimeOffset.UtcNow;
        var estado = cache.Get<EstadoTentativas>(chave) ?? new EstadoTentativas
        {
            JanelaInicioUtc = agora,
            FalhasConsecutivas = 0
        };

        var janelaExpirada = agora - estado.JanelaInicioUtc > janelaFalhas;
        var bloqueioExpirado = estado.BloqueadoAteUtc is not null && estado.BloqueadoAteUtc <= agora;

        if (janelaExpirada || bloqueioExpirado)
        {
            estado = new EstadoTentativas
            {
                JanelaInicioUtc = agora,
                FalhasConsecutivas = 0
            };
        }

        estado.FalhasConsecutivas += 1;

        if (estado.FalhasConsecutivas >= maxTentativasFalha)
        {
            estado.BloqueadoAteUtc = agora.Add(duracaoBloqueio);
        }

        cache.Set(
            chave,
            estado,
            agora.Add(janelaFalhas > duracaoBloqueio ? janelaFalhas : duracaoBloqueio).Add(TimeSpan.FromMinutes(1)));
    }

    public void LimparFalhas(string emailNormalizado)
    {
        if (string.IsNullOrWhiteSpace(emailNormalizado))
        {
            return;
        }

        cache.Remove(ObterChaveCache(emailNormalizado));
    }

    private static string ObterChaveCache(string emailNormalizado)
    {
        return $"seguranca_login:{emailNormalizado}";
    }

    private sealed class EstadoTentativas
    {
        public DateTimeOffset JanelaInicioUtc { get; init; }
        public int FalhasConsecutivas { get; set; }
        public DateTimeOffset? BloqueadoAteUtc { get; set; }
    }
}
