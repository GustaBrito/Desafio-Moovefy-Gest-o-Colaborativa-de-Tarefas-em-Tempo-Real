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

    public ConsultaTarefasCasoDeUso(IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioTarefa = repositorioTarefa;
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

        var tarefas = resultadoConsulta.Itens
            .Select(tarefa => MapearParaResposta(tarefa, dataReferencia))
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

        return MapearParaResposta(tarefa, DateTime.UtcNow);
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

        if (filtro.ResponsavelId.HasValue && filtro.ResponsavelId.Value == Guid.Empty)
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
            ResponsavelId = filtro.ResponsavelId,
            DataPrazoInicial = filtro.DataPrazoInicial,
            DataPrazoFinal = filtro.DataPrazoFinal,
            CampoOrdenacao = filtro.CampoOrdenacao ?? CampoOrdenacaoTarefa.DataCriacao,
            DirecaoOrdenacao = filtro.DirecaoOrdenacao ?? DirecaoOrdenacaoTarefa.Descendente,
            Pular = parametrosPaginacao.Pular,
            Tomar = parametrosPaginacao.Tomar
        };
    }

    private static TarefaResposta MapearParaResposta(Tarefa tarefa, DateTime dataReferencia)
    {
        return new TarefaResposta
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Status = tarefa.Status,
            Prioridade = tarefa.Prioridade,
            ProjetoId = tarefa.ProjetoId,
            ResponsavelId = tarefa.ResponsavelId,
            DataCriacao = tarefa.DataCriacao,
            DataPrazo = tarefa.DataPrazo,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(dataReferencia)
        };
    }
}
