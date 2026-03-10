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
                ["BancoDados:AplicarSeedDadosDemonstracao"] = "false",
                ["AutenticacaoJwt:ChaveSecreta"] = "defina-uma-chave-jwt-segura-por-variavel-de-ambiente",
                ["AutenticacaoJwt:Emissor"] = "GerenciadorTarefas.Api",
                ["AutenticacaoJwt:Publico"] = "GerenciadorTarefas.Web",
                ["AutenticacaoJwt:ExpiracaoMinutos"] = "120",
                ["AutenticacaoJwt:Usuarios:0:Id"] = "8c519a4d-3f6d-4d0b-8b77-6ee8f5735990",
                ["AutenticacaoJwt:Usuarios:0:Nome"] = "Administrador",
                ["AutenticacaoJwt:Usuarios:0:Email"] = "admin@gerenciadortarefas.local",
                ["AutenticacaoJwt:Usuarios:0:Senha"] = "Admin@123",
                ["AutenticacaoJwt:Usuarios:0:Perfil"] = "Administrador",
                ["AutenticacaoJwt:Usuarios:1:Id"] = "f3af6b8c-58de-4225-a1d2-838b22f2d08e",
                ["AutenticacaoJwt:Usuarios:1:Nome"] = "Colaborador",
                ["AutenticacaoJwt:Usuarios:1:Email"] = "colaborador@gerenciadortarefas.local",
                ["AutenticacaoJwt:Usuarios:1:Senha"] = "Colaborador@123",
                ["AutenticacaoJwt:Usuarios:1:Perfil"] = "Colaborador"
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
