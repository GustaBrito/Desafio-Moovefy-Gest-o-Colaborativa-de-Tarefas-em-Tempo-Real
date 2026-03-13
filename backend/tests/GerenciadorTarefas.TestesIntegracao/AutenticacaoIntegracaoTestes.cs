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
            Email = "superadmin@gerenciadortarefas.local",
            Senha = "SuperAdmin@123"
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
            Email = "superadmin@gerenciadortarefas.local",
            Senha = "SenhaInvalida"
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var envelopeErro = await ClienteApiTeste.LerEnvelopeErroAsync(resposta);
        envelopeErro.Codigo.Should().Be("nao_autenticado");
    }

    [Fact]
    public async Task Login_DeveBloquearTemporariamente_QuandoExcedeTentativasInvalidas()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        for (var tentativa = 1; tentativa <= 3; tentativa++)
        {
            var respostaInvalida = await cliente.PostAsJsonAsync("/api/autenticacao/login", new
            {
                Email = "superadmin@gerenciadortarefas.local",
                Senha = "SenhaInvalida"
            });

            respostaInvalida.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var respostaBloqueio = await cliente.PostAsJsonAsync("/api/autenticacao/login", new
        {
            Email = "superadmin@gerenciadortarefas.local",
            Senha = "SuperAdmin@123"
        });

        respostaBloqueio.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        var envelopeErro = await ClienteApiTeste.LerEnvelopeErroAsync(respostaBloqueio);
        envelopeErro.Codigo.Should().Be("login_bloqueado_temporariamente");
    }
}
