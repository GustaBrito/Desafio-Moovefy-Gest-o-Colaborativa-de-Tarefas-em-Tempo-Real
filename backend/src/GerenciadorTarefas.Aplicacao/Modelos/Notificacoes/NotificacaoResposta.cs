namespace GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;

public sealed class NotificacaoResposta
{
    public Guid Id { get; init; }
    public Guid ResponsavelUsuarioId { get; init; }
    public Guid TarefaId { get; init; }
    public Guid ProjetoId { get; init; }
    public string TituloTarefa { get; init; } = string.Empty;
    public string Mensagem { get; init; } = string.Empty;
    public bool Reatribuicao { get; init; }
    public DateTime DataCriacao { get; init; }
}
