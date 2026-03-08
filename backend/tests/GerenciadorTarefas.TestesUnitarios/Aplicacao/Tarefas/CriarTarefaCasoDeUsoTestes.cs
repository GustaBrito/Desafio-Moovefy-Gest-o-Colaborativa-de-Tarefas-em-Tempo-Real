using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class CriarTarefaCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly NotificadorTempoRealTarefasFalso notificador = new();
    private readonly CriarTarefaCasoDeUso casoDeUso;

    public CriarTarefaCasoDeUsoTestes()
    {
        casoDeUso = new CriarTarefaCasoDeUso(repositorioProjeto, repositorioTarefa, notificador);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoEntradaForNula()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoProjetoNaoExistir()
    {
        var entrada = CriarEntradaValida();

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoProjetoIdForVazio()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Titulo",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.Empty,
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(1)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador do projeto*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoResponsavelIdForVazio()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Titulo",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.Empty,
            DataPrazo = DateTime.UtcNow.AddDays(1)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador do responsavel*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoPrioridadeForInvalida()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Prioridade = (PrioridadeTarefa)999,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(2)
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*prioridade*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloForInvalido()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "   ",
            Prioridade = PrioridadeTarefa.Baixa,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(1)
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*titulo da tarefa*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloExcederLimite()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = new string('x', 201),
            Prioridade = PrioridadeTarefa.Baixa,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(1)
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*titulo da tarefa*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDescricaoExcederLimite()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = new string('x', 2001),
            Prioridade = PrioridadeTarefa.Baixa,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(1)
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*descricao da tarefa*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDataPrazoForInvalida()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Titulo",
            Prioridade = PrioridadeTarefa.Baixa,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.MinValue
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*data de prazo*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveCriarTarefaEEnviarNotificacao()
    {
        var entrada = CriarEntradaValida();
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var resposta = await casoDeUso.ExecutarAsync(entrada);

        resposta.Status.Should().Be(StatusTarefa.Pendente);
        resposta.Titulo.Should().Be("Implementar endpoint");
        repositorioTarefa.TarefaAdicionada.Should().NotBeNull();
        repositorioTarefa.SalvarAlteracoesFoiChamado.Should().BeTrue();
        notificador.NotificacoesEnviadas.Should().ContainSingle();
        notificador.NotificacoesEnviadas[0].Reatribuicao.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutarAsync_DevePersistirDescricaoNula_QuandoDescricaoForEspacos()
    {
        var entrada = new CriarTarefaEntrada
        {
            Titulo = "Tarefa sem descricao",
            Descricao = "   ",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(2)
        };
        repositorioProjeto.Projetos.Add(CriarProjeto(entrada.ProjetoId));

        var resposta = await casoDeUso.ExecutarAsync(entrada);

        resposta.Descricao.Should().BeNull();
        repositorioTarefa.TarefaAdicionada!.Descricao.Should().BeNull();
    }

    private static Projeto CriarProjeto(Guid projetoId)
    {
        return new Projeto
        {
            Id = projetoId,
            Nome = "Projeto principal",
            DataCriacao = DateTime.UtcNow.AddDays(-5)
        };
    }

    private static CriarTarefaEntrada CriarEntradaValida()
    {
        return new CriarTarefaEntrada
        {
            Titulo = "  Implementar endpoint  ",
            Descricao = "  Descricao valida  ",
            Prioridade = PrioridadeTarefa.Alta,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataPrazo = DateTime.UtcNow.AddDays(5)
        };
    }
}
