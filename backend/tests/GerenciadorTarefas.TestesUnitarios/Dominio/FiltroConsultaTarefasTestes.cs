using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.TestesUnitarios.Dominio;

public sealed class FiltroConsultaTarefasTestes
{
    [Fact]
    public void Propriedades_DevePermitirLeituraDosValoresConfigurados()
    {
        var projetoId = Guid.NewGuid();
        var ResponsavelUsuarioId = Guid.NewGuid();
        var dataInicial = DateTime.UtcNow.Date;
        var dataFinal = dataInicial.AddDays(5);

        var filtro = new FiltroConsultaTarefas
        {
            ProjetoId = projetoId,
            Status = StatusTarefa.Pendente,
            ResponsavelUsuarioId = ResponsavelUsuarioId,
            DataPrazoInicial = dataInicial,
            DataPrazoFinal = dataFinal,
            CampoOrdenacao = CampoOrdenacaoTarefa.Titulo,
            DirecaoOrdenacao = DirecaoOrdenacaoTarefa.Descendente,
            Pular = 20,
            Tomar = 10
        };

        filtro.ProjetoId.Should().Be(projetoId);
        filtro.Status.Should().Be(StatusTarefa.Pendente);
        filtro.ResponsavelUsuarioId.Should().Be(ResponsavelUsuarioId);
        filtro.DataPrazoInicial.Should().Be(dataInicial);
        filtro.DataPrazoFinal.Should().Be(dataFinal);
        filtro.CampoOrdenacao.Should().Be(CampoOrdenacaoTarefa.Titulo);
        filtro.DirecaoOrdenacao.Should().Be(DirecaoOrdenacaoTarefa.Descendente);
        filtro.Pular.Should().Be(20);
        filtro.Tomar.Should().Be(10);
    }
}
