namespace GerenciadorTarefas.Api.Servicos.Cache;

public static class PoliticasCacheConsulta
{
    public static readonly TimeSpan DuracaoProjetos = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan DuracaoTarefas = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan DuracaoDashboard = TimeSpan.FromSeconds(30);
}
