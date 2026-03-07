namespace GerenciadorTarefas.Dominio.Entidades;

public class Tarefa
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int Status { get; set; }
    public int Prioridade { get; set; }
    public Guid ProjetoId { get; set; }
    public Guid ResponsavelId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataPrazo { get; set; }
    public DateTime? DataConclusao { get; set; }
}
