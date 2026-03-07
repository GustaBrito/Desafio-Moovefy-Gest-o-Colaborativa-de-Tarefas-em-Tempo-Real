namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;

public sealed class CriarProjetoRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
}
