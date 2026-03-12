namespace GerenciadorTarefas.Dominio.Entidades;

public class ProjetoArea
{
    public Guid ProjetoId { get; set; }
    public Guid AreaId { get; set; }

    public Projeto? Projeto { get; set; }
    public Area? Area { get; set; }
}
