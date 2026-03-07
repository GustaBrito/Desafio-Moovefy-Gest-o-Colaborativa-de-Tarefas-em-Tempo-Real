namespace GerenciadorTarefas.Api.Contratos.Respostas;

public sealed class RespostaErroApi
{
    public bool Sucesso { get; init; } = false;
    public int Status { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
    public string? Detalhe { get; init; }
    public string? CodigoRastreio { get; init; }
    public IReadOnlyCollection<ErroValidacaoResposta> ErrosValidacao { get; init; } = [];
}
