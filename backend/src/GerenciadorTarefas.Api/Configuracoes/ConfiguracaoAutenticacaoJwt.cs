using System.Text;
using GerenciadorTarefas.Api.Modelos.Autenticacao;
using GerenciadorTarefas.Api.Seguranca;
using GerenciadorTarefas.Dominio.Enumeracoes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoAutenticacaoJwt
{
    public static IServiceCollection AdicionarAutenticacaoJwt(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        var secaoConfiguracaoJwt = configuracao.GetSection(ConfiguracaoJwt.NomeSecao);
        servicos.Configure<ConfiguracaoJwt>(secaoConfiguracaoJwt);

        var configuracaoJwt = secaoConfiguracaoJwt.Get<ConfiguracaoJwt>()
            ?? throw new InvalidOperationException("A secao de autenticacao JWT nao foi configurada.");

        ValidarConfiguracaoJwt(configuracaoJwt);
        var chaveAssinatura = Encoding.UTF8.GetBytes(configuracaoJwt.ChaveSecreta);

        servicos
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opcoes =>
            {
                opcoes.RequireHttpsMetadata = configuracaoJwt.ExigirHttpsMetadata;
                opcoes.SaveToken = false;
                opcoes.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(chaveAssinatura),
                    ValidateIssuer = true,
                    ValidIssuer = configuracaoJwt.Emissor,
                    ValidateAudience = true,
                    ValidAudience = configuracaoJwt.Publico,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                opcoes.Events = new JwtBearerEvents
                {
                    OnMessageReceived = contexto =>
                    {
                        var tokenConsulta = contexto.Request.Query["access_token"];
                        var caminho = contexto.HttpContext.Request.Path;

                        if (!string.IsNullOrWhiteSpace(tokenConsulta)
                            && caminho.StartsWithSegments("/hubs/notificacoes"))
                        {
                            contexto.Token = tokenConsulta;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        servicos.AddAuthorization(opcoes =>
        {
            opcoes.AddPolicy(PoliticasAutorizacao.ApenasSuperAdmin, politica =>
            {
                politica.RequireRole(PerfilGlobalUsuario.SuperAdmin.ToString());
            });

            opcoes.AddPolicy(PoliticasAutorizacao.AdministracaoUsuarios, politica =>
            {
                politica.RequireRole(
                    PerfilGlobalUsuario.SuperAdmin.ToString(),
                    PerfilGlobalUsuario.Admin.ToString());
            });

            opcoes.AddPolicy(PoliticasAutorizacao.AdministracaoAreas, politica =>
            {
                politica.RequireRole(PerfilGlobalUsuario.SuperAdmin.ToString());
            });
        });

        return servicos;
    }

    private static void ValidarConfiguracaoJwt(ConfiguracaoJwt configuracaoJwt)
    {
        if (string.IsNullOrWhiteSpace(configuracaoJwt.ChaveSecreta)
            || configuracaoJwt.ChaveSecreta.Length < 32)
        {
            throw new InvalidOperationException(
                "A chave secreta de JWT deve possuir ao menos 32 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(configuracaoJwt.Emissor))
        {
            throw new InvalidOperationException("O emissor de JWT deve ser informado.");
        }

        if (string.IsNullOrWhiteSpace(configuracaoJwt.Publico))
        {
            throw new InvalidOperationException("O publico de JWT deve ser informado.");
        }

        if (configuracaoJwt.ExpiracaoMinutos <= 0)
        {
            throw new InvalidOperationException("A expiracao do token JWT deve ser maior que zero.");
        }
    }
}
