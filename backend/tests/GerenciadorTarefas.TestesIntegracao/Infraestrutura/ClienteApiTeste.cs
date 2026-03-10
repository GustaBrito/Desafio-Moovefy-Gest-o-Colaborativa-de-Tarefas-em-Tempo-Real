using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace GerenciadorTarefas.TestesIntegracao.Infraestrutura;

internal static class ClienteApiTeste
{
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> ObterTokenAsync(
        HttpClient cliente,
        string email = "admin@gerenciadortarefas.local",
        string senha = "Admin@123")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/autenticacao/login", new
        {
            Email = email,
            Senha = senha
        });

        resposta.IsSuccessStatusCode.Should().BeTrue();

        var envelope = await LerEnvelopeSucessoAsync<LoginDadosRespostaTeste>(resposta);
        envelope.Dados.Should().NotBeNull();
        envelope.Dados!.TokenAcesso.Should().NotBeNullOrWhiteSpace();

        return envelope.Dados.TokenAcesso;
    }

    public static async Task ConfigurarAutorizacaoAsync(HttpClient cliente)
    {
        var token = await ObterTokenAsync(cliente);

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task ConfigurarAutorizacaoAsync(
        HttpClient cliente,
        string email,
        string senha)
    {
        var token = await ObterTokenAsync(cliente, email, senha);

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<EnvelopeSucessoRespostaTeste<TDados>> LerEnvelopeSucessoAsync<TDados>(
        HttpResponseMessage resposta)
    {
        var envelope = await resposta.Content.ReadFromJsonAsync<EnvelopeSucessoRespostaTeste<TDados>>(OpcoesJson);
        envelope.Should().NotBeNull();
        envelope!.Sucesso.Should().BeTrue();
        return envelope;
    }

    public static async Task<EnvelopeErroRespostaTeste> LerEnvelopeErroAsync(HttpResponseMessage resposta)
    {
        var envelope = await resposta.Content.ReadFromJsonAsync<EnvelopeErroRespostaTeste>(OpcoesJson);
        envelope.Should().NotBeNull();
        envelope!.Sucesso.Should().BeFalse();
        return envelope;
    }
}
