using GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Projetos;

public sealed class ExcluirProjetoCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly ExcluirProjetoCasoDeUso casoDeUso;

    public ExcluirProjetoCasoDeUsoTestes()
    {
        casoDeUso = new ExcluirProjetoCasoDeUso(repositorioProjeto, repositorioTarefa);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoProjetoNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoProjetoPossuirTarefas()
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto com tarefas"
        };
        repositorioProjeto.Projetos.Add(projeto);
        repositorioTarefa.ExistePorProjetoIdSobrescrito = true;

        var acao = async () => await casoDeUso.ExecutarAsync(projeto.Id);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*tarefas vinculadas*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveExcluirProjetoQuandoNaoHouverTarefas()
    {
        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = "Projeto sem tarefas"
        };
        repositorioProjeto.Projetos.Add(projeto);
        repositorioTarefa.ExistePorProjetoIdSobrescrito = false;

        await casoDeUso.ExecutarAsync(projeto.Id);

        repositorioProjeto.ProjetoRemovido.Should().BeSameAs(projeto);
        repositorioProjeto.SalvarAlteracoesFoiChamado.Should().BeTrue();
    }
}
