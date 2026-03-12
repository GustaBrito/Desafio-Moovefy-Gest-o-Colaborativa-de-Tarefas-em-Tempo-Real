namespace GerenciadorTarefas.Dominio.Entidades;

public class Projeto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Guid AreaId { get; set; }
    public Guid? CriadoPorUsuarioId { get; set; }
    public Guid? GestorUsuarioId { get; set; }
    public DateTime DataCriacao { get; set; }

    public Area? Area { get; set; }
    public Usuario? CriadoPorUsuario { get; set; }
    public Usuario? GestorUsuario { get; set; }
    public ICollection<ProjetoArea> AreasVinculadas { get; set; } = new List<ProjetoArea>();
    public ICollection<ProjetoUsuarioVinculado> UsuariosVinculados { get; set; } = new List<ProjetoUsuarioVinculado>();
}
