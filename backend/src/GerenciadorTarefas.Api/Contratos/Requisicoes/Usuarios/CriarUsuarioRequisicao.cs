using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Usuarios;

public sealed class CriarUsuarioRequisicao
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public PerfilGlobalUsuario PerfilGlobal { get; set; }
    public bool Ativo { get; set; } = true;
    public IReadOnlyCollection<Guid> AreaIds { get; set; } = [];
}
