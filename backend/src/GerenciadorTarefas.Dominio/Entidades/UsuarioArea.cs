namespace GerenciadorTarefas.Dominio.Entidades;

public class UsuarioArea
{
    public Guid UsuarioId { get; set; }
    public Guid AreaId { get; set; }

    public Usuario? Usuario { get; set; }
    public Area? Area { get; set; }
}
