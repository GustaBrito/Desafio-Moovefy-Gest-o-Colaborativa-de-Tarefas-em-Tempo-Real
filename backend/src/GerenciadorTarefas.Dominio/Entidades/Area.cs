namespace GerenciadorTarefas.Dominio.Entidades;

public class Area
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public bool Ativa { get; set; } = true;

    public ICollection<UsuarioArea> UsuariosVinculados { get; set; } = new List<UsuarioArea>();
}
