using System.Net;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class ObservabilidadeIntegracaoTestes
{
    [Fact]
    public async Task HealthLive_DeveRetornarOk()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        var resposta = await cliente.GetAsync("/health/live");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resposta.Content.ReadAsStringAsync();
        payload.Should().Contain("\"status\"");
    }

    [Fact]
    public async Task HealthReady_DeveRetornarOkOuDegraded()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        var resposta = await cliente.GetAsync("/health/ready");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await resposta.Content.ReadAsStringAsync();
        payload.Should().Contain("\"checks\"");
    }

    [Fact]
    public async Task MetricasOperacionais_DeveRetornarSnapshot()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();

        // A coleta do snapshot ocorre durante a propria requisicao de metricas.
        // Portanto, aquecemos uma chamada antes para garantir contagem observavel.
        var respostaAquecimento = await cliente.GetAsync("/api/autenticacao/saude");
        respostaAquecimento.StatusCode.Should().Be(HttpStatusCode.OK);

        var resposta = await cliente.GetAsync("/api/observabilidade/metricas");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ClienteApiTeste
            .LerEnvelopeSucessoAsync<MetricasOperacionaisDadosRespostaTeste>(resposta);

        envelope.Dados.Should().NotBeNull();
        envelope.Dados!.TotalRequisicoes.Should().BeGreaterThan(0);
        envelope.Dados.MemoriaGerenciadaBytes.Should().BeGreaterThan(0);
    }
}
