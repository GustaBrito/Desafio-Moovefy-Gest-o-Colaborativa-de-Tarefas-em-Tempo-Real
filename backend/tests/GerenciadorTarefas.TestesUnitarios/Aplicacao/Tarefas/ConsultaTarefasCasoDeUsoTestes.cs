using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class ConsultaTarefasCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly RepositorioUsuarioFalso repositorioUsuario = new();
    private readonly RepositorioAreaFalso repositorioArea = new();
    private readonly ConsultaTarefasCasoDeUso casoDeUso;

    public ConsultaTarefasCasoDeUsoTestes()
    {
        casoDeUso = new ConsultaTarefasCasoDeUso(
            repositorioTarefa,
            repositorioProjeto,
            repositorioUsuario,
            repositorioArea);
    }

    [Fact]
    public async Task ListarAsync_DeveListarComPaginacaoPadrao_QuandoFiltroNulo()
    {
        repositorioTarefa.ResultadoListagemSobrescrito = new ResultadoConsultaTarefas(
            [
                CriarTarefa(StatusTarefa.Pendente, DateTime.UtcNow.AddDays(-1))
            ],
            totalRegistros: 1);

        var resultado = await casoDeUso.ListarAsync();

        resultado.Itens.Should().HaveCount(1);
        resultado.NumeroPagina.Should().Be(1);
        resultado.TamanhoPagina.Should().Be(20);
        resultado.Itens.Single().EstaAtrasada.Should().BeTrue();
        repositorioTarefa.UltimoFiltroListagem.Should().NotBeNull();
        repositorioTarefa.UltimoFiltroListagem!.CampoOrdenacao.Should().Be(CampoOrdenacaoTarefa.DataCriacao);
        repositorioTarefa.UltimoFiltroListagem.DirecaoOrdenacao.Should().Be(DirecaoOrdenacaoTarefa.Descendente);
    }

    [Fact]
    public async Task ListarAsync_DeveLancarExcecao_QuandoIntervaloPrazoForInvalido()
    {
        var filtro = new FiltroConsultaTarefasEntrada
        {
            DataPrazoInicial = DateTime.UtcNow,
            DataPrazoFinal = DateTime.UtcNow.AddDays(-1)
        };

        var acao = async () => await casoDeUso.ListarAsync(filtro);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*data de prazo inicial*");
    }

    [Fact]
    public async Task ListarAsync_DeveLancarExcecao_QuandoProjetoIdForGuidVazio()
    {
        var filtro = new FiltroConsultaTarefasEntrada
        {
            ProjetoId = Guid.Empty
        };

        var acao = async () => await casoDeUso.ListarAsync(filtro);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador do projeto*");
    }

    [Fact]
    public async Task ListarAsync_DeveLancarExcecao_QuandoResponsavelUsuarioIdForGuidVazio()
    {
        var filtro = new FiltroConsultaTarefasEntrada
        {
            ResponsavelUsuarioId = Guid.Empty
        };

        var acao = async () => await casoDeUso.ListarAsync(filtro);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*identificador do responsavel*");
    }

    [Fact]
    public async Task ListarAsync_DeveReplicarFiltrosValidosNoRepositorio()
    {
        var projetoId = Guid.NewGuid();
        var ResponsavelUsuarioId = Guid.NewGuid();
        var dataInicial = DateTime.UtcNow.Date;
        var dataFinal = dataInicial.AddDays(3);

        repositorioTarefa.ResultadoListagemSobrescrito = new ResultadoConsultaTarefas([], 0);

        await casoDeUso.ListarAsync(new FiltroConsultaTarefasEntrada
        {
            ProjetoId = projetoId,
            Status = StatusTarefa.EmAndamento,
            ResponsavelUsuarioId = ResponsavelUsuarioId,
            DataPrazoInicial = dataInicial,
            DataPrazoFinal = dataFinal,
            CampoOrdenacao = CampoOrdenacaoTarefa.DataPrazo,
            DirecaoOrdenacao = DirecaoOrdenacaoTarefa.Ascendente,
            NumeroPagina = 2,
            TamanhoPagina = 15
        });

        repositorioTarefa.UltimoFiltroListagem.Should().NotBeNull();
        repositorioTarefa.UltimoFiltroListagem!.ProjetoId.Should().Be(projetoId);
        repositorioTarefa.UltimoFiltroListagem.Status.Should().Be(StatusTarefa.EmAndamento);
        repositorioTarefa.UltimoFiltroListagem.ResponsavelUsuarioId.Should().Be(ResponsavelUsuarioId);
        repositorioTarefa.UltimoFiltroListagem.DataPrazoInicial.Should().Be(dataInicial);
        repositorioTarefa.UltimoFiltroListagem.DataPrazoFinal.Should().Be(dataFinal);
        repositorioTarefa.UltimoFiltroListagem.CampoOrdenacao.Should().Be(CampoOrdenacaoTarefa.DataPrazo);
        repositorioTarefa.UltimoFiltroListagem.DirecaoOrdenacao.Should().Be(DirecaoOrdenacaoTarefa.Ascendente);
        repositorioTarefa.UltimoFiltroListagem.Pular.Should().Be(15);
        repositorioTarefa.UltimoFiltroListagem.Tomar.Should().Be(15);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ObterPorIdAsync(Guid.Empty);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoTarefaNaoExistir()
    {
        var acao = async () => await casoDeUso.ObterPorIdAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarTarefaMapeada()
    {
        var tarefa = CriarTarefa(StatusTarefa.EmAndamento, DateTime.UtcNow.AddDays(2));
        repositorioTarefa.Tarefas.Add(tarefa);

        var resposta = await casoDeUso.ObterPorIdAsync(tarefa.Id);

        resposta.Id.Should().Be(tarefa.Id);
        resposta.Status.Should().Be(StatusTarefa.EmAndamento);
    }

    private static Tarefa CriarTarefa(StatusTarefa status, DateTime dataPrazo)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Tarefa consulta",
            Status = status,
            Prioridade = PrioridadeTarefa.Alta,
            ProjetoId = Guid.NewGuid(),
            ResponsavelUsuarioId = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow.AddDays(-3),
            DataPrazo = dataPrazo
        };
    }
}
