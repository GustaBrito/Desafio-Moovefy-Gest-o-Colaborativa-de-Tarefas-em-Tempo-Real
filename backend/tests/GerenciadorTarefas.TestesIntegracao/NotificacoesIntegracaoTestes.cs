using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class NotificacoesIntegracaoTestes
{
    private static readonly Guid ResponsavelAdministradorId =
        Guid.Parse("8c519a4d-3f6d-4d0b-8b77-6ee8f5735990");

    private static readonly Guid ResponsavelColaboradorId =
        Guid.Parse("f3af6b8c-58de-4225-a1d2-838b22f2d08e");

    [Fact]
    public async Task ListarHistorico_DeveBloquear_QuandoUsuarioNaoAdministradorFiltrarOutroResponsavel()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            cliente,
            "colaborador@gerenciadortarefas.local",
            "Colaborador@123");

        var resposta = await cliente.GetAsync($"/api/notificacoes?responsavelId={ResponsavelAdministradorId:D}");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListarHistorico_DeveRetornarSomenteNotificacoesDoUsuarioAutenticado()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var clienteAdmin = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(clienteAdmin);

        var projetoId = await CriarProjetoAsync(clienteAdmin, "Projeto historico notificacoes");

        await CriarTarefaAsync(
            clienteAdmin,
            projetoId,
            "Tarefa destinada ao colaborador",
            ResponsavelColaboradorId);

        await CriarTarefaAsync(
            clienteAdmin,
            projetoId,
            "Tarefa destinada ao administrador",
            ResponsavelAdministradorId);

        using var clienteColaborador = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            clienteColaborador,
            "colaborador@gerenciadortarefas.local",
            "Colaborador@123");

        var resposta = await clienteColaborador.GetAsync("/api/notificacoes?limite=50");
        resposta.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ClienteApiTeste
            .LerEnvelopeSucessoAsync<List<NotificacaoDadosRespostaTeste>>(resposta);
        envelope.Dados.Should().NotBeNull();
        envelope.Dados!
            .Should()
            .OnlyContain(notificacao => notificacao.ResponsavelId == ResponsavelColaboradorId);
    }

    [Fact]
    public async Task ListarHistorico_DevePermitirFiltroArbitrario_ParaAdministrador()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var clienteAdmin = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(clienteAdmin);

        var resposta = await clienteAdmin.GetAsync($"/api/notificacoes?responsavelId={ResponsavelColaboradorId:D}");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> CriarProjetoAsync(HttpClient cliente, string nome)
    {
        var respostaCriacao = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = nome,
            Descricao = "Projeto de apoio para testes de notificacoes"
        });

        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created);
        var projetoCriado = await ClienteApiTeste.LerEnvelopeSucessoAsync<ProjetoDadosRespostaTeste>(respostaCriacao);
        projetoCriado.Dados.Should().NotBeNull();
        return projetoCriado.Dados!.Id;
    }

    private static async Task CriarTarefaAsync(
        HttpClient cliente,
        Guid projetoId,
        string titulo,
        Guid responsavelId)
    {
        var respostaCriacao = await cliente.PostAsJsonAsync("/api/tarefas", new
        {
            Titulo = titulo,
            Descricao = "Tarefa para validar historico de notificacoes.",
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = projetoId,
            ResponsavelId = responsavelId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        });

        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed class NotificacaoDadosRespostaTeste
    {
        public Guid ResponsavelId { get; init; }
    }
}
