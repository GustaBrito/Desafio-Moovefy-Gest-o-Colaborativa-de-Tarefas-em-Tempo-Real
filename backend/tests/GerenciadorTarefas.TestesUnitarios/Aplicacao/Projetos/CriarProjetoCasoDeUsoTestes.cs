using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Projetos;

public sealed class CriarProjetoCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioAreaFalso repositorioArea = new();
    private readonly RepositorioUsuarioFalso repositorioUsuario = new();
    private readonly Area areaPadrao = new()
    {
        Id = Guid.NewGuid(),
        Nome = "Area Teste",
        Ativa = true
    };
    private readonly CriarProjetoCasoDeUso casoDeUso;

    public CriarProjetoCasoDeUsoTestes()
    {
        repositorioArea.Areas.Add(areaPadrao);
        casoDeUso = new CriarProjetoCasoDeUso(repositorioProjeto, repositorioArea, repositorioUsuario);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoEntradaForNula()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoNomeForInvalido()
    {
        var entrada = new CriarProjetoEntrada
        {
            Nome = "   "
        };

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*nome do projeto*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoNomeExcederLimite()
    {
        var entrada = new CriarProjetoEntrada
        {
            Nome = new string('x', 151)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*nome do projeto*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDescricaoExcederLimite()
    {
        var entrada = new CriarProjetoEntrada
        {
            Nome = "Projeto A",
            Descricao = new string('x', 1001)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(entrada);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*descricao do projeto*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveCriarProjetoComDadosNormalizados()
    {
        var entrada = new CriarProjetoEntrada
        {
            Nome = "  Projeto Estrategico  ",
            Descricao = "  Descricao valida  ",
            AreaId = areaPadrao.Id
        };

        var resposta = await casoDeUso.ExecutarAsync(entrada);

        resposta.Nome.Should().Be("Projeto Estrategico");
        resposta.Descricao.Should().Be("Descricao valida");
        resposta.Id.Should().NotBeEmpty();
        repositorioProjeto.ProjetoAdicionado.Should().NotBeNull();
        repositorioProjeto.SalvarAlteracoesFoiChamado.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarAsync_DevePersistirDescricaoNula_QuandoDescricaoForEspacos()
    {
        var entrada = new CriarProjetoEntrada
        {
            Nome = "Projeto sem descricao",
            Descricao = "   ",
            AreaId = areaPadrao.Id
        };

        var resposta = await casoDeUso.ExecutarAsync(entrada);

        resposta.Descricao.Should().BeNull();
        repositorioProjeto.ProjetoAdicionado!.Descricao.Should().BeNull();
    }
}
