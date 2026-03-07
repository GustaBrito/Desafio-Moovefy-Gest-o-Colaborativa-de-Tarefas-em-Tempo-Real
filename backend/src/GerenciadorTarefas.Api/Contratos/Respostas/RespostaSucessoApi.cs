namespace GerenciadorTarefas.Api.Contratos.Respostas;

public sealed class RespostaSucessoApi<TDados>
{
    public bool Sucesso { get; init; } = true;
    public string Mensagem { get; init; } = string.Empty;
    public TDados? Dados { get; init; }
    public string? CodigoRastreio { get; init; }
}
