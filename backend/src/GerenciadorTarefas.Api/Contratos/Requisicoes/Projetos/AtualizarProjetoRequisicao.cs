namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;

public sealed class AtualizarProjetoRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid AreaId { get; set; }
    public IReadOnlyCollection<Guid>? AreaIds { get; set; }
    public Guid? GestorUsuarioId { get; set; }
    public IReadOnlyCollection<Guid>? UsuarioIdsVinculados { get; set; }
}
