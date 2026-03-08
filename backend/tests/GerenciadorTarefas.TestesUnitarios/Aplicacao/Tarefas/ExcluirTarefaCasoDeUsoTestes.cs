using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class ExcluirTarefaCasoDeUsoTestes
{
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly ExcluirTarefaCasoDeUso casoDeUso;

    public ExcluirTarefaCasoDeUsoTestes()
    {
        casoDeUso = new ExcluirTarefaCasoDeUso(repositorioTarefa);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTarefaNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveBloquearExclusao_QuandoStatusEmAndamento()
    {
        var tarefa = CriarTarefa(StatusTarefa.EmAndamento);
        repositorioTarefa.Tarefas.Add(tarefa);

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EmAndamento*");
    }

    [Fact]
    public async Task ExecutarAsync_DeveExcluirTarefa_QuandoStatusPermitir()
    {
        var tarefa = CriarTarefa(StatusTarefa.Cancelada);
        repositorioTarefa.Tarefas.Add(tarefa);

        await casoDeUso.ExecutarAsync(tarefa.Id);

        repositorioTarefa.TarefaRemovida.Should().BeSameAs(tarefa);
        repositorioTarefa.SalvarAlteracoesFoiChamado.Should().BeTrue();
    }

    private static Tarefa CriarTarefa(StatusTarefa status)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Tarefa de exclusao",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow.AddDays(-2),
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };
    }
}
