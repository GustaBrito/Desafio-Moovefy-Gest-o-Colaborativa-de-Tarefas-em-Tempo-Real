using System.Diagnostics.Metrics;

namespace GerenciadorTarefas.Api.Servicos.Observabilidade;

public sealed class ServicoMetricasOperacionais
{
    private readonly Meter meter = new("GerenciadorTarefas.Api.Operacional", "1.0.0");
    private readonly Counter<long> contadorRequisicoes;
    private readonly Counter<long> contadorFalhas;
    private readonly Histogram<double> histogramaDuracaoRequisicoesMs;
    private readonly DateTimeOffset inicioUtc = DateTimeOffset.UtcNow;

    private long totalRequisicoes;
    private long totalFalhas;
    private long duracaoAcumuladaMs;

    public ServicoMetricasOperacionais()
    {
        contadorRequisicoes = meter.CreateCounter<long>("api.requests.total");
        contadorFalhas = meter.CreateCounter<long>("api.requests.failed");
        histogramaDuracaoRequisicoesMs = meter.CreateHistogram<double>("api.requests.duration.ms");
    }

    public void RegistrarRequisicao(string metodoHttp, string rota, int statusCode, double duracaoMs)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("http.method", metodoHttp),
            new KeyValuePair<string, object?>("http.route", rota),
            new KeyValuePair<string, object?>("http.status_code", statusCode)
        };

        contadorRequisicoes.Add(1, tags);
        histogramaDuracaoRequisicoesMs.Record(duracaoMs, tags);

        Interlocked.Increment(ref totalRequisicoes);
        Interlocked.Add(ref duracaoAcumuladaMs, (long)Math.Round(duracaoMs));

        if (statusCode >= 500)
        {
            contadorFalhas.Add(1, tags);
            Interlocked.Increment(ref totalFalhas);
        }
    }

    public MetricasOperacionaisSnapshot ObterSnapshot()
    {
        var total = Interlocked.Read(ref totalRequisicoes);
        var falhas = Interlocked.Read(ref totalFalhas);
        var duracaoTotalMs = Interlocked.Read(ref duracaoAcumuladaMs);

        return new MetricasOperacionaisSnapshot
        {
            InicioUtc = inicioUtc,
            UptimeSegundos = (DateTimeOffset.UtcNow - inicioUtc).TotalSeconds,
            TotalRequisicoes = total,
            TotalFalhasServidor = falhas,
            TaxaFalhasServidor = total > 0 ? Math.Round((falhas * 100.0) / total, 3) : 0,
            DuracaoMediaRequisicaoMs = total > 0
                ? Math.Round((double)duracaoTotalMs / total, 3)
                : 0,
            MemoriaGerenciadaBytes = GC.GetTotalMemory(false),
            ThreadsWorkerDisponiveis = ObterThreadsDisponiveis()
        };
    }

    private static int ObterThreadsDisponiveis()
    {
        ThreadPool.GetAvailableThreads(out var worker, out _);
        return worker;
    }
}

public sealed class MetricasOperacionaisSnapshot
{
    public DateTimeOffset InicioUtc { get; init; }
    public double UptimeSegundos { get; init; }
    public long TotalRequisicoes { get; init; }
    public long TotalFalhasServidor { get; init; }
    public double TaxaFalhasServidor { get; init; }
    public double DuracaoMediaRequisicaoMs { get; init; }
    public long MemoriaGerenciadaBytes { get; init; }
    public int ThreadsWorkerDisponiveis { get; init; }
}
