namespace GerenciadorTarefas.Dominio.Entidades;

public class Projeto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; }
}
