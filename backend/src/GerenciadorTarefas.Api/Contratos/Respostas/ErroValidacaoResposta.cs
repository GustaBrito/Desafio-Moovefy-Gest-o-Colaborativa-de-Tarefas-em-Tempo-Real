namespace GerenciadorTarefas.Api.Contratos.Respostas;

public sealed class ErroValidacaoResposta
{
    public string Campo { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
}
