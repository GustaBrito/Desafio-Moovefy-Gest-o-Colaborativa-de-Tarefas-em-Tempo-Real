namespace GerenciadorTarefas.Aplicacao.Modelos.Paginacao;

public sealed class ResultadoPaginado<TRegistro>
{
    public ResultadoPaginado(
        IReadOnlyCollection<TRegistro> itens,
        int totalRegistros,
        int numeroPagina,
        int tamanhoPagina)
    {
        Itens = itens;
        TotalRegistros = totalRegistros;
        NumeroPagina = numeroPagina;
        TamanhoPagina = tamanhoPagina;
    }

    public IReadOnlyCollection<TRegistro> Itens { get; }
    public int TotalRegistros { get; }
    public int NumeroPagina { get; }
    public int TamanhoPagina { get; }

    public int TotalPaginas => TamanhoPagina <= 0
        ? 0
        : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);

    public static ResultadoPaginado<TRegistro> Criar(
        IReadOnlyCollection<TRegistro> itens,
        int totalRegistros,
        ParametrosPaginacao parametrosPaginacao)
    {
        return new ResultadoPaginado<TRegistro>(
            itens,
            totalRegistros,
            parametrosPaginacao.NumeroPaginaNormalizada,
            parametrosPaginacao.TamanhoPaginaNormalizado);
    }
}
