using System.Globalization;
using System.Threading.RateLimiting;
using GerenciadorTarefas.Api.Contratos.Respostas;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoRateLimiting
{
    public const string NomePoliticaRateLimitLogin = "limite_taxa_login";

    private const string SecaoLimitacaoTaxa = "LimitacaoTaxa";
    private const string CodigoErroLimiteTaxa = "limite_taxa_excedido";

    public static IServiceCollection AdicionarLimitacaoTaxaPadrao(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var regraGlobal = ObterRegra(configuracao, $"{SecaoLimitacaoTaxa}:Global", new RegraLimitacaoTaxa
        {
            PermissoesPorJanela = 120,
            JanelaEmSegundos = 60,
            LimiteFila = 0
        });

        var regraLogin = ObterRegra(configuracao, $"{SecaoLimitacaoTaxa}:Login", new RegraLimitacaoTaxa
        {
            PermissoesPorJanela = 5,
            JanelaEmSegundos = 60,
            LimiteFila = 0
        });

        servicos.AddRateLimiter(opcoes =>
        {
            opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opcoes.OnRejected = async (contexto, cancellationToken) =>
            {
                if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var intervaloEspera))
                {
                    var segundosEspera = Math.Ceiling(intervaloEspera.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);

                    contexto.HttpContext.Response.Headers.RetryAfter = segundosEspera;
                }

                var respostaErro = new RespostaErroApi
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Codigo = CodigoErroLimiteTaxa,
                    Mensagem = "Limite de requisicoes excedido.",
                    Detalhe = "Aguarde alguns instantes e tente novamente.",
                    CodigoRastreio = contexto.HttpContext.TraceIdentifier
                };

                contexto.HttpContext.Response.ContentType = "application/json";

                await contexto.HttpContext.Response.WriteAsJsonAsync(
                    respostaErro,
                    cancellationToken: cancellationToken);
            };

            opcoes.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ObterChaveParticao(contexto),
                    factory: _ => CriarOpcoesJanelaFixa(regraGlobal)));

            opcoes.AddPolicy(NomePoliticaRateLimitLogin, contexto =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ObterChaveParticao(contexto),
                    factory: _ => CriarOpcoesJanelaFixa(regraLogin)));
        });

        return servicos;
    }

    private static FixedWindowRateLimiterOptions CriarOpcoesJanelaFixa(RegraLimitacaoTaxa regra)
    {
        return new FixedWindowRateLimiterOptions
        {
            PermitLimit = regra.PermissoesPorJanela,
            Window = TimeSpan.FromSeconds(regra.JanelaEmSegundos),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = regra.LimiteFila,
            AutoReplenishment = true
        };
    }

    private static RegraLimitacaoTaxa ObterRegra(
        IConfiguration configuracao,
        string caminhoSecao,
        RegraLimitacaoTaxa valorPadrao)
    {
        var regra = configuracao.GetSection(caminhoSecao).Get<RegraLimitacaoTaxa>();

        if (regra is null)
        {
            return valorPadrao;
        }

        return new RegraLimitacaoTaxa
        {
            PermissoesPorJanela = regra.PermissoesPorJanela > 0
                ? regra.PermissoesPorJanela
                : valorPadrao.PermissoesPorJanela,
            JanelaEmSegundos = regra.JanelaEmSegundos > 0
                ? regra.JanelaEmSegundos
                : valorPadrao.JanelaEmSegundos,
            LimiteFila = regra.LimiteFila >= 0
                ? regra.LimiteFila
                : valorPadrao.LimiteFila
        };
    }

    private static string ObterChaveParticao(HttpContext contexto)
    {
        var origemReencaminhadaBruta = contexto.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var origemReencaminhada = origemReencaminhadaBruta?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(origemReencaminhada))
        {
            return origemReencaminhada;
        }

        var enderecoIp = contexto.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrWhiteSpace(enderecoIp))
        {
            return enderecoIp;
        }

        return "ip_desconhecido";
    }

    private sealed class RegraLimitacaoTaxa
    {
        public int PermissoesPorJanela { get; init; }
        public int JanelaEmSegundos { get; init; }
        public int LimiteFila { get; init; }
    }
}
