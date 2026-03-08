using GerenciadorTarefas.Api.Configuracoes;
using GerenciadorTarefas.Api.Hubs;
using Serilog;

var construtorAplicacao = WebApplication.CreateBuilder(args);
construtorAplicacao.AdicionarObservabilidade();

construtorAplicacao.Services.AdicionarInjecaoDependencia(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarAutenticacaoJwt(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarDocumentacaoApi();
construtorAplicacao.Services.AdicionarTratamentoExcecaoGlobal();
construtorAplicacao.Services.AdicionarPadraoRespostaApi();
construtorAplicacao.Services.AdicionarValidacaoEntrada();
construtorAplicacao.Services.AddSignalR();
construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

aplicacao.UseSerilogRequestLogging();
aplicacao.UsarTratamentoExcecaoGlobal();
aplicacao.UsarDocumentacaoApi();
aplicacao.UseHttpsRedirection();
aplicacao.UseAuthentication();
aplicacao.UseAuthorization();
aplicacao.MapControllers();
aplicacao.MapHub<HubNotificacoes>("/hubs/notificacoes");

aplicacao.Run();
