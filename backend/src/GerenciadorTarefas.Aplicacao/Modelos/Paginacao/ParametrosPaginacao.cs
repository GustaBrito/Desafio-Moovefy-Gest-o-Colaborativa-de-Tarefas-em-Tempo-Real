namespace GerenciadorTarefas.Aplicacao.Modelos.Paginacao;

public sealed class ParametrosPaginacao
{
    public const int NumeroPaginaPadrao = 1;
    public const int TamanhoPaginaPadrao = 20;
    public const int TamanhoPaginaMaximo = 100;

    public int NumeroPagina { get; init; } = NumeroPaginaPadrao;
    public int TamanhoPagina { get; init; } = TamanhoPaginaPadrao;

    public int Pular => (NumeroPaginaNormalizada - 1) * TamanhoPaginaNormalizado;
    public int Tomar => TamanhoPaginaNormalizado;

    public int NumeroPaginaNormalizada => NumeroPagina < NumeroPaginaPadrao
        ? NumeroPaginaPadrao
        : NumeroPagina;

    public int TamanhoPaginaNormalizado
    {
        get
        {
            if (TamanhoPagina < 1)
            {
                return TamanhoPaginaPadrao;
            }

            return TamanhoPagina > TamanhoPaginaMaximo
                ? TamanhoPaginaMaximo
                : TamanhoPagina;
        }
    }
}
