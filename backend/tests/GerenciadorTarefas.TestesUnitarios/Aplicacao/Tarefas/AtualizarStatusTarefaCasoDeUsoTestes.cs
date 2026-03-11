using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class AtualizarStatusTarefaCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly RepositorioUsuarioFalso repositorioUsuario = new();
    private readonly RepositorioAreaFalso repositorioArea = new();
    private readonly AtualizarStatusTarefaCasoDeUso casoDeUso;

    public AtualizarStatusTarefaCasoDeUsoTestes()
    {
        casoDeUso = new AtualizarStatusTarefaCasoDeUso(
            repositorioTarefa,
            repositorioProjeto,
            repositorioUsuario,
            repositorioArea);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty, new AtualizarStatusTarefaEntrada
        {
            Status = StatusTarefa.Pendente
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoEntradaForNula()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoStatusForInvalido()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), new AtualizarStatusTarefaEntrada
        {
            Status = (StatusTarefa)999
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTarefaNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), new AtualizarStatusTarefaEntrada
        {
            Status = StatusTarefa.EmAndamento
        });

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveAtualizarStatusParaConcluidaComDataConclusao()
    {
        var tarefa = CriarTarefa(StatusTarefa.EmAndamento);
        repositorioTarefa.Tarefas.Add(tarefa);

        var resposta = await casoDeUso.ExecutarAsync(tarefa.Id, new AtualizarStatusTarefaEntrada
        {
            Status = StatusTarefa.Concluida
        });

        resposta.Status.Should().Be(StatusTarefa.Concluida);
        resposta.DataConclusao.Should().NotBeNull();
        repositorioTarefa.TarefaAtualizada.Should().BeSameAs(tarefa);
        repositorioTarefa.SalvarAlteracoesFoiChamado.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoFluxoDeStatusForInvalido()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente);
        repositorioTarefa.Tarefas.Add(tarefa);

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, new AtualizarStatusTarefaEntrada
        {
            Status = StatusTarefa.Concluida
        });

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    private static Tarefa CriarTarefa(StatusTarefa status)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Tarefa teste",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelUsuarioId = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow.AddDays(-2),
            DataPrazo = DateTime.UtcNow.AddDays(2)
        };
    }
}
