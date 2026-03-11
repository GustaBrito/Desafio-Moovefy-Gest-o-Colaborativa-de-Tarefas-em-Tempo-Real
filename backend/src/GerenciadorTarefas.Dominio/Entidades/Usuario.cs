using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Dominio.Entidades;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; }
    public DateTime? UltimoAcesso { get; set; }

    public ICollection<UsuarioArea> AreasVinculadas { get; set; } = new List<UsuarioArea>();
}
