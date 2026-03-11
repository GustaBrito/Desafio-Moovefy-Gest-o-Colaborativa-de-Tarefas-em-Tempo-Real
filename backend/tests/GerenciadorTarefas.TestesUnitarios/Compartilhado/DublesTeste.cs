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

    public Task<IReadOnlyCollection<Projeto>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsPermitidas = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Projeto> consulta = ResultadoListagemSobrescrito is not null
            ? ResultadoListagemSobrescrito
            : Projetos;

        if (areaIdsPermitidas is not null && areaIdsPermitidas.Count > 0)
        {
            var conjuntoAreas = areaIdsPermitidas.ToHashSet();
            consulta = consulta.Where(projeto => conjuntoAreas.Contains(projeto.AreaId));
        }

        return Task.FromResult((IReadOnlyCollection<Projeto>)consulta.ToList());
    }

    public Task<Projeto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Projetos.FirstOrDefault(projeto => projeto.Id == id));
    }

    public Task<IReadOnlyCollection<Projeto>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var conjuntoIds = ids.ToHashSet();
        var projetos = Projetos.Where(projeto => conjuntoIds.Contains(projeto.Id)).ToList();
        return Task.FromResult((IReadOnlyCollection<Projeto>)projetos);
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

    public Task<IReadOnlyCollection<Tarefa>> ListarTodasPorAreasAsync(
        IReadOnlyCollection<Guid> areaIds,
        CancellationToken cancellationToken = default)
    {
        var conjuntoAreas = areaIds.ToHashSet();
        var tarefas = Tarefas
            .Where(tarefa => tarefa.Projeto is not null && conjuntoAreas.Contains(tarefa.Projeto.AreaId))
            .ToList();

        return Task.FromResult((IReadOnlyCollection<Tarefa>)tarefas);
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
    public Guid? UltimoResponsavelUsuarioIdConsulta { get; private set; }
    public int UltimoLimiteConsulta { get; private set; }
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Notificacao? NotificacaoAdicionada { get; private set; }

    public Task<IReadOnlyCollection<Notificacao>> ListarRecentesAsync(
        Guid? responsavelUsuarioId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        UltimoResponsavelUsuarioIdConsulta = responsavelUsuarioId;
        UltimoLimiteConsulta = limite;

        var consulta = Notificacoes.AsEnumerable();

        if (responsavelUsuarioId.HasValue)
        {
            consulta = consulta.Where(notificacao =>
                notificacao.ResponsavelUsuarioId == responsavelUsuarioId.Value);
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

internal sealed class RepositorioAreaFalso : IRepositorioArea
{
    public List<Area> Areas { get; } = [];
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Area? AreaAdicionada { get; private set; }
    public Area? AreaAtualizada { get; private set; }

    public Task<IReadOnlyCollection<Area>> ListarAsync(
        bool somenteAtivas = false,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Area> consulta = Areas;
        if (somenteAtivas)
        {
            consulta = consulta.Where(area => area.Ativa);
        }

        return Task.FromResult((IReadOnlyCollection<Area>)consulta.ToList());
    }

    public Task<IReadOnlyCollection<Area>> ListarPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var conjuntoIds = ids.ToHashSet();
        var resultado = Areas.Where(area => conjuntoIds.Contains(area.Id)).ToList();
        return Task.FromResult((IReadOnlyCollection<Area>)resultado);
    }

    public Task<Area?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Areas.FirstOrDefault(area => area.Id == id));
    }

    public Task<Area?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Areas.FirstOrDefault(area =>
            area.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<Area?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Areas.FirstOrDefault(area =>
            string.Equals(area.Codigo, codigo, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AdicionarAsync(Area area, CancellationToken cancellationToken = default)
    {
        AreaAdicionada = area;
        Areas.Add(area);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        SalvarAlteracoesFoiChamado = true;
        return Task.CompletedTask;
    }

    public void Atualizar(Area area)
    {
        AreaAtualizada = area;
    }
}

internal sealed class RepositorioUsuarioFalso : IRepositorioUsuario
{
    public List<Usuario> Usuarios { get; } = [];
    public List<UsuarioArea> VinculosUsuarioArea { get; } = [];
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Usuario? UsuarioAdicionado { get; private set; }
    public Usuario? UsuarioAtualizado { get; private set; }

    public Task<IReadOnlyCollection<Usuario>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyCollection<Usuario>)Usuarios.ToList());
    }

    public Task<IReadOnlyCollection<Usuario>> ListarPorAreasAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        var conjuntoAreas = areaIds.ToHashSet();
        var usuarioIds = VinculosUsuarioArea
            .Where(vinculo => conjuntoAreas.Contains(vinculo.AreaId))
            .Select(vinculo => vinculo.UsuarioId)
            .ToHashSet();

        IEnumerable<Usuario> consulta = Usuarios.Where(usuario => usuarioIds.Contains(usuario.Id));
        if (somenteAtivos)
        {
            consulta = consulta.Where(usuario => usuario.Ativo);
        }

        return Task.FromResult((IReadOnlyCollection<Usuario>)consulta.ToList());
    }

    public Task<IReadOnlyCollection<Usuario>> ObterPorIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var conjuntoIds = ids.ToHashSet();
        var usuarios = Usuarios.Where(usuario => conjuntoIds.Contains(usuario.Id)).ToList();
        return Task.FromResult((IReadOnlyCollection<Usuario>)usuarios);
    }

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Usuarios.FirstOrDefault(usuario => usuario.Id == id));
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Usuarios.FirstOrDefault(usuario =>
            usuario.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
    }

    public Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        UsuarioAdicionado = usuario;
        Usuarios.Add(usuario);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        SalvarAlteracoesFoiChamado = true;
        return Task.CompletedTask;
    }

    public void Atualizar(Usuario usuario)
    {
        UsuarioAtualizado = usuario;
    }
}

internal sealed class RepositorioUsuarioAreaFalso : IRepositorioUsuarioArea
{
    public List<UsuarioArea> Vinculos { get; } = [];
    public List<Usuario> Usuarios { get; } = [];
    public bool SalvarAlteracoesFoiChamado { get; private set; }
    public Guid? UltimoUsuarioRemovido { get; private set; }

    public Task<IReadOnlyCollection<UsuarioArea>> ListarPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var resultado = Vinculos.Where(vinculo => vinculo.UsuarioId == usuarioId).ToList();
        return Task.FromResult((IReadOnlyCollection<UsuarioArea>)resultado);
    }

    public Task<IReadOnlyCollection<Guid>> ListarAreaIdsPorUsuarioIdAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var areaIds = Vinculos
            .Where(vinculo => vinculo.UsuarioId == usuarioId)
            .Select(vinculo => vinculo.AreaId)
            .Distinct()
            .ToArray();
        return Task.FromResult((IReadOnlyCollection<Guid>)areaIds);
    }

    public Task<IReadOnlyCollection<Guid>> ListarUsuarioIdsPorAreaIdsAsync(
        IReadOnlyCollection<Guid> areaIds,
        bool somenteAtivos = false,
        CancellationToken cancellationToken = default)
    {
        var conjuntoAreas = areaIds.ToHashSet();
        var consulta = Vinculos.Where(vinculo => conjuntoAreas.Contains(vinculo.AreaId));

        var usuarioIds = consulta
            .Select(vinculo => vinculo.UsuarioId)
            .Distinct()
            .Where(usuarioId =>
                !somenteAtivos
                || Usuarios.FirstOrDefault(usuario => usuario.Id == usuarioId)?.Ativo == true)
            .ToArray();

        return Task.FromResult((IReadOnlyCollection<Guid>)usuarioIds);
    }

    public Task<bool> UsuarioPertenceAreaAsync(
        Guid usuarioId,
        Guid areaId,
        CancellationToken cancellationToken = default)
    {
        var resultado = Vinculos.Any(vinculo =>
            vinculo.UsuarioId == usuarioId && vinculo.AreaId == areaId);
        return Task.FromResult(resultado);
    }

    public Task RemoverPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        UltimoUsuarioRemovido = usuarioId;
        Vinculos.RemoveAll(vinculo => vinculo.UsuarioId == usuarioId);
        return Task.CompletedTask;
    }

    public Task AdicionarEmLoteAsync(
        IReadOnlyCollection<UsuarioArea> vinculos,
        CancellationToken cancellationToken = default)
    {
        Vinculos.AddRange(vinculos);
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
        Guid responsavelUsuarioId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default)
    {
        NotificacoesEnviadas.Add(new RegistroNotificacaoTempoReal
        {
            ResponsavelUsuarioId = responsavelUsuarioId,
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
    public Guid ResponsavelUsuarioId { get; init; }
    public Guid TarefaId { get; init; }
    public Guid ProjetoId { get; init; }
    public string TituloTarefa { get; init; } = string.Empty;
    public bool Reatribuicao { get; init; }
}
