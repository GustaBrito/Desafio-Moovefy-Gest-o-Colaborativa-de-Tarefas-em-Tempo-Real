using GerenciadorTarefas.Aplicacao.Modelos.Paginacao;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Paginacao;

public sealed class ParametrosPaginacaoTestes
{
    [Fact]
    public void NumeroPaginaNormalizada_DeveAssumirUm_QuandoNumeroMenorQueUm()
    {
        var parametros = new ParametrosPaginacao
        {
            NumeroPagina = -10
        };

        parametros.NumeroPaginaNormalizada.Should().Be(1);
    }

    [Fact]
    public void TamanhoPaginaNormalizado_DeveAssumirPadrao_QuandoValorInvalido()
    {
        var parametros = new ParametrosPaginacao
        {
            TamanhoPagina = 0
        };

        parametros.TamanhoPaginaNormalizado.Should().Be(ParametrosPaginacao.TamanhoPaginaPadrao);
    }

    [Fact]
    public void TamanhoPaginaNormalizado_DeveAssumirMaximo_QuandoMaiorQueLimite()
    {
        var parametros = new ParametrosPaginacao
        {
            TamanhoPagina = 999
        };

        parametros.TamanhoPaginaNormalizado.Should().Be(ParametrosPaginacao.TamanhoPaginaMaximo);
    }

    [Fact]
    public void Pular_DeveUsarValoresNormalizados()
    {
        var parametros = new ParametrosPaginacao
        {
            NumeroPagina = 3,
            TamanhoPagina = 10
        };

        parametros.Pular.Should().Be(20);
        parametros.Tomar.Should().Be(10);
    }
}
