namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoCors
{
    public const string NomePoliticaCorsPadrao = "politica_cors_padrao";

    public static IServiceCollection AdicionarCorsPadrao(
        this IServiceCollection servicos,
        IConfiguration configuracao,
        IHostEnvironment ambiente)
    {
        var origensPermitidasConfiguracao = configuracao
            .GetSection("Cors:OrigensPermitidas")
            .Get<string[]>()
            ?? Array.Empty<string>();

        var origensPermitidas = new HashSet<string>(
            origensPermitidasConfiguracao
                .Where(origem => !string.IsNullOrWhiteSpace(origem))
                .Select(origem => origem.Trim()),
            StringComparer.OrdinalIgnoreCase);

        if (origensPermitidas.Count == 0)
        {
            origensPermitidas.UnionWith(
            new[]
            {
                "http://localhost:5173",
                "https://localhost:5173",
                "http://127.0.0.1:5173",
                "https://127.0.0.1:5173"
            });
        }

        servicos.AddCors(opcoes =>
        {
            opcoes.AddPolicy(NomePoliticaCorsPadrao, politica =>
            {
                politica
                    .SetIsOriginAllowed(origem =>
                        origensPermitidas.Contains(origem) ||
                        (ambiente.IsDevelopment() && EhOrigemLocalDesenvolvimento(origem)))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return servicos;
    }

    private static bool EhOrigemLocalDesenvolvimento(string origem)
    {
        if (!Uri.TryCreate(origem, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            return false;
        }

        return uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
               uri.Host.Equals("::1", StringComparison.OrdinalIgnoreCase);
    }
}
