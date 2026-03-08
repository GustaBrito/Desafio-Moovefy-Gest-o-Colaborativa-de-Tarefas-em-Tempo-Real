using GerenciadorTarefas.Aplicacao.CasosDeUso.Dashboard;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Contratos.Dashboard;
using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Api.Servicos.Notificacoes;
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

        servicos.AddScoped<INotificadorTempoRealTarefas, NotificadorTempoRealTarefasSignalR>();
        servicos.AddScoped<IRepositorioProjeto, RepositorioProjeto>();
        servicos.AddScoped<IRepositorioTarefa, RepositorioTarefa>();
        servicos.AddScoped<IConsultaMetricasDashboardCasoDeUso, ConsultaMetricasDashboardCasoDeUso>();
        servicos.AddScoped<IAtualizarProjetoCasoDeUso, AtualizarProjetoCasoDeUso>();
        servicos.AddScoped<ICriarProjetoCasoDeUso, CriarProjetoCasoDeUso>();
        servicos.AddScoped<IConsultaProjetosCasoDeUso, ConsultaProjetosCasoDeUso>();
        servicos.AddScoped<IExcluirProjetoCasoDeUso, ExcluirProjetoCasoDeUso>();
        servicos.AddScoped<ICriarTarefaCasoDeUso, CriarTarefaCasoDeUso>();
        servicos.AddScoped<IAtualizarTarefaCasoDeUso, AtualizarTarefaCasoDeUso>();
        servicos.AddScoped<IAtualizarStatusTarefaCasoDeUso, AtualizarStatusTarefaCasoDeUso>();
        servicos.AddScoped<IConsultaTarefasCasoDeUso, ConsultaTarefasCasoDeUso>();
        servicos.AddScoped<IExcluirTarefaCasoDeUso, ExcluirTarefaCasoDeUso>();

        return servicos;
    }
}
