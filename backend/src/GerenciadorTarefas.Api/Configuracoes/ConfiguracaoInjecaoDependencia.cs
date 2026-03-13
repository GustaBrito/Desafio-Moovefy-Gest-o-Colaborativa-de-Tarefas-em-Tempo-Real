using GerenciadorTarefas.Aplicacao.CasosDeUso.Dashboard;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Notificacoes;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Areas;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;
using GerenciadorTarefas.Aplicacao.Contratos.Areas;
using GerenciadorTarefas.Aplicacao.Contratos.Dashboard;
using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Contratos.Seguranca;
using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Api.Modelos.Autenticacao;
using GerenciadorTarefas.Api.Servicos.Autenticacao;
using GerenciadorTarefas.Api.Servicos.Cache;
using GerenciadorTarefas.Api.Servicos.Notificacoes;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Infraestrutura.Persistencia;
using GerenciadorTarefas.Infraestrutura.Repositorios;
using GerenciadorTarefas.Infraestrutura.Seguranca;
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

        servicos.AddMemoryCache();
        servicos.Configure<ConfiguracaoSegurancaLogin>(
            configuracao.GetSection(ConfiguracaoSegurancaLogin.NomeSecao));

        servicos.AddSingleton<IServicoCacheConsulta, ServicoCacheConsultaMemoria>();
        servicos.AddSingleton<IServicoHashSenha, ServicoHashSenhaPbkdf2>();
        servicos.AddSingleton<IControleTentativasLogin, ControleTentativasLoginMemoria>();
        servicos.AddScoped<IServicoAutenticacao, ServicoAutenticacaoJwt>();
        servicos.AddScoped<INotificadorTempoRealTarefas, NotificadorTempoRealTarefasSignalR>();
        servicos.AddScoped<IRepositorioArea, RepositorioArea>();
        servicos.AddScoped<IRepositorioNotificacao, RepositorioNotificacao>();
        servicos.AddScoped<IRepositorioProjeto, RepositorioProjeto>();
        servicos.AddScoped<IRepositorioTarefa, RepositorioTarefa>();
        servicos.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        servicos.AddScoped<IRepositorioUsuarioArea, RepositorioUsuarioArea>();
        servicos.AddScoped<IConsultaAreasCasoDeUso, ConsultaAreasCasoDeUso>();
        servicos.AddScoped<ICriarAreaCasoDeUso, CriarAreaCasoDeUso>();
        servicos.AddScoped<IAtualizarAreaCasoDeUso, AtualizarAreaCasoDeUso>();
        servicos.AddScoped<IConsultaUsuariosCasoDeUso, ConsultaUsuariosCasoDeUso>();
        servicos.AddScoped<ICriarUsuarioCasoDeUso, CriarUsuarioCasoDeUso>();
        servicos.AddScoped<IAtualizarUsuarioCasoDeUso, AtualizarUsuarioCasoDeUso>();
        servicos.AddScoped<IAlterarStatusUsuarioCasoDeUso, AlterarStatusUsuarioCasoDeUso>();
        servicos.AddScoped<IConsultaHistoricoNotificacoesCasoDeUso, ConsultaHistoricoNotificacoesCasoDeUso>();
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
