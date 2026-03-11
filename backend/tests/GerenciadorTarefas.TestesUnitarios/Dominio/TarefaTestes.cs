using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.TestesUnitarios.Dominio;

public sealed class TarefaTestes
{
    [Fact]
    public void EstaAtrasada_DeveRetornarVerdadeiro_QuandoStatusPendenteEPrazoPassado()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, DateTime.UtcNow.AddDays(-1));

        var estaAtrasada = tarefa.EstaAtrasada(DateTime.UtcNow);

        estaAtrasada.Should().BeTrue();
    }

    [Fact]
    public void EstaAtrasada_DeveRetornarFalso_QuandoStatusConcluidaMesmoComPrazoPassado()
    {
        var tarefa = CriarTarefa(StatusTarefa.Concluida, DateTime.UtcNow.AddDays(-2));

        var estaAtrasada = tarefa.EstaAtrasada(DateTime.UtcNow);

        estaAtrasada.Should().BeFalse();
    }

    [Fact]
    public void AtualizarStatus_DevePreencherDataConclusao_QuandoConcluir()
    {
        var dataConclusao = DateTime.UtcNow;
        var tarefa = CriarTarefa(StatusTarefa.EmAndamento, DateTime.UtcNow.AddDays(1));

        tarefa.AtualizarStatus(StatusTarefa.Concluida, dataConclusao);

        tarefa.Status.Should().Be(StatusTarefa.Concluida);
        tarefa.DataConclusao.Should().Be(dataConclusao);
    }

    [Fact]
    public void AtualizarStatus_DeveLimparDataConclusao_QuandoNaoConcluir()
    {
        var tarefa = CriarTarefa(StatusTarefa.EmAndamento, DateTime.UtcNow.AddDays(1));
        tarefa.DataConclusao = DateTime.UtcNow.AddDays(-1);

        tarefa.AtualizarStatus(StatusTarefa.Cancelada, DateTime.UtcNow);

        tarefa.Status.Should().Be(StatusTarefa.Cancelada);
        tarefa.DataConclusao.Should().BeNull();
    }

    [Fact]
    public void AtualizarStatus_DeveLancarExcecao_QuandoTransicaoInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, DateTime.UtcNow.AddDays(1));

        var acao = () => tarefa.AtualizarStatus(StatusTarefa.Concluida, DateTime.UtcNow);

        acao.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Transicao de status invalida*");
    }

    [Fact]
    public void PodeTransitarPara_DeveRespeitarFluxoCanceladaComoEstadoTerminal()
    {
        var tarefa = CriarTarefa(StatusTarefa.Cancelada, DateTime.UtcNow.AddDays(1));

        tarefa.PodeTransitarPara(StatusTarefa.Pendente).Should().BeFalse();
        tarefa.PodeTransitarPara(StatusTarefa.EmAndamento).Should().BeFalse();
        tarefa.PodeTransitarPara(StatusTarefa.Concluida).Should().BeFalse();
        tarefa.PodeTransitarPara(StatusTarefa.Cancelada).Should().BeTrue();
    }

    [Fact]
    public void PodeTransitarPara_DeveRespeitarFluxoPadrao()
    {
        var tarefaPendente = CriarTarefa(StatusTarefa.Pendente, DateTime.UtcNow.AddDays(1));
        var tarefaAndamento = CriarTarefa(StatusTarefa.EmAndamento, DateTime.UtcNow.AddDays(1));
        var tarefaConcluida = CriarTarefa(StatusTarefa.Concluida, DateTime.UtcNow.AddDays(1));

        tarefaPendente.PodeTransitarPara(StatusTarefa.EmAndamento).Should().BeTrue();
        tarefaPendente.PodeTransitarPara(StatusTarefa.Cancelada).Should().BeTrue();
        tarefaPendente.PodeTransitarPara(StatusTarefa.Concluida).Should().BeFalse();

        tarefaAndamento.PodeTransitarPara(StatusTarefa.Concluida).Should().BeTrue();
        tarefaAndamento.PodeTransitarPara(StatusTarefa.Cancelada).Should().BeTrue();
        tarefaAndamento.PodeTransitarPara(StatusTarefa.Pendente).Should().BeFalse();

        tarefaConcluida.PodeTransitarPara(StatusTarefa.Pendente).Should().BeFalse();
        tarefaConcluida.PodeTransitarPara(StatusTarefa.EmAndamento).Should().BeFalse();
        tarefaConcluida.PodeTransitarPara(StatusTarefa.Concluida).Should().BeTrue();
    }

    [Fact]
    public void AtualizarStatus_DeveNaoAlterarDados_QuandoNovoStatusForIgualAoAtual()
    {
        var dataConclusao = DateTime.UtcNow.AddHours(-2);
        var tarefa = CriarTarefa(StatusTarefa.Concluida, DateTime.UtcNow.AddDays(-3));
        tarefa.DataConclusao = dataConclusao;

        tarefa.AtualizarStatus(StatusTarefa.Concluida, DateTime.UtcNow);

        tarefa.Status.Should().Be(StatusTarefa.Concluida);
        tarefa.DataConclusao.Should().Be(dataConclusao);
    }

    [Fact]
    public void PodeTransitarPara_DeveRetornarFalso_QuandoStatusAtualForDesconhecido()
    {
        var tarefa = CriarTarefa((StatusTarefa)999, DateTime.UtcNow.AddDays(1));

        var podeTransitar = tarefa.PodeTransitarPara(StatusTarefa.Pendente);

        podeTransitar.Should().BeFalse();
    }

    private static Tarefa CriarTarefa(StatusTarefa status, DateTime dataPrazo)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Implementar regra",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelUsuarioId = Guid.NewGuid(),
            DataCriacao = DateTime.UtcNow.AddDays(-5),
            DataPrazo = dataPrazo
        };
    }
}
