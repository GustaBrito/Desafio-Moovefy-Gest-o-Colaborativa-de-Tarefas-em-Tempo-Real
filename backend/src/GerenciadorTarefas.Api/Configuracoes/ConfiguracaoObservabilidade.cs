using System.Text.Json;
using GerenciadorTarefas.Api.Servicos.Observabilidade;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoObservabilidade
{
    public static WebApplicationBuilder AdicionarObservabilidade(this WebApplicationBuilder construtorAplicacao)
    {
        construtorAplicacao.Host.UseSerilog((contextoHospedagem, servicos, configuracaoSerilog) =>
        {
            configuracaoSerilog
                .ReadFrom.Configuration(contextoHospedagem.Configuration)
                .ReadFrom.Services(servicos)
                .Enrich.FromLogContext();
        });

        construtorAplicacao.Services.AddSingleton<ServicoMetricasOperacionais>();
        construtorAplicacao.Services.AddHealthChecks()
            .AddCheck("liveness", () => HealthCheckResult.Healthy("Aplicacao operacional."), tags: new[] { "live" })
            .AddCheck<VerificacaoSaudeBancoDados>("database", tags: new[] { "ready" });

        return construtorAplicacao;
    }

    public static WebApplication UsarCorrelacaoObservabilidade(this WebApplication aplicacao)
    {
        aplicacao.UseMiddleware<MiddlewareCorrelacaoObservabilidade>();
        return aplicacao;
    }

    public static WebApplication MapearHealthChecksObservabilidade(this WebApplication aplicacao)
    {
        aplicacao.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = verificacao => verificacao.Tags.Contains("live"),
            ResponseWriter = EscreverRespostaHealthCheck
        });

        aplicacao.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = verificacao => verificacao.Tags.Contains("ready"),
            ResponseWriter = EscreverRespostaHealthCheck
        });

        return aplicacao;
    }

    private static Task EscreverRespostaHealthCheck(HttpContext contexto, HealthReport relatorio)
    {
        contexto.Response.ContentType = "application/json";

        var payload = new
        {
            status = relatorio.Status.ToString(),
            totalDurationMs = Math.Round(relatorio.TotalDuration.TotalMilliseconds, 2),
            checks = relatorio.Entries.ToDictionary(
                item => item.Key,
                item => new
                {
                    status = item.Value.Status.ToString(),
                    description = item.Value.Description,
                    durationMs = Math.Round(item.Value.Duration.TotalMilliseconds, 2),
                    data = item.Value.Data
                })
        };

        return contexto.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
