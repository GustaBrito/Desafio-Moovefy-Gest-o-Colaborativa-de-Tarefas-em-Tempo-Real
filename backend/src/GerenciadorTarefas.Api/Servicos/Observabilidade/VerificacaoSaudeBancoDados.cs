using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GerenciadorTarefas.Api.Servicos.Observabilidade;

public sealed class VerificacaoSaudeBancoDados : IHealthCheck
{
    private readonly ContextoBancoDados contextoBancoDados;

    public VerificacaoSaudeBancoDados(ContextoBancoDados contextoBancoDados)
    {
        this.contextoBancoDados = contextoBancoDados;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var conecta = await contextoBancoDados.Database.CanConnectAsync(cancellationToken);
        if (!conecta)
        {
            return HealthCheckResult.Unhealthy("Conexao com banco de dados indisponivel.");
        }

        var migracoesPendentes = (await contextoBancoDados.Database
            .GetPendingMigrationsAsync(cancellationToken))
            .ToList();

        if (migracoesPendentes.Count > 0)
        {
            return HealthCheckResult.Degraded(
                "Banco conectado, mas existem migracoes pendentes.",
                data: new Dictionary<string, object>
                {
                    ["migracoesPendentes"] = migracoesPendentes
                });
        }

        return HealthCheckResult.Healthy("Banco de dados operacional.");
    }
}
