namespace GerenciadorTarefas.Dominio.Entidades;

public class ProjetoUsuarioVinculado
{
    public Guid ProjetoId { get; set; }
    public Guid UsuarioId { get; set; }

    public Projeto? Projeto { get; set; }
    public Usuario? Usuario { get; set; }
}
