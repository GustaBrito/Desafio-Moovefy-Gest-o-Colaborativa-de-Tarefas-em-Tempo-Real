using GerenciadorTarefas.Infraestrutura.Persistencia;
using GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace GerenciadorTarefas.TestesIntegracao.Infraestrutura;

internal sealed class FabricaAplicacaoWebTeste : WebApplicationFactory<Program>
{
    private string? nomeBancoTeste;
    private string? connectionStringAdmin;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connectionStringAdmin = ResolverConnectionStringAdmin();
        nomeBancoTeste = $"gerenciador_tarefas_testes_{Guid.NewGuid():N}";

        CriarBancoTeste(connectionStringAdmin, nomeBancoTeste);
        var connectionStringBancoTeste = ConstruirConnectionStringBancoTeste(
            connectionStringAdmin,
            nomeBancoTeste);

        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuracao) =>
        {
            configuracao.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BancoDados"] = connectionStringBancoTeste,
                ["BancoDados:AplicarMigracoesAutomaticamente"] = "false",
                ["BancoDados:AplicarSeedDadosDemonstracao"] = "false",
                ["AutenticacaoJwt:ChaveSecreta"] = "defina-uma-chave-jwt-segura-por-variavel-de-ambiente",
                ["AutenticacaoJwt:Emissor"] = "GerenciadorTarefas.Api",
                ["AutenticacaoJwt:Publico"] = "GerenciadorTarefas.Web",
                ["AutenticacaoJwt:ExpiracaoMinutos"] = "120",
                ["AutenticacaoJwt:ExigirHttpsMetadata"] = "false",
                ["LimitacaoTaxa:Global:PermissoesPorJanela"] = "300",
                ["LimitacaoTaxa:Global:JanelaEmSegundos"] = "60",
                ["LimitacaoTaxa:Login:PermissoesPorJanela"] = "300",
                ["LimitacaoTaxa:Login:JanelaEmSegundos"] = "60",
                ["Seguranca:Login:MaxTentativasFalha"] = "3",
                ["Seguranca:Login:JanelaFalhasSegundos"] = "300",
                ["Seguranca:Login:DuracaoBloqueioSegundos"] = "600"
            });
        });

        builder.ConfigureServices(servicos =>
        {
            servicos.RemoveAll(typeof(DbContextOptions<ContextoBancoDados>));
            servicos.RemoveAll(typeof(ContextoBancoDados));

            servicos.AddDbContext<ContextoBancoDados>(opcoes =>
            {
                opcoes.UseNpgsql(connectionStringBancoTeste);
            });

            var provedor = servicos.BuildServiceProvider();

            using var escopo = provedor.CreateScope();
            var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();
            contextoBancoDados.Database.Migrate();
            SemeadorDadosDemonstracao.AplicarAsync(contextoBancoDados).GetAwaiter().GetResult();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing
            && !string.IsNullOrWhiteSpace(connectionStringAdmin)
            && !string.IsNullOrWhiteSpace(nomeBancoTeste))
        {
            try
            {
                ExcluirBancoTeste(connectionStringAdmin, nomeBancoTeste);
            }
            catch
            {
                // Nao falha o teste por erro de limpeza.
            }
        }

        base.Dispose(disposing);
    }

    private static string ResolverConnectionStringAdmin()
    {
        var connectionStringPorAmbiente = Environment.GetEnvironmentVariable(
            "TESTES_INTEGRACAO_CONNECTION_STRING_ADMIN");

        if (!string.IsNullOrWhiteSpace(connectionStringPorAmbiente))
        {
            if (!TentarConectar(connectionStringPorAmbiente))
            {
                throw new InvalidOperationException(
                    "Nao foi possivel conectar ao PostgreSQL da variavel TESTES_INTEGRACAO_CONNECTION_STRING_ADMIN.");
            }

            return connectionStringPorAmbiente;
        }

        var conexoesPadrao = new[]
        {
            "Host=localhost;Port=55433;Database=postgres;Username=postgres;Password=postgres;Pooling=false",
            "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres;Pooling=false"
        };

        foreach (var conexao in conexoesPadrao)
        {
            if (TentarConectar(conexao))
            {
                return conexao;
            }
        }

        throw new InvalidOperationException(
            "Nao foi possivel conectar ao PostgreSQL para os testes de integracao. " +
            "Configure TESTES_INTEGRACAO_CONNECTION_STRING_ADMIN ou inicie o banco local/container.");
    }

    private static bool TentarConectar(string connectionString)
    {
        try
        {
            using var conexao = new NpgsqlConnection(connectionString);
            conexao.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ConstruirConnectionStringBancoTeste(
        string connectionStringAdmin,
        string nomeBanco)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionStringAdmin)
        {
            Database = nomeBanco,
            Pooling = false
        };

        return builder.ConnectionString;
    }

    private static void CriarBancoTeste(string connectionStringAdmin, string nomeBanco)
    {
        using var conexao = new NpgsqlConnection(connectionStringAdmin);
        conexao.Open();

        using var comando = conexao.CreateCommand();
        comando.CommandText = $"CREATE DATABASE \"{nomeBanco}\"";
        comando.ExecuteNonQuery();
    }

    private static void ExcluirBancoTeste(string connectionStringAdmin, string nomeBanco)
    {
        using var conexao = new NpgsqlConnection(connectionStringAdmin);
        conexao.Open();

        using var comandoEncerrarConexoes = conexao.CreateCommand();
        comandoEncerrarConexoes.CommandText =
            """
            SELECT pg_terminate_backend(pid)
            FROM pg_stat_activity
            WHERE datname = @nomeBanco
              AND pid <> pg_backend_pid();
            """;
        comandoEncerrarConexoes.Parameters.AddWithValue("nomeBanco", nomeBanco);
        comandoEncerrarConexoes.ExecuteNonQuery();

        using var comandoExcluir = conexao.CreateCommand();
        comandoExcluir.CommandText = $"DROP DATABASE IF EXISTS \"{nomeBanco}\"";
        comandoExcluir.ExecuteNonQuery();
    }
}
