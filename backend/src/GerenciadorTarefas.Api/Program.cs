using GerenciadorTarefas.Infraestrutura.Persistencia;
using Microsoft.EntityFrameworkCore;

var construtorAplicacao = WebApplication.CreateBuilder(args);

construtorAplicacao.Logging.ClearProviders();
construtorAplicacao.Logging.AddConsole();

var stringConexao = construtorAplicacao.Configuration.GetConnectionString("BancoDados")
    ?? throw new InvalidOperationException("A string de conexão 'BancoDados' não foi configurada.");

construtorAplicacao.Services.AddDbContext<ContextoBancoDados>(opcoes =>
    opcoes.UseNpgsql(stringConexao));

construtorAplicacao.Services.AddControllers();

var aplicacao = construtorAplicacao.Build();

aplicacao.UseHttpsRedirection();
aplicacao.UseAuthorization();
aplicacao.MapControllers();

aplicacao.Run();
