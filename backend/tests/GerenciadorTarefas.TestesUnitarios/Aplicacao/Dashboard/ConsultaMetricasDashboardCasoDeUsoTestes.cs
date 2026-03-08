using GerenciadorTarefas.Aplicacao.CasosDeUso.Dashboard;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Dashboard;

public sealed class ConsultaMetricasDashboardCasoDeUsoTestes
{
    [Fact]
    public async Task ExecutarAsync_DeveRetornarMetricasZeradas_QuandoNaoHaTarefas()
    {
        var repositorioTarefa = new RepositorioTarefaFalso();
        var casoDeUso = new ConsultaMetricasDashboardCasoDeUso(repositorioTarefa);

        var resposta = await casoDeUso.ExecutarAsync();

        resposta.TaxaConclusao.Should().Be(0);
        resposta.TarefasAtrasadas.Should().Be(0);
        resposta.TotalTarefasPorStatus.Should().HaveCount(Enum.GetValues<StatusTarefa>().Length);
    }

    [Fact]
    public async Task ExecutarAsync_DeveCalcularMetricasCorretamente()
    {
        var repositorioTarefa = new RepositorioTarefaFalso();
        var dataReferencia = DateTime.UtcNow;

        repositorioTarefa.Tarefas.AddRange(
        [
            CriarTarefa(StatusTarefa.Pendente, dataReferencia.AddDays(-1)),
            CriarTarefa(StatusTarefa.EmAndamento, dataReferencia.AddDays(2)),
            CriarTarefa(StatusTarefa.Concluida, dataReferencia.AddDays(-3), dataReferencia.AddDays(-4)),
            CriarTarefa(StatusTarefa.Concluida, dataReferencia.AddDays(-4), dataReferencia.AddDays(-2))
        ]);

        var casoDeUso = new ConsultaMetricasDashboardCasoDeUso(repositorioTarefa);

        var resposta = await casoDeUso.ExecutarAsync();

        resposta.TarefasAtrasadas.Should().Be(1);
        resposta.TarefasConcluidasNoPrazo.Should().Be(1);
        resposta.TaxaConclusao.Should().Be(50);
        resposta.TotalTarefasPorStatus.Single(item => item.Status == StatusTarefa.Concluida).Total.Should().Be(2);
    }

    private static Tarefa CriarTarefa(
        StatusTarefa status,
        DateTime dataPrazo,
        DateTime? dataConclusao = null)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Tarefa metrica",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelId = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow.AddDays(-6),
            DataPrazo = dataPrazo,
            DataConclusao = dataConclusao
        };
    }
}
