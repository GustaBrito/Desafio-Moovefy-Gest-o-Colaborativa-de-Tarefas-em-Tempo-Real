namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Areas;

public sealed class AtualizarAreaRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public bool Ativa { get; set; }
}
