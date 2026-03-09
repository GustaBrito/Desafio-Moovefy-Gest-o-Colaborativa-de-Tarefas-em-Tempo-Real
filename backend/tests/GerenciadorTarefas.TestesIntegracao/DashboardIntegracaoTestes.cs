using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class DashboardIntegracaoTestes
{
    private static readonly Guid ResponsavelAdministradorId =
        Guid.Parse("8c519a4d-3f6d-4d0b-8b77-6ee8f5735990");

    [Fact]
    public async Task MetricasDashboard_DeveRetornarIndicadoresCorretos()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var projetoId = await CriarProjetoAsync(cliente);

        var tarefaPendenteAtrasadaId = await CriarTarefaAsync(
            cliente,
            projetoId,
            "Tarefa pendente atrasada",
            DateTime.UtcNow.AddDays(-2));

        var tarefaEmAndamentoId = await CriarTarefaAsync(
            cliente,
            projetoId,
            "Tarefa em andamento",
            DateTime.UtcNow.AddDays(3));

        var tarefaConcluidaId = await CriarTarefaAsync(
            cliente,
            projetoId,
            "Tarefa concluida no prazo",
            DateTime.UtcNow.AddDays(2));

        await AtualizarStatusAsync(cliente, tarefaEmAndamentoId, StatusTarefa.EmAndamento);
        await AtualizarStatusAsync(cliente, tarefaConcluidaId, StatusTarefa.EmAndamento);
        await AtualizarStatusAsync(cliente, tarefaConcluidaId, StatusTarefa.Concluida);

        var respostaMetricas = await cliente.GetAsync("/api/dashboard/metricas");

        respostaMetricas.StatusCode.Should().Be(HttpStatusCode.OK);
        var metricasEnvelope =
            await ClienteApiTeste.LerEnvelopeSucessoAsync<MetricasDashboardDadosRespostaTeste>(respostaMetricas);

        metricasEnvelope.Dados.Should().NotBeNull();
        var metricas = metricasEnvelope.Dados!;

        var totalPorStatus = metricas.TotalTarefasPorStatus.ToDictionary(item => item.Status, item => item.Total);

        totalPorStatus.GetValueOrDefault((int)StatusTarefa.Pendente).Should().Be(1);
        totalPorStatus.GetValueOrDefault((int)StatusTarefa.EmAndamento).Should().Be(1);
        totalPorStatus.GetValueOrDefault((int)StatusTarefa.Concluida).Should().Be(1);
        totalPorStatus.GetValueOrDefault((int)StatusTarefa.Cancelada).Should().Be(0);

        metricas.TarefasAtrasadas.Should().Be(1);
        metricas.TarefasConcluidasNoPrazo.Should().Be(1);
        metricas.TaxaConclusao.Should().Be(33.33m);

        var respostaTarefaPendente = await cliente.GetAsync($"/api/tarefas/{tarefaPendenteAtrasadaId}");
        respostaTarefaPendente.StatusCode.Should().Be(HttpStatusCode.OK);
        var tarefaPendente = await ClienteApiTeste.LerEnvelopeSucessoAsync<TarefaDadosRespostaTeste>(respostaTarefaPendente);
        tarefaPendente.Dados.Should().NotBeNull();
        tarefaPendente.Dados!.EstaAtrasada.Should().BeTrue();
    }

    private static async Task<Guid> CriarProjetoAsync(HttpClient cliente)
    {
        var respostaCriacao = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = "Projeto dashboard integracao",
            Descricao = "Projeto de apoio para teste de dashboard"
        });

        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created);
        var projetoCriado = await ClienteApiTeste.LerEnvelopeSucessoAsync<ProjetoDadosRespostaTeste>(respostaCriacao);
        projetoCriado.Dados.Should().NotBeNull();
        return projetoCriado.Dados!.Id;
    }

    private static async Task<Guid> CriarTarefaAsync(
        HttpClient cliente,
        Guid projetoId,
        string titulo,
        DateTime dataPrazo)
    {
        var respostaCriacao = await cliente.PostAsJsonAsync("/api/tarefas", new
        {
            Titulo = titulo,
            Descricao = "Tarefa de apoio para teste de dashboard",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = projetoId,
            ResponsavelId = ResponsavelAdministradorId,
            DataPrazo = dataPrazo
        });

        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created);
        var tarefaCriada = await ClienteApiTeste.LerEnvelopeSucessoAsync<TarefaDadosRespostaTeste>(respostaCriacao);
        tarefaCriada.Dados.Should().NotBeNull();
        return tarefaCriada.Dados!.Id;
    }

    private static async Task AtualizarStatusAsync(HttpClient cliente, Guid tarefaId, StatusTarefa status)
    {
        var resposta = await cliente.PatchAsJsonAsync(
            $"/api/tarefas/{tarefaId}/status",
            new { Status = status });

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
