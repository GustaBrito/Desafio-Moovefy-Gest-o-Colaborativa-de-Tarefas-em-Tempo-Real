using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class ProjetosIntegracaoTestes
{
    private const string ResponsavelAdministradorId = "8c519a4d-3f6d-4d0b-8b77-6ee8f5735990";

    [Fact]
    public async Task FluxoProjetos_DeveCriarAtualizarListarEExcluir()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var respostaCriacao = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = "Projeto Integracao",
            Descricao = "Criado em teste de integracao"
        });

        var corpoCriacao = await respostaCriacao.Content.ReadAsStringAsync();
        respostaCriacao.StatusCode.Should().Be(HttpStatusCode.Created, corpoCriacao);
        var projetoCriado = await ClienteApiTeste.LerEnvelopeSucessoAsync<ProjetoDadosRespostaTeste>(respostaCriacao);
        projetoCriado.Dados.Should().NotBeNull();
        var projetoId = projetoCriado.Dados!.Id;

        var respostaAtualizacao = await cliente.PutAsJsonAsync($"/api/projetos/{projetoId}", new
        {
            Nome = "Projeto Integracao Atualizado",
            Descricao = "Descricao atualizada"
        });

        respostaAtualizacao.StatusCode.Should().Be(HttpStatusCode.OK);

        var respostaLista = await cliente.GetAsync("/api/projetos");
        respostaLista.StatusCode.Should().Be(HttpStatusCode.OK);

        var listaProjetos = await ClienteApiTeste.LerEnvelopeSucessoAsync<List<ProjetoDadosRespostaTeste>>(respostaLista);
        listaProjetos.Dados.Should().NotBeNull();
        listaProjetos.Dados!
            .Any(projeto => projeto.Id == projetoId && projeto.Nome == "Projeto Integracao Atualizado")
            .Should()
            .BeTrue();

        var respostaExclusao = await cliente.DeleteAsync($"/api/projetos/{projetoId}");
        respostaExclusao.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExcluirProjeto_DeveBloquear_QuandoExistiremTarefasVinculadas()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var respostaProjeto = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = "Projeto com tarefas",
            Descricao = "Regra de bloqueio"
        });

        var projetoCriado = await ClienteApiTeste.LerEnvelopeSucessoAsync<ProjetoDadosRespostaTeste>(respostaProjeto);
        var projetoId = projetoCriado.Dados!.Id;

        var respostaTarefa = await cliente.PostAsJsonAsync("/api/tarefas", new
        {
            Titulo = "Tarefa vinculada",
            Descricao = "Impede exclusao",
            Prioridade = 2,
            ProjetoId = projetoId,
            ResponsavelId = Guid.Parse(ResponsavelAdministradorId),
            DataPrazo = DateTime.UtcNow.AddDays(3)
        });
        respostaTarefa.StatusCode.Should().Be(HttpStatusCode.Created);

        var respostaExclusao = await cliente.DeleteAsync($"/api/projetos/{projetoId}");

        respostaExclusao.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var erro = await ClienteApiTeste.LerEnvelopeErroAsync(respostaExclusao);
        erro.Codigo.Should().Be("operacao_invalida");
    }
}
