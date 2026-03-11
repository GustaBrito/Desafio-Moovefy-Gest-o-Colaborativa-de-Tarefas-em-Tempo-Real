namespace GerenciadorTarefas.Dominio.Entidades;

public sealed class Notificacao
{
    public Guid Id { get; set; }
    public Guid ResponsavelUsuarioId { get; set; }
    public Guid TarefaId { get; set; }
    public Guid ProjetoId { get; set; }
    public string TituloTarefa { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public bool Reatribuicao { get; set; }
    public DateTime DataCriacao { get; set; }

    public Usuario? ResponsavelUsuario { get; set; }
    public Tarefa? Tarefa { get; set; }
    public Projeto? Projeto { get; set; }
}
