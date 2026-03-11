using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Projetos;

public sealed class AtualizarProjetoCasoDeUsoTestes
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
    private readonly AtualizarProjetoCasoDeUso casoDeUso;

    public AtualizarProjetoCasoDeUsoTestes()
    {
        repositorioArea.Areas.Add(areaPadrao);
        casoDeUso = new AtualizarProjetoCasoDeUso(repositorioProjeto, repositorioArea, repositorioUsuario);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty, new AtualizarProjetoEntrada
        {
            Nome = "Projeto",
            AreaId = areaPadrao.Id
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
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoProjetoNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), new AtualizarProjetoEntrada
        {
            Nome = "Projeto",
            AreaId = areaPadrao.Id
        });

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoNomeForInvalido()
    {
        var projetoId = Guid.NewGuid();
        repositorioProjeto.Projetos.Add(new Projeto
        {
            Id = projetoId,
            Nome = "Atual",
            AreaId = areaPadrao.Id
        });

        var acao = async () => await casoDeUso.ExecutarAsync(projetoId, new AtualizarProjetoEntrada
        {
            Nome = " "
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoNomeExcederLimite()
    {
        var projetoId = Guid.NewGuid();
        repositorioProjeto.Projetos.Add(new Projeto
        {
            Id = projetoId,
            Nome = "Atual",
            AreaId = areaPadrao.Id
        });

        var acao = async () => await casoDeUso.ExecutarAsync(projetoId, new AtualizarProjetoEntrada
        {
            Nome = new string('x', 151)
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDescricaoExcederLimite()
    {
        var projetoId = Guid.NewGuid();
        repositorioProjeto.Projetos.Add(new Projeto
        {
            Id = projetoId,
            Nome = "Atual",
            AreaId = areaPadrao.Id
        });

        var acao = async () => await casoDeUso.ExecutarAsync(projetoId, new AtualizarProjetoEntrada
        {
            Nome = "Projeto valido",
            Descricao = new string('x', 1001),
            AreaId = areaPadrao.Id
        });

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveAtualizarProjetoComSucesso()
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto atual",
            Descricao = "Descricao anterior",
            AreaId = areaPadrao.Id,
            DataCriacao = DateTime.UtcNow.AddDays(-10)
        };
        repositorioProjeto.Projetos.Add(projeto);

        var resposta = await casoDeUso.ExecutarAsync(projeto.Id, new AtualizarProjetoEntrada
        {
            Nome = "  Projeto novo  ",
            Descricao = "  Nova descricao  ",
            AreaId = areaPadrao.Id
        });

        resposta.Id.Should().Be(projeto.Id);
        resposta.Nome.Should().Be("Projeto novo");
        resposta.Descricao.Should().Be("Nova descricao");
        repositorioProjeto.ProjetoAtualizado.Should().BeSameAs(projeto);
        repositorioProjeto.SalvarAlteracoesFoiChamado.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarAsync_DeveDefinirDescricaoNula_QuandoDescricaoForEspacos()
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto A",
            Descricao = "Descricao antiga",
            AreaId = areaPadrao.Id,
            DataCriacao = DateTime.UtcNow.AddDays(-2)
        };
        repositorioProjeto.Projetos.Add(projeto);

        var resposta = await casoDeUso.ExecutarAsync(projeto.Id, new AtualizarProjetoEntrada
        {
            Nome = "Projeto A Atualizado",
            Descricao = "   ",
            AreaId = areaPadrao.Id
        });

        resposta.Descricao.Should().BeNull();
        projeto.Descricao.Should().BeNull();
    }
}
