using System.Diagnostics;
using Serilog.Context;

namespace GerenciadorTarefas.Api.Servicos.Observabilidade;

public sealed class MiddlewareCorrelacaoObservabilidade
{
    private const string NomeCabecalhoCorrelacao = "X-Correlation-Id";
    private const string NomeCabecalhoTrace = "X-Trace-Id";
    private static readonly ActivitySource FonteAtividade = new("GerenciadorTarefas.Api");

    private readonly RequestDelegate proximo;

    public MiddlewareCorrelacaoObservabilidade(RequestDelegate proximo)
    {
        this.proximo = proximo;
    }

    public async Task InvokeAsync(
        HttpContext contexto,
        ServicoMetricasOperacionais servicoMetricasOperacionais)
    {
        var correlationId = ObterCorrelationId(contexto);
        var tempoInicio = Stopwatch.GetTimestamp();

        using var atividade = FonteAtividade.StartActivity(
            $"{contexto.Request.Method} {contexto.Request.Path}",
            ActivityKind.Server);

        var traceId = atividade?.TraceId.ToString() ?? contexto.TraceIdentifier;
        contexto.Response.Headers[NomeCabecalhoCorrelacao] = correlationId;
        contexto.Response.Headers[NomeCabecalhoTrace] = traceId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            try
            {
                atividade?.SetTag("http.method", contexto.Request.Method);
                atividade?.SetTag("http.route", contexto.Request.Path.Value ?? string.Empty);

                await proximo(contexto);
            }
            finally
            {
                var duracaoMs = Stopwatch.GetElapsedTime(tempoInicio).TotalMilliseconds;
                var statusCode = contexto.Response.StatusCode;
                var rota = contexto.Request.Path.Value ?? "desconhecida";

                atividade?.SetTag("http.status_code", statusCode);
                servicoMetricasOperacionais.RegistrarRequisicao(
                    contexto.Request.Method,
                    rota,
                    statusCode,
                    duracaoMs);
            }
        }
    }

    private static string ObterCorrelationId(HttpContext contexto)
    {
        var correlationId = contexto.Request.Headers[NomeCabecalhoCorrelacao].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        contexto.TraceIdentifier = correlationId;
        return correlationId;
    }
}
