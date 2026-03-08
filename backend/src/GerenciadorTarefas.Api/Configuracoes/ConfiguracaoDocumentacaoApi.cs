using Microsoft.OpenApi.Models;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoDocumentacaoApi
{
    public static IServiceCollection AdicionarDocumentacaoApi(this IServiceCollection servicos)
    {
        servicos.AddEndpointsApiExplorer();
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Gerenciador de Tarefas API",
                Version = "v1",
                Description = "API para gestao colaborativa de projetos e tarefas."
            });

            opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato Bearer."
            });

            opcoes.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return servicos;
    }

    public static WebApplication UsarDocumentacaoApi(this WebApplication aplicacao)
    {
        aplicacao.UseSwagger();
        aplicacao.UseSwaggerUI(opcoes =>
        {
            opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Gerenciador de Tarefas API v1");
            opcoes.RoutePrefix = "documentacao";
        });

        return aplicacao;
    }
}
