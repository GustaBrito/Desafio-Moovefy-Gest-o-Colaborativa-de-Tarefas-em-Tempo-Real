using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Paginacao;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Modelos.Tarefas;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class ConsultaTarefasCasoDeUso : IConsultaTarefasCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioArea repositorioArea;

    public ConsultaTarefasCasoDeUso(
        IRepositorioTarefa repositorioTarefa,
        IRepositorioProjeto repositorioProjeto,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioArea repositorioArea)
    {
        this.repositorioTarefa = repositorioTarefa;
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioArea = repositorioArea;
    }

    public async Task<ResultadoPaginado<TarefaResposta>> ListarAsync(
        FiltroConsultaTarefasEntrada? filtro = null,
        CancellationToken cancellationToken = default)
    {
        var parametrosPaginacao = new ParametrosPaginacao
        {
            NumeroPagina = filtro?.NumeroPagina ?? ParametrosPaginacao.NumeroPaginaPadrao,
            TamanhoPagina = filtro?.TamanhoPagina ?? ParametrosPaginacao.TamanhoPaginaPadrao
        };

        var filtroNormalizado = CriarFiltroConsulta(filtro, parametrosPaginacao);
        var resultadoConsulta = await repositorioTarefa.ListarAsync(filtroNormalizado, cancellationToken);
        var dataReferencia = DateTime.UtcNow;
        var dadosEnriquecimento = await CarregarDadosEnriquecimentoAsync(
            resultadoConsulta.Itens,
            cancellationToken);

        var tarefas = resultadoConsulta.Itens
            .Select(tarefa => MapearParaResposta(tarefa, dataReferencia, dadosEnriquecimento))
            .ToList();

        return ResultadoPaginado<TarefaResposta>.Criar(
            tarefas,
            resultadoConsulta.TotalRegistros,
            parametrosPaginacao);
    }

    public async Task<TarefaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da tarefa deve ser informado.", nameof(id));
        }

        var tarefa = await repositorioTarefa.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null)
        {
            throw new KeyNotFoundException($"Tarefa com id '{id}' nao foi encontrada.");
        }

        var dadosEnriquecimento = await CarregarDadosEnriquecimentoAsync([tarefa], cancellationToken);
        return MapearParaResposta(tarefa, DateTime.UtcNow, dadosEnriquecimento);
    }

    private static FiltroConsultaTarefas CriarFiltroConsulta(
        FiltroConsultaTarefasEntrada? filtro,
        ParametrosPaginacao parametrosPaginacao)
    {
        if (filtro is null)
        {
            return new FiltroConsultaTarefas
            {
                Pular = parametrosPaginacao.Pular,
                Tomar = parametrosPaginacao.Tomar
            };
        }

        if (filtro.ProjetoId.HasValue && filtro.ProjetoId.Value == Guid.Empty)
        {
            throw new ArgumentException("Quando informado, o identificador do projeto deve ser valido.", nameof(filtro));
        }

        if (filtro.ResponsavelUsuarioId.HasValue && filtro.ResponsavelUsuarioId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Quando informado, o identificador do responsavel deve ser valido.",
                nameof(filtro));
        }

        if (filtro.DataPrazoInicial.HasValue && filtro.DataPrazoFinal.HasValue
            && filtro.DataPrazoInicial.Value > filtro.DataPrazoFinal.Value)
        {
            throw new ArgumentException(
                "A data de prazo inicial nao pode ser maior que a data de prazo final.",
                nameof(filtro));
        }

        return new FiltroConsultaTarefas
        {
            ProjetoId = filtro.ProjetoId,
            Status = filtro.Status,
            ResponsavelUsuarioId = filtro.ResponsavelUsuarioId,
            AreasProjetoPermitidas = filtro.AreasProjetoPermitidas,
            DataPrazoInicial = filtro.DataPrazoInicial,
            DataPrazoFinal = filtro.DataPrazoFinal,
            CampoOrdenacao = filtro.CampoOrdenacao ?? CampoOrdenacaoTarefa.DataCriacao,
            DirecaoOrdenacao = filtro.DirecaoOrdenacao ?? DirecaoOrdenacaoTarefa.Descendente,
            Pular = parametrosPaginacao.Pular,
            Tomar = parametrosPaginacao.Tomar
        };
    }

    private async Task<DadosEnriquecimentoTarefa> CarregarDadosEnriquecimentoAsync(
        IReadOnlyCollection<Tarefa> tarefas,
        CancellationToken cancellationToken)
    {
        var projetoIds = tarefas.Select(tarefa => tarefa.ProjetoId).Distinct().ToArray();
        var responsavelIds = tarefas.Select(tarefa => tarefa.ResponsavelUsuarioId).Distinct().ToArray();

        var projetos = await repositorioProjeto.ObterPorIdsAsync(projetoIds, cancellationToken);
        var usuarios = await repositorioUsuario.ObterPorIdsAsync(responsavelIds, cancellationToken);
        var areaIds = projetos.Select(projeto => projeto.AreaId).Distinct().ToArray();
        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);

        return new DadosEnriquecimentoTarefa
        {
            UsuariosPorId = usuarios.ToDictionary(usuario => usuario.Id),
            ProjetosPorId = projetos.ToDictionary(projeto => projeto.Id),
            AreasPorId = areas.ToDictionary(area => area.Id)
        };
    }

    private static TarefaResposta MapearParaResposta(
        Tarefa tarefa,
        DateTime dataReferencia,
        DadosEnriquecimentoTarefa dadosEnriquecimento)
    {
        dadosEnriquecimento.UsuariosPorId.TryGetValue(tarefa.ResponsavelUsuarioId, out var responsavel);
        dadosEnriquecimento.ProjetosPorId.TryGetValue(tarefa.ProjetoId, out var projeto);
        var areaNome = projeto is not null
            && dadosEnriquecimento.AreasPorId.TryGetValue(projeto.AreaId, out var area)
            ? area.Nome
            : null;

        return new TarefaResposta
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Status = tarefa.Status,
            Prioridade = tarefa.Prioridade,
            ProjetoId = tarefa.ProjetoId,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            ResponsavelNome = responsavel?.Nome,
            ResponsavelEmail = responsavel?.Email,
            AreaNome = areaNome,
            DataCriacao = tarefa.DataCriacao,
            DataPrazo = tarefa.DataPrazo,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(dataReferencia)
        };
    }

    private sealed class DadosEnriquecimentoTarefa
    {
        public IReadOnlyDictionary<Guid, Usuario> UsuariosPorId { get; init; }
            = new Dictionary<Guid, Usuario>();

        public IReadOnlyDictionary<Guid, Projeto> ProjetosPorId { get; init; }
            = new Dictionary<Guid, Projeto>();

        public IReadOnlyDictionary<Guid, Area> AreasPorId { get; init; }
            = new Dictionary<Guid, Area>();
    }
}
