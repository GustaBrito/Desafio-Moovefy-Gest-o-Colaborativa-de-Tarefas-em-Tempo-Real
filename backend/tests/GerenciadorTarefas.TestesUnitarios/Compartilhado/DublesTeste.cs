using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.TestesUnitarios.Compartilhado;

internal sealed class RepositorioProjetoFalso : IRepositorioProjeto
{
    public List<Projeto> Projetos { get; } = [];
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Projeto? ProjetoAdicionado { get; private set; }
    public Projeto? ProjetoAtualizado { get; private set; }
    public Projeto? ProjetoRemovido { get; private set; }
    public IReadOnlyCollection<Projeto>? ResultadoListagemSobrescrito { get; set; }

    public Task<IReadOnlyCollection<Projeto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        if (ResultadoListagemSobrescrito is not null)
        {
            return Task.FromResult(ResultadoListagemSobrescrito);
        }

        return Task.FromResult((IReadOnlyCollection<Projeto>)Projetos.ToList());
    }

    public Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Projetos.FirstOrDefault(projeto => projeto.Id == id));
    }

    public Task AdicionarAsync(Projeto projeto, CancellationToken cancellationToken = default)
    {
        ProjetoAdicionado = projeto;
        Projetos.Add(projeto);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        SalvarAlteracoesFoiChamado = true;
        return Task.CompletedTask;
    }

    public void Atualizar(Projeto projeto)
    {
        ProjetoAtualizado = projeto;
    }

    public void Remover(Projeto projeto)
    {
        ProjetoRemovido = projeto;
        Projetos.Remove(projeto);
    }
}

internal sealed class RepositorioTarefaFalso : IRepositorioTarefa
{
    public List<Tarefa> Tarefas { get; } = [];
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Tarefa? TarefaAdicionada { get; private set; }
    public Tarefa? TarefaAtualizada { get; private set; }
    public Tarefa? TarefaRemovida { get; private set; }
    public ResultadoConsultaTarefas? ResultadoListagemSobrescrito { get; set; }
    public IReadOnlyCollection<Tarefa>? ResultadoListarTodasSobrescrito { get; set; }
    public bool? ExistePorProjetoIdSobrescrito { get; set; }
    public FiltroConsultaTarefas? UltimoFiltroListagem { get; private set; }

    public Task<ResultadoConsultaTarefas> ListarAsync(
        FiltroConsultaTarefas filtroConsulta,
        CancellationToken cancellationToken = default)
    {
        UltimoFiltroListagem = filtroConsulta;

        if (ResultadoListagemSobrescrito is not null)
        {
            return Task.FromResult(ResultadoListagemSobrescrito);
        }

        var itens = Tarefas.Skip(filtroConsulta.Pular).Take(filtroConsulta.Tomar).ToList();
        return Task.FromResult(new ResultadoConsultaTarefas(itens, Tarefas.Count));
    }

    public Task<IReadOnlyCollection<Tarefa>> ListarTodasAsync(CancellationToken cancellationToken = default)
    {
        if (ResultadoListarTodasSobrescrito is not null)
        {
            return Task.FromResult(ResultadoListarTodasSobrescrito);
        }

        return Task.FromResult((IReadOnlyCollection<Tarefa>)Tarefas.ToList());
    }

    public Task<Tarefa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Tarefas.FirstOrDefault(tarefa => tarefa.Id == id));
    }

    public Task<bool> ExistePorProjetoIdAsync(Guid projetoId, CancellationToken cancellationToken = default)
    {
        if (ExistePorProjetoIdSobrescrito.HasValue)
        {
            return Task.FromResult(ExistePorProjetoIdSobrescrito.Value);
        }

        return Task.FromResult(Tarefas.Any(tarefa => tarefa.ProjetoId == projetoId));
    }

    public Task AdicionarAsync(Tarefa tarefa, CancellationToken cancellationToken = default)
    {
        TarefaAdicionada = tarefa;
        Tarefas.Add(tarefa);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        SalvarAlteracoesFoiChamado = true;
        return Task.CompletedTask;
    }

    public void Atualizar(Tarefa tarefa)
    {
        TarefaAtualizada = tarefa;
    }

    public void Remover(Tarefa tarefa)
    {
        TarefaRemovida = tarefa;
        Tarefas.Remove(tarefa);
    }
}

internal sealed class RepositorioNotificacaoFalso : IRepositorioNotificacao
{
    public List<Notificacao> Notificacoes { get; } = [];
    public Guid? UltimoResponsavelIdConsulta { get; private set; }
    public int UltimoLimiteConsulta { get; private set; }
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Notificacao? NotificacaoAdicionada { get; private set; }

    public Task<IReadOnlyCollection<Notificacao>> ListarRecentesAsync(
        Guid? responsavelId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        UltimoResponsavelIdConsulta = responsavelId;
        UltimoLimiteConsulta = limite;

        var consulta = Notificacoes.AsEnumerable();

        if (responsavelId.HasValue)
        {
            consulta = consulta.Where(notificacao => notificacao.ResponsavelId == responsavelId.Value);
        }

        var resultado = consulta
            .OrderByDescending(notificacao => notificacao.DataCriacao)
            .Take(limite)
            .ToList();

        return Task.FromResult((IReadOnlyCollection<Notificacao>)resultado);
    }

    public Task AdicionarAsync(Notificacao notificacao, CancellationToken cancellationToken = default)
    {
        NotificacaoAdicionada = notificacao;
        Notificacoes.Add(notificacao);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        SalvarAlteracoesFoiChamado = true;
        return Task.CompletedTask;
    }
}

internal sealed class NotificadorTempoRealTarefasFalso : INotificadorTempoRealTarefas
{
    public List<RegistroNotificacaoTempoReal> NotificacoesEnviadas { get; } = [];

    public Task NotificarAtribuicaoAsync(
        Guid responsavelId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default)
    {
        NotificacoesEnviadas.Add(new RegistroNotificacaoTempoReal
        {
            ResponsavelId = responsavelId,
            TarefaId = tarefaId,
            ProjetoId = projetoId,
            TituloTarefa = tituloTarefa,
            Reatribuicao = reatribuicao
        });

        return Task.CompletedTask;
    }
}

internal sealed class RegistroNotificacaoTempoReal
{
    public Guid ResponsavelId { get; init; }
    public Guid TarefaId { get; init; }
    public Guid ProjetoId { get; init; }
    public string TituloTarefa { get; init; } = string.Empty;
    public bool Reatribuicao { get; init; }
}
