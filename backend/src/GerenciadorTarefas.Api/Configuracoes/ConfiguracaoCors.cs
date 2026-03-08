namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoCors
{
    public const string NomePoliticaCorsPadrao = "politica_cors_padrao";

    public static IServiceCollection AdicionarCorsPadrao(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var origensPermitidas = configuracao
            .GetSection("Cors:OrigensPermitidas")
            .Get<string[]>()
            ?? Array.Empty<string>();

        servicos.AddCors(opcoes =>
        {
            opcoes.AddPolicy(NomePoliticaCorsPadrao, politica =>
            {
                if (origensPermitidas.Length == 0)
                {
                    politica
                        .WithOrigins(
                            "http://localhost:5173",
                            "https://localhost:5173",
                            "http://127.0.0.1:5173",
                            "https://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();

                    return;
                }

                politica
                    .WithOrigins(origensPermitidas)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return servicos;
    }
}
