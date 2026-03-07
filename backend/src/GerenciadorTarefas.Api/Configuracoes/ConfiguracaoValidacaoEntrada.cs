using FluentValidation;
using FluentValidation.AspNetCore;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoValidacaoEntrada
{
    public static IServiceCollection AdicionarValidacaoEntrada(this IServiceCollection servicos)
    {
        servicos.AddFluentValidationAutoValidation();
        servicos.AddValidatorsFromAssemblyContaining(typeof(ConfiguracaoValidacaoEntrada));

        return servicos;
    }
}
