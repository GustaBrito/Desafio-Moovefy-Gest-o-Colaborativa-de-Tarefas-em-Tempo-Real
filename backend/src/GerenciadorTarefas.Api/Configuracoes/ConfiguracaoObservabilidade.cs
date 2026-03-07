using Serilog;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoObservabilidade
{
    public static WebApplicationBuilder AdicionarObservabilidade(this WebApplicationBuilder construtorAplicacao)
    {
        construtorAplicacao.Host.UseSerilog((contextoHospedagem, servicos, configuracaoSerilog) =>
        {
            configuracaoSerilog
                .ReadFrom.Configuration(contextoHospedagem.Configuration)
                .ReadFrom.Services(servicos)
                .Enrich.FromLogContext();
        });

        return construtorAplicacao;
    }
}
