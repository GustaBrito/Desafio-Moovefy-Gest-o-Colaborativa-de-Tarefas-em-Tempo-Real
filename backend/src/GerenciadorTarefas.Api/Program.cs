var construtorAplicacao = WebApplication.CreateBuilder(args);

construtorAplicacao.Logging.ClearProviders();
construtorAplicacao.Logging.AddConsole();

construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

aplicacao.UseHttpsRedirection();
aplicacao.UseAuthorization();
aplicacao.MapControllers();

aplicacao.Run();
