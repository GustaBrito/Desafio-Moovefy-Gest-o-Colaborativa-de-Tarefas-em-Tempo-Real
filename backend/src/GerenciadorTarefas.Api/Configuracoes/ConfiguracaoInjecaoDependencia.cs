using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using GerenciadorTarefas.Infraestrutura.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoInjecaoDependencia
{
    public static IServiceCollection AdicionarInjecaoDependencia(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var stringConexao = configuracao.GetConnectionString("BancoDados")
            ?? throw new InvalidOperationException("A string de conexao 'BancoDados' nao foi configurada.");

        servicos.AddDbContext<ContextoBancoDados>(opcoes =>
            opcoes.UseNpgsql(stringConexao));

        servicos.AddScoped<IRepositorioProjeto, RepositorioProjeto>();
        servicos.AddScoped<IRepositorioTarefa, RepositorioTarefa>();
        servicos.AddScoped<IConsultaProjetosCasoDeUso, ConsultaProjetosCasoDeUso>();

        return servicos;
    }
}
