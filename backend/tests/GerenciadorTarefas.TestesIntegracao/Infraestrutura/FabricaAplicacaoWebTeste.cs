using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GerenciadorTarefas.TestesIntegracao.Infraestrutura;

internal sealed class FabricaAplicacaoWebTeste : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuracao) =>
        {
            configuracao.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BancoDados:AplicarMigracoesAutomaticamente"] = "false",
                ["BancoDados:AplicarSeedDadosDemonstracao"] = "false"
            });
        });

        builder.ConfigureServices(servicos =>
        {
            servicos.RemoveAll(typeof(DbContextOptions<ContextoBancoDados>));
            servicos.RemoveAll(typeof(ContextoBancoDados));

            var nomeBancoTeste = $"gerenciador_tarefas_testes_{Guid.NewGuid():N}";

            servicos.AddDbContext<ContextoBancoDados>(opcoes =>
            {
                opcoes.UseInMemoryDatabase(nomeBancoTeste);
            });

            var provedor = servicos.BuildServiceProvider();

            using var escopo = provedor.CreateScope();
            var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();
            contextoBancoDados.Database.EnsureDeleted();
            contextoBancoDados.Database.EnsureCreated();
        });
    }
}
