using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class TarefasIntegracaoTestes
{
    private static readonly Guid ResponsavelAdministradorId =
        Guid.Parse("8c519a4d-3f6d-4d0b-8b77-6ee8f5735990");

    [Fact]
    public async Task FluxoTarefas_DeveCriarAtualizarStatusListarEExcluir()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var projetoId = await CriarProjetoAsync(cliente, "Projeto fluxo tarefas");
        var tarefaId = await CriarTarefaAsync(
            cliente,
            projetoId,
            "Tarefa fluxo integracao",
            DateTime.UtcNow.AddDays(5));

        var respostaAtualizarStatus = await cliente.PatchAsJsonAsync(
            $"/api/tarefas/{tarefaId}/status",
            new
            {
                Status = StatusTarefa.EmAndamento
            });

        respostaAtualizarStatus.StatusCode.Should().Be(HttpStatusCode.OK);
        var tarefaEmAndamento =
            await ClienteApiTeste.LerEnvelopeSucessoAsync<TarefaDadosRespostaTeste>(respostaAtualizarStatus);
        tarefaEmAndamento.Dados.Should().NotBeNull();
        tarefaEmAndamento.Dados!.Status.Should().Be((int)StatusTarefa.EmAndamento);

        var respostaListagem = await cliente.GetAsync("/api/tarefas?numeroPagina=1&tamanhoPagina=10");
        respostaListagem.StatusCode.Should().Be(HttpStatusCode.OK);

        var tarefas = await ClienteApiTeste
            .LerEnvelopeSucessoAsync<ResultadoPaginadoDadosRespostaTeste<TarefaDadosRespostaTeste>>(respostaListagem);
        tarefas.Dados.Should().NotBeNull();
        tarefas.Dados!.Itens.Any(item => item.Id == tarefaId).Should().BeTrue();

        var respostaConcluir = await cliente.PatchAsJsonAsync(
            $"/api/tarefas/{tarefaId}/status",
            new
            {
                Status = StatusTarefa.Concluida
            });

        respostaConcluir.StatusCode.Should().Be(HttpStatusCode.OK);
        var tarefaConcluida = await ClienteApiTeste.LerEnvelopeSucessoAsync<TarefaDadosRespostaTeste>(respostaConcluir);
        tarefaConcluida.Dados.Should().NotBeNull();
        tarefaConcluida.Dados!.Status.Should().Be((int)StatusTarefa.Concluida);
        tarefaConcluida.Dados.DataConclusao.Should().NotBeNull();

        var respostaExclusao = await cliente.DeleteAsync($"/api/tarefas/{tarefaId}");
        respostaExclusao.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExcluirTarefa_DeveBloquear_QuandoStatusEmAndamento()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var projetoId = await CriarProjetoAsync(cliente, "Projeto bloqueio exclusao tarefa");
        var tarefaId = await CriarTarefaAsync(
            cliente,
            projetoId,
            "Tarefa para bloquear exclusao",
            DateTime.UtcNow.AddDays(2));

        var respostaAtualizarStatus = await cliente.PatchAsJsonAsync(
            $"/api/tarefas/{tarefaId}/status",
            new
            {
                Status = StatusTarefa.EmAndamento
            });
        respostaAtualizarStatus.StatusCode.Should().Be(HttpStatusCode.OK);

        var respostaExclusao = await cliente.DeleteAsync($"/api/tarefas/{tarefaId}");
        respostaExclusao.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelopeErro = await ClienteApiTeste.LerEnvelopeErroAsync(respostaExclusao);
        envelopeErro.Codigo.Should().Be("operacao_invalida");
    }

    private static async Task<Guid> CriarProjetoAsync(HttpClient cliente, string nome)
    {
        var respostaCriacao = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = nome,
            Descricao = "Projeto de apoio para teste de tarefas",
            AreaId = SemeadorDadosDemonstracao.AreaDesenvolvimentoSoftwareId
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
            Descricao = "Tarefa de apoio para teste de integracao",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = projetoId,
            ResponsavelUsuarioId = ResponsavelAdministradorId,
            DataPrazo = dataPrazo
        });

        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created);
        var tarefaCriada = await ClienteApiTeste.LerEnvelopeSucessoAsync<TarefaDadosRespostaTeste>(respostaCriacao);
        tarefaCriada.Dados.Should().NotBeNull();
        return tarefaCriada.Dados!.Id;
    }
}
