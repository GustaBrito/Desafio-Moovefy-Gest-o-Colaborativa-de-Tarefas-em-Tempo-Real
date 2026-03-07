using GerenciadorTarefas.Api.Configuracoes;

var construtorAplicacao = WebApplication.CreateBuilder(args);

construtorAplicacao.Logging.ClearProviders();
construtorAplicacao.Logging.AddConsole();

construtorAplicacao.Services.AdicionarInjecaoDependencia(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarDocumentacaoApi();
construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

aplicacao.UsarDocumentacaoApi();
aplicacao.UseHttpsRedirection();
aplicacao.UseAuthorization();
aplicacao.MapControllers();

aplicacao.Run();
