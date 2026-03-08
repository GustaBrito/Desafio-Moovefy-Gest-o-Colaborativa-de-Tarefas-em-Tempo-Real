using GerenciadorTarefas.Api.Configuracoes;
using GerenciadorTarefas.Api.Hubs;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;
using Microsoft.EntityFrameworkCore;
using Serilog;

var construtorAplicacao = WebApplication.CreateBuilder(args);
construtorAplicacao.AdicionarObservabilidade();

construtorAplicacao.Services.AdicionarInjecaoDependencia(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarAutenticacaoJwt(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarCorsPadrao(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarLimitacaoTaxaPadrao(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarDocumentacaoApi();
construtorAplicacao.Services.AdicionarTratamentoExcecaoGlobal();
construtorAplicacao.Services.AdicionarPadraoRespostaApi();
construtorAplicacao.Services.AdicionarValidacaoEntrada();
construtorAplicacao.Services.AddSignalR();
construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

var aplicarMigracoesAutomaticamente = aplicacao.Configuration
    .GetValue<bool>("BancoDados:AplicarMigracoesAutomaticamente");

var aplicarSeedDadosDemonstracao = aplicacao.Configuration
    .GetValue<bool>("BancoDados:AplicarSeedDadosDemonstracao");

if (aplicarMigracoesAutomaticamente || aplicarSeedDadosDemonstracao)
{
    using var escopo = aplicacao.Services.CreateScope();
    var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();

    if (aplicarMigracoesAutomaticamente)
    {
        contextoBancoDados.Database.Migrate();
    }

    if (aplicarSeedDadosDemonstracao)
    {
        await SemeadorDadosDemonstracao.AplicarAsync(contextoBancoDados);
    }
}

aplicacao.UseSerilogRequestLogging();
aplicacao.UsarTratamentoExcecaoGlobal();
aplicacao.UsarDocumentacaoApi();
aplicacao.UseHttpsRedirection();
aplicacao.UseCors(ConfiguracaoCors.NomePoliticaCorsPadrao);
aplicacao.UseRateLimiter();
aplicacao.UseAuthentication();
aplicacao.UseAuthorization();
aplicacao.MapControllers();
aplicacao.MapHub<HubNotificacoes>("/hubs/notificacoes");

aplicacao.Run();
