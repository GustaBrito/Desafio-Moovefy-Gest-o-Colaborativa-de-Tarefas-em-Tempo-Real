using GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.TestesUnitarios.Compartilhado;

namespace GerenciadorTarefas.TestesUnitarios.Aplicacao.Tarefas;

public sealed class AtualizarTarefaCasoDeUsoTestes
{
    private readonly RepositorioProjetoFalso repositorioProjeto = new();
    private readonly RepositorioTarefaFalso repositorioTarefa = new();
    private readonly RepositorioUsuarioFalso repositorioUsuario = new();
    private readonly RepositorioAreaFalso repositorioArea = new();
    private readonly RepositorioUsuarioAreaFalso repositorioUsuarioArea = new();
    private readonly NotificadorTempoRealTarefasFalso notificador = new();
    private readonly Area areaPadrao = new()
    {
        Id = Guid.NewGuid(),
        Nome = "Area Teste",
        Ativa = true
    };
    private readonly AtualizarTarefaCasoDeUso casoDeUso;

    public AtualizarTarefaCasoDeUsoTestes()
    {
        repositorioArea.Areas.Add(areaPadrao);
        casoDeUso = new AtualizarTarefaCasoDeUso(
            repositorioTarefa,
            repositorioProjeto,
            repositorioUsuario,
            repositorioArea,
            repositorioUsuarioArea,
            notificador);
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoIdForVazio()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.Empty, CriarEntradaValida(Guid.NewGuid()));

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoEntradaForNula()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), null!);

        await acao.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTarefaNaoExistir()
    {
        var acao = async () => await casoDeUso.ExecutarAsync(Guid.NewGuid(), CriarEntradaValida(Guid.NewGuid()));

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloForInvalido()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = CriarEntradaValida(tarefa.ResponsavelUsuarioId);
        entrada = new AtualizarTarefaEntrada
        {
            Titulo = " ",
            Descricao = entrada.Descricao,
            Status = entrada.Status,
            Prioridade = entrada.Prioridade,
            ResponsavelUsuarioId = entrada.ResponsavelUsuarioId,
            DataPrazo = entrada.DataPrazo
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTituloExcederLimite()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = new string('x', 201),
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDescricaoExcederLimite()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = new string('x', 2001),
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoStatusForInvalido()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = (StatusTarefa)999,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoPrioridadeForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = (PrioridadeTarefa)999,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoResponsavelForVazio()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = Guid.Empty,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoDataPrazoForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.MinValue
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveLancarExcecao_QuandoTransicaoStatusForInvalida()
    {
        var tarefa = CriarTarefa(StatusTarefa.Pendente, Guid.NewGuid());
        repositorioTarefa.Tarefas.Add(tarefa);
        AdicionarContextoProjetoEUsuarios(tarefa.ProjetoId, tarefa.ResponsavelUsuarioId);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo",
            Descricao = "Descricao",
            Status = StatusTarefa.Concluida,
            Prioridade = PrioridadeTarefa.Media,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(3)
        };

        var acao = async () => await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        await acao.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecutarAsync_DeveAtualizarSemNotificar_QuandoResponsavelNaoMudar()
    {
        var responsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavel);
        repositorioTarefa.Tarefas.Add(tarefa);
        AdicionarContextoProjetoEUsuarios(tarefa.ProjetoId, responsavel);

        var resposta = await casoDeUso.ExecutarAsync(tarefa.Id, CriarEntradaValida(responsavel));

        resposta.Titulo.Should().Be("Tarefa atualizada");
        resposta.Status.Should().Be(StatusTarefa.EmAndamento);
        repositorioTarefa.TarefaAtualizada.Should().BeSameAs(tarefa);
        repositorioTarefa.SalvarAlteracoesFoiChamado.Should().BeTrue();
        notificador.NotificacoesEnviadas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_DeveNotificar_QuandoResponsavelMudar()
    {
        var responsavelAnterior = Guid.NewGuid();
        var novoResponsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavelAnterior);
        repositorioTarefa.Tarefas.Add(tarefa);
        AdicionarContextoProjetoEUsuarios(tarefa.ProjetoId, responsavelAnterior, novoResponsavel);

        await casoDeUso.ExecutarAsync(tarefa.Id, CriarEntradaValida(novoResponsavel));

        notificador.NotificacoesEnviadas.Should().ContainSingle();
        notificador.NotificacoesEnviadas[0].ResponsavelUsuarioId.Should().Be(novoResponsavel);
        notificador.NotificacoesEnviadas[0].Reatribuicao.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarAsync_DeveDefinirDescricaoNula_QuandoDescricaoForEspacos()
    {
        var responsavel = Guid.NewGuid();
        var tarefa = CriarTarefa(StatusTarefa.Pendente, responsavel);
        repositorioTarefa.Tarefas.Add(tarefa);
        AdicionarContextoProjetoEUsuarios(tarefa.ProjetoId, responsavel);

        var entrada = new AtualizarTarefaEntrada
        {
            Titulo = "Titulo novo",
            Descricao = "   ",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Alta,
            ResponsavelUsuarioId = responsavel,
            DataPrazo = DateTime.UtcNow.AddDays(2)
        };

        var resposta = await casoDeUso.ExecutarAsync(tarefa.Id, entrada);

        resposta.Descricao.Should().BeNull();
        tarefa.Descricao.Should().BeNull();
    }

    private static AtualizarTarefaEntrada CriarEntradaValida(Guid ResponsavelUsuarioId)
    {
        return new AtualizarTarefaEntrada
        {
            Titulo = "  Tarefa atualizada  ",
            Descricao = "  Descricao nova  ",
            Status = StatusTarefa.EmAndamento,
            Prioridade = PrioridadeTarefa.Urgente,
            ResponsavelUsuarioId = ResponsavelUsuarioId,
            DataPrazo = DateTime.UtcNow.AddDays(4)
        };
    }

    private static Tarefa CriarTarefa(StatusTarefa status, Guid ResponsavelUsuarioId)
    {
        return new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = "Titulo original",
            Descricao = "Descricao original",
            Status = status,
            Prioridade = PrioridadeTarefa.Media,
            ProjetoId = Guid.NewGuid(),
            ResponsavelUsuarioId = ResponsavelUsuarioId,
            DataCriacao = DateTime.UtcNow.AddDays(-3),
            DataPrazo = DateTime.UtcNow.AddDays(5)
        };
    }

    private void AdicionarContextoProjetoEUsuarios(Guid projetoId, params Guid[] responsavelIds)
    {
        if (repositorioProjeto.Projetos.All(projeto => projeto.Id != projetoId))
        {
            repositorioProjeto.Projetos.Add(new Projeto
            {
                Id = projetoId,
                Nome = "Projeto teste",
                AreaId = areaPadrao.Id,
                DataCriacao = DateTime.UtcNow.AddDays(-10)
            });
        }

        foreach (var responsavelId in responsavelIds.Distinct())
        {
            if (repositorioUsuario.Usuarios.All(usuario => usuario.Id != responsavelId))
            {
                var usuario = new Usuario
                {
                    Id = responsavelId,
                    Nome = $"Usuario {responsavelId:N}"[..14],
                    Email = $"{responsavelId:N}@local",
                    SenhaHash = "hash",
                    Ativo = true,
                    PerfilGlobal = PerfilGlobalUsuario.Colaborador,
                    DataCriacao = DateTime.UtcNow.AddDays(-30)
                };
                repositorioUsuario.Usuarios.Add(usuario);
                repositorioUsuarioArea.Usuarios.Add(usuario);
            }

            if (repositorioUsuarioArea.Vinculos.All(vinculo =>
                vinculo.UsuarioId != responsavelId || vinculo.AreaId != areaPadrao.Id))
            {
                repositorioUsuarioArea.Vinculos.Add(new UsuarioArea
                {
                    UsuarioId = responsavelId,
                    AreaId = areaPadrao.Id
                });
            }
        }
    }
}
