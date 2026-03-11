using GerenciadorTarefas.Aplicacao.CasosDeUso.Notificacoes;
using GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Notificacoes;

public sealed class ConsultaHistoricoNotificacoesCasoDeUsoTestes
{
    private readonly RepositorioNotificacaoFalso repositorioNotificacao = new();
    private readonly ConsultaHistoricoNotificacoesCasoDeUso casoDeUso;

    public ConsultaHistoricoNotificacoesCasoDeUsoTestes()
    {
        casoDeUso = new ConsultaHistoricoNotificacoesCasoDeUso(repositorioNotificacao);
    }

    [Fact]
    public async Task ListarAsync_DeveLancarExcecao_QuandoResponsavelForGuidVazio()
    {
        var acao = async () => await casoDeUso.ListarAsync(new ConsultaHistoricoNotificacoesEntrada
        {
            ResponsavelUsuarioId = Guid.Empty
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListarAsync_DeveNormalizarLimite_QuandoValorInvalido()
    {
        await casoDeUso.ListarAsync(new ConsultaHistoricoNotificacoesEntrada
        {
            Limite = 0
        });

        repositorioNotificacao.UltimoLimiteConsulta.Should().Be(50);
    }

    [Fact]
    public async Task ListarAsync_DeveNormalizarLimiteMaximo()
    {
        await casoDeUso.ListarAsync(new ConsultaHistoricoNotificacoesEntrada
        {
            Limite = 500
        });

        repositorioNotificacao.UltimoLimiteConsulta.Should().Be(200);
    }

    [Fact]
    public async Task ListarAsync_DeveMapearNotificacoes()
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            ResponsavelUsuarioId = Guid.NewGuid(),
            TarefaId = Guid.NewGuid(),
            ProjetoId = Guid.NewGuid(),
            TituloTarefa = "Atribuicao",
            Mensagem = "Nova atribuicao de tarefa",
            Reatribuicao = false,
            DataCriacao = DateTime.UtcNow.AddMinutes(-10)
        };
        repositorioNotificacao.Notificacoes.Add(notificacao);

        var resposta = await casoDeUso.ListarAsync();

        resposta.Should().ContainSingle();
        resposta.Single().Mensagem.Should().Be(notificacao.Mensagem);
    }
}
