using System.Net;
using System.Net.Http.Json;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;
using GerenciadorTarefas.TestesIntegracao.Infraestrutura;

namespace GerenciadorTarefas.TestesIntegracao;

public sealed class AdministracaoIntegracaoTestes
{
    [Fact]
    public async Task SuperAdmin_DeveCriarAreaComRetornoCreated()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/areas", new
        {
            Nome = $"Area Integracao {Guid.NewGuid():N}"[..20],
            Codigo = $"IT{Guid.NewGuid():N}"[..8],
            Ativa = true
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        resposta.Headers.Location.Should().NotBeNull();
        resposta.Headers.Location!.ToString().Should().Contain("/api/areas/");
    }

    [Fact]
    public async Task Admin_NaoDeveCriarArea()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            cliente,
            "admin.marketing@gerenciadortarefas.local",
            "AdminMarketing@123");

        var resposta = await cliente.PostAsJsonAsync("/api/areas", new
        {
            Nome = "Area nao permitida",
            Codigo = "ADMX",
            Ativa = true
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdmin_DeveReceberBadRequest_QuandoCriarAreaSemNome()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/areas", new
        {
            Nome = "",
            Codigo = "INV",
            Ativa = true
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Admin_DeveCriarColaboradorNoEscopoComRetornoCreated()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            cliente,
            "admin.marketing@gerenciadortarefas.local",
            "AdminMarketing@123");

        var resposta = await cliente.PostAsJsonAsync("/api/usuarios", new
        {
            Nome = "Colaborador Integracao",
            Email = $"colab.integracao.{Guid.NewGuid():N}@gerenciadortarefas.local",
            Senha = "SenhaForte@123",
            PerfilGlobal = PerfilGlobalUsuario.Colaborador,
            Ativo = true,
            AreaIds = new[] { SemeadorDadosDemonstracao.AreaMarketingId }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        resposta.Headers.Location.Should().NotBeNull();
        resposta.Headers.Location!.ToString().Should().Contain("/api/usuarios/");
    }

    [Fact]
    public async Task Admin_NaoDeveCriarSuperAdmin()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            cliente,
            "admin.marketing@gerenciadortarefas.local",
            "AdminMarketing@123");

        var resposta = await cliente.PostAsJsonAsync("/api/usuarios", new
        {
            Nome = "Super Admin Indevido",
            Email = $"superadmin.indevido.{Guid.NewGuid():N}@gerenciadortarefas.local",
            Senha = "SenhaForte@123",
            PerfilGlobal = PerfilGlobalUsuario.SuperAdmin,
            Ativo = true,
            AreaIds = new[] { SemeadorDadosDemonstracao.AreaMarketingId }
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Colaborador_NaoDeveAcessarListagemUsuarios()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(
            cliente,
            "colaborador.marketing@gerenciadortarefas.local",
            "ColaboradorMkt@123");

        var resposta = await cliente.GetAsync("/api/usuarios");

        resposta.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CriarProjeto_ComAreasEUsuariosVinculados_DevePersistirVinculos()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = $"Projeto Integracao MultiArea {Guid.NewGuid():N}"[..30],
            Descricao = "Projeto para validar areaIds e usuarios vinculados.",
            AreaIds = new[]
            {
                SemeadorDadosDemonstracao.AreaGeralId,
                SemeadorDadosDemonstracao.AreaMarketingId
            },
            GestorUsuarioId = SemeadorDadosDemonstracao.UsuarioAdminMarketingId,
            UsuarioIdsVinculados = new[]
            {
                SemeadorDadosDemonstracao.UsuarioColaboradorMarketingId
            }
        });

        var corpo = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.Created, corpo);
        var projeto = await ClienteApiTeste.LerEnvelopeSucessoAsync<ProjetoDadosRespostaTeste>(resposta);
        projeto.Dados.Should().NotBeNull();
        projeto.Dados!.AreaIds.Should().Contain(SemeadorDadosDemonstracao.AreaGeralId);
        projeto.Dados.AreaIds.Should().Contain(SemeadorDadosDemonstracao.AreaMarketingId);
        projeto.Dados.UsuarioIdsVinculados.Should().Contain(SemeadorDadosDemonstracao.UsuarioColaboradorMarketingId);
        projeto.Dados.UsuarioIdsVinculados.Should().Contain(SemeadorDadosDemonstracao.UsuarioSuperAdminId);
    }

    [Fact]
    public async Task CriarProjeto_SemArea_DeveRetornarBadRequest()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        using var cliente = fabrica.CreateClient();
        await ClienteApiTeste.ConfigurarAutorizacaoAsync(cliente);

        var resposta = await cliente.PostAsJsonAsync("/api/projetos", new
        {
            Nome = "Projeto invalido",
            Descricao = "Sem area",
            AreaId = Guid.Empty
        });

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
