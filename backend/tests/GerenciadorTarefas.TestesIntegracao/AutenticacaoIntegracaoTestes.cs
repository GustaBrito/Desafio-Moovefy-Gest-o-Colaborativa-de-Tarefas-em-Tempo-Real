using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class AutenticacaoIntegracaoTestes
{
    [Fact]
    public async Task Login_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/api/autenticacao/login", new
        {
            Email = "admin@gerenciadortarefas.local",
            Senha = "Admin@123"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ClienteApiTeste.LerEnvelopeSucessoAsync<LoginDadosRespostaTeste>(resposta);
        envelope.Dados.Should().NotBeNull();
        envelope.Dados!.TokenAcesso.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_DeveRetornarNaoAutorizado_QuandoCredenciaisInvalidas()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/api/autenticacao/login", new
        {
            Email = "admin@gerenciadortarefas.local",
            Senha = "SenhaInvalida"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var envelopeErro = await ClienteApiTeste.LerEnvelopeErroAsync(resposta);
        envelopeErro.Codigo.Should().Be("nao_autenticado");
    }
}
