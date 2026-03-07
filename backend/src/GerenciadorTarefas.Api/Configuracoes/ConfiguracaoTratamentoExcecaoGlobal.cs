using GerenciadorTarefas.Api.Intermediarios;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoTratamentoExcecaoGlobal
{
    public static IServiceCollection AdicionarTratamentoExcecaoGlobal(this IServiceCollection servicos)
    {
        servicos.AddProblemDetails();
        return servicos;
    }

    public static IApplicationBuilder UsarTratamentoExcecaoGlobal(this IApplicationBuilder aplicacao)
    {
        return aplicacao.UseMiddleware<MiddlewareTratamentoExcecaoGlobal>();
    }
}
