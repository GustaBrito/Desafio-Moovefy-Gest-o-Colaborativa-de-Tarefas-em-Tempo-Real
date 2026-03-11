using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class FiltroConsultaTarefasEntradaTestes
{
    [Fact]
    public void Propriedades_DevePermitirConfiguracaoCompleta()
    {
        var projetoId = Guid.NewGuid();
        var ResponsavelUsuarioId = Guid.NewGuid();
        var dataInicial = DateTime.UtcNow.Date;
        var dataFinal = dataInicial.AddDays(7);

        var filtro = new FiltroConsultaTarefasEntrada
        {
            ProjetoId = projetoId,
            Status = StatusTarefa.EmAndamento,
            ResponsavelUsuarioId = ResponsavelUsuarioId,
            DataPrazoInicial = dataInicial,
            DataPrazoFinal = dataFinal,
            CampoOrdenacao = CampoOrdenacaoTarefa.DataPrazo,
            DirecaoOrdenacao = DirecaoOrdenacaoTarefa.Ascendente,
            NumeroPagina = 3,
            TamanhoPagina = 25
        };

        filtro.ProjetoId.Should().Be(projetoId);
        filtro.Status.Should().Be(StatusTarefa.EmAndamento);
        filtro.ResponsavelUsuarioId.Should().Be(ResponsavelUsuarioId);
        filtro.DataPrazoInicial.Should().Be(dataInicial);
        filtro.DataPrazoFinal.Should().Be(dataFinal);
        filtro.CampoOrdenacao.Should().Be(CampoOrdenacaoTarefa.DataPrazo);
        filtro.DirecaoOrdenacao.Should().Be(DirecaoOrdenacaoTarefa.Ascendente);
        filtro.NumeroPagina.Should().Be(3);
        filtro.TamanhoPagina.Should().Be(25);
    }
}
