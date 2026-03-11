using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Projetos;

public sealed class ConsultaProjetosCasoDeUsoTestes
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
    private readonly ConsultaProjetosCasoDeUso casoDeUso;

    public ConsultaProjetosCasoDeUsoTestes()
    {
        repositorioArea.Areas.Add(areaPadrao);
        casoDeUso = new ConsultaProjetosCasoDeUso(repositorioProjeto, repositorioArea, repositorioUsuario);
    }

    [Fact]
    public async Task ListarAsync_DeveMapearProjetos()
    {
        repositorioProjeto.Projetos.AddRange(
        [
            new Projeto
            {
                Id = Guid.NewGuid(),
                Nome = "Projeto 1",
                Descricao = "Descricao 1",
                AreaId = areaPadrao.Id,
                DataCriacao = DateTime.UtcNow.AddDays(-3)
            },
            new Projeto
            {
                Id = Guid.NewGuid(),
                Nome = "Projeto 2",
                AreaId = areaPadrao.Id,
                DataCriacao = DateTime.UtcNow.AddDays(-1)
            }
        ]);

        var resposta = await casoDeUso.ListarAsync();

        resposta.Should().HaveCount(2);
        resposta.Select(projeto => projeto.Nome).Should().Contain(["Projeto 1", "Projeto 2"]);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoIdVazio()
    {
        var acao = async () => await casoDeUso.ObterPorIdAsync(Guid.Empty);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoProjetoNaoEncontrado()
    {
        var acao = async () => await casoDeUso.ObterPorIdAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarProjetoMapeado()
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto detalhado",
            Descricao = "Descricao detalhada",
            AreaId = areaPadrao.Id,
            DataCriacao = DateTime.UtcNow.AddDays(-4)
        };
        repositorioProjeto.Projetos.Add(projeto);

        var resposta = await casoDeUso.ObterPorIdAsync(projeto.Id);

        resposta.Id.Should().Be(projeto.Id);
        resposta.Nome.Should().Be(projeto.Nome);
        resposta.Descricao.Should().Be(projeto.Descricao);
    }
}
