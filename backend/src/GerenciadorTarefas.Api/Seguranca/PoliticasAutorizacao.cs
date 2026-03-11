namespace GerenciadorTarefas.Api.Seguranca;

public static class PoliticasAutorizacao
{
    public const string ApenasSuperAdmin = "apenas_super_admin";
    public const string AdministracaoUsuarios = "administracao_usuarios";
    public const string AdministracaoAreas = "administracao_areas";
}
