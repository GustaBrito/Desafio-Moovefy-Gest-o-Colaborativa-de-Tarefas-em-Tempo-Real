using GerenciadorTarefas.Api.Configuracoes;
using Serilog;

var construtorAplicacao = WebApplication.CreateBuilder(args);
construtorAplicacao.AdicionarObservabilidade();

construtorAplicacao.Services.AdicionarInjecaoDependencia(construtorAplicacao.Configuration);
construtorAplicacao.Services.AdicionarDocumentacaoApi();
construtorAplicacao.Services.AdicionarTratamentoExcecaoGlobal();
construtorAplicacao.Services.AdicionarPadraoRespostaApi();
construtorAplicacao.Services.AdicionarValidacaoEntrada();
construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

aplicacao.UseSerilogRequestLogging();
aplicacao.UsarTratamentoExcecaoGlobal();
aplicacao.UsarDocumentacaoApi();
aplicacao.UseHttpsRedirection();
aplicacao.UseAuthorization();
aplicacao.MapControllers();

aplicacao.Run();
