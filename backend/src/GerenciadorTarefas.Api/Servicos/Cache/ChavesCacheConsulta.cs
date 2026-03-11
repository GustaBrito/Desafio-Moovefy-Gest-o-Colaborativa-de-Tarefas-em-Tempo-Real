using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.Api.Servicos.Cache;

public static class ChavesCacheConsulta
{
    public const string PrefixoProjetos = "consultas:projetos";
    public const string PrefixoTarefas = "consultas:tarefas";
    public const string PrefixoDashboard = "consultas:dashboard";

    public static string ObterListaProjetos() => $"{PrefixoProjetos}:lista";

    public static string ObterProjetoPorId(Guid id) => $"{PrefixoProjetos}:item:{id:N}";

    public static string ObterListaTarefas(
        Guid? projetoId,
        StatusTarefa? status,
        Guid? responsavelUsuarioId,
        DateTime? dataPrazoInicial,
        DateTime? dataPrazoFinal,
        CampoOrdenacaoTarefa? campoOrdenacao,
        DirecaoOrdenacaoTarefa? direcaoOrdenacao,
        int numeroPagina,
        int tamanhoPagina)
    {
        var chaveFiltro = string.Join("|",
            projetoId?.ToString("N") ?? "null",
            ((int?)status)?.ToString() ?? "null",
            responsavelUsuarioId?.ToString("N") ?? "null",
            dataPrazoInicial?.Ticks.ToString() ?? "null",
            dataPrazoFinal?.Ticks.ToString() ?? "null",
            ((int?)campoOrdenacao)?.ToString() ?? "null",
            ((int?)direcaoOrdenacao)?.ToString() ?? "null",
            numeroPagina,
            tamanhoPagina);

        return $"{PrefixoTarefas}:lista:{chaveFiltro}";
    }

    public static string ObterTarefaPorId(Guid id) => $"{PrefixoTarefas}:item:{id:N}";

    public static string ObterMetricasDashboard() => $"{PrefixoDashboard}:metricas";
}
