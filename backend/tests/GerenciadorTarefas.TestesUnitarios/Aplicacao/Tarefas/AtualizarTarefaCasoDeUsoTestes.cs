using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class AtualizarTarefaCasoDeUsoTestes
{
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly NotificadorTempoRealTarefasFalso notificador = new();
    private readonly AtualizarTarefaCasoDeUso casoDeUso;

    public AtualizarTarefaCasoDeUsoTestes()
    {
        casoDeUso = new AtualizarTarefaCasoDeUso(repositorioTarefa, notificador);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty, CriarEntradaValida(Guid.NewGuid()));

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoEntradaForNula()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTarefaNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), CriarEntradaValida(Guid.NewGuid()));

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloForInvalido()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = CriarEntradaValida(tarefa.ResponsavelId);
        entrada = new AtualizarTarefaEntrada
        {
            Titulo = " ",
            Descricao = entrada.Descricao,
            Status = entrada.Status,
            Prioridade = entrada.Prioridade,
            ResponsavelId = entrada.ResponsavelId,
            DataPrazo = entrada.DataPrazo
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloExcederLimite()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = new string('x', 201),
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDescricaoExcederLimite()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = new string('x', 2001),
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoStatusForInvalido()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = (StatusTarefa)999,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoPrioridadeForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = (PrioridadeTarefa)999,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoResponsavelForVazio()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = Guid.Empty,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDataPrazoForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.MinValue
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTransicaoStatusForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.Concluida,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelId = tarefa.ResponsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveAtualizarSemNotificar_QuandoResponsavelNaoMudar()
    {
        var responsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavel);
        repositorioTarefa.Tarefas.Add(tarefa);

        var resposta = await casoDeUso.ExecutarAsync(tarefa.Id, CriarEntradaValida(responsavel));

        resposta.Titulo.Should().Be("Tarefa atualizada");
        resposta.Status.Should().Be(StatusTarefa.EmAndamento);
        repositorioTarefa.TarefaAtualizada.Should().BeSameAs(tarefa);
        repositorioTarefa.SalvarAlteracoesFoiChamado.Should().BeTrue();
        notificador.NotificacoesEnviadas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_DeveNotificar_QuandoResponsavelMudar()
    {
        var responsavelAnterior = Guid.NewGuid();
        var novoResponsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavelAnterior);
        repositorioTarefa.Tarefas.Add(tarefa);

        await casoDeUso.ExecutarAsync(tarefa.Id, CriarEntradaValida(novoResponsavel));

        notificador.NotificacoesEnviadas.Should().ContainSingle();
        notificador.NotificacoesEnviadas[0].ResponsavelId.Should().Be(novoResponsavel);
        notificador.NotificacoesEnviadas[0].Reatribuicao.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarAsync_DeveDefinirDescricaoNula_QuandoDescricaoForEspacos()
    {
        var responsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavel);
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo novo",
            Descricao = "   ",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Alta,
            ResponsavelId = responsavel,
            DataPrazo = DateTime.UtcNow.AddDays(2)
        };

        var resposta = await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        resposta.Descricao.Should().BeNull();
        tarefa.Descricao.Should().BeNull();
    }

    private static AtualizarTarefaEntrada CriarEntradaValida(Guid responsavelId)
    {
        return new AtualizarTarefaEntrada
        {
            Titulo = "  Tarefa atualizada  ",
            Descricao = "  Descricao nova  ",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Urgente,
            ResponsavelId = responsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(4)
        };
    }

    private static Tarefa CriarTarefa(StatusTarefa status, Guid responsavelId)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Titulo original",
            Descricao = "Descricao original",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = responsavelId,
            DataCriacao = DateTime.UtcNow.AddDays(-3),
            DataPrazo = DateTime.UtcNow.AddDays(5)
        };
    }
}
