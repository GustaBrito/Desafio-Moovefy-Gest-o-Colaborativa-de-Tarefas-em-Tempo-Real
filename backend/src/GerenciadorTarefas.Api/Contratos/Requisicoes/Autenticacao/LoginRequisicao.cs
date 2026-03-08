namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Autenticacao;

public sealed class LoginRequisicao
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
