using GerenciadorTarefas.Aplicacao.Modelos.Paginacao;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Paginacao;

public sealed class ResultadoPaginadoTestes
{
    [Fact]
    public void TotalPaginas_DeveCalcularComArredondamentoParaCima()
    {
        var resultado = new ResultadoPaginado<int>(
            itens: [1, 2, 3],
            totalRegistros: 21,
            numeroPagina: 1,
            tamanhoPagina: 10);

        resultado.TotalPaginas.Should().Be(3);
    }

    [Fact]
    public void TotalPaginas_DeveRetornarZero_QuandoTamanhoPaginaForInvalido()
    {
        var resultado = new ResultadoPaginado<int>(
            itens: [],
            totalRegistros: 10,
            numeroPagina: 1,
            tamanhoPagina: 0);

        resultado.TotalPaginas.Should().Be(0);
    }

    [Fact]
    public void Criar_DeveUtilizarParametrosNormalizados()
    {
        var parametros = new ParametrosPaginacao
        {
            NumeroPagina = -2,
            TamanhoPagina = 500
        };

        var resultado = ResultadoPaginado<string>.Criar(
            itens: ["a"],
            totalRegistros: 1,
            parametrosPaginacao: parametros);

        resultado.NumeroPagina.Should().Be(1);
        resultado.TamanhoPagina.Should().Be(ParametrosPaginacao.TamanhoPaginaMaximo);
    }
}
