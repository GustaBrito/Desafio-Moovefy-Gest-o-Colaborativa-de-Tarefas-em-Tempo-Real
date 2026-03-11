using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class CriarTarefaCasoDeUso : ICriarTarefaCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioTarefa repositorioTarefa;
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly INotificadorTempoRealTarefas notificadorTempoRealTarefas;

    public CriarTarefaCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioTarefa repositorioTarefa,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioArea repositorioArea,
        IRepositorioUsuarioArea repositorioUsuarioArea,
        INotificadorTempoRealTarefas notificadorTempoRealTarefas)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioTarefa = repositorioTarefa;
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioArea = repositorioArea;
        this.repositorioUsuarioArea = repositorioUsuarioArea;
        this.notificadorTempoRealTarefas = notificadorTempoRealTarefas;
    }

    public async Task<TarefaResposta> ExecutarAsync(
        CriarTarefaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        if (entrada.ProjetoId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do projeto deve ser informado.", nameof(entrada));
        }

        if (entrada.ResponsavelUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("O identificador do responsavel deve ser informado.", nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Prioridade))
        {
            throw new ArgumentException("A prioridade informada e invalida.", nameof(entrada));
        }

        var tituloNormalizado = entrada.Titulo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tituloNormalizado))
        {
            throw new ArgumentException("O titulo da tarefa deve ser informado.", nameof(entrada));
        }

        if (tituloNormalizado.Length > 200)
        {
            throw new ArgumentException("O titulo da tarefa deve ter no maximo 200 caracteres.", nameof(entrada));
        }

        var descricaoNormalizada = string.IsNullOrWhiteSpace(entrada.Descricao)
            ? null
            : entrada.Descricao.Trim();

        if (descricaoNormalizada is not null && descricaoNormalizada.Length > 2000)
        {
            throw new ArgumentException("A descricao da tarefa deve ter no maximo 2000 caracteres.", nameof(entrada));
        }

        if (entrada.DataPrazo <= DateTime.MinValue)
        {
            throw new ArgumentException("A data de prazo da tarefa deve ser informada.", nameof(entrada));
        }

        var projeto = await repositorioProjeto.ObterPorIdAsync(entrada.ProjetoId, cancellationToken);
        if (projeto is null)
        {
            throw new KeyNotFoundException($"Projeto com id '{entrada.ProjetoId}' nao foi encontrado.");
        }

        var responsavel = await repositorioUsuario.ObterPorIdAsync(entrada.ResponsavelUsuarioId, cancellationToken);
        if (responsavel is null)
        {
            throw new ArgumentException("O responsavel informado nao existe.", nameof(entrada));
        }

        if (!responsavel.Ativo)
        {
            throw new InvalidOperationException("Nao e permitido atribuir tarefas para usuario inativo.");
        }

        var pertenceAreaProjeto = await repositorioUsuarioArea.UsuarioPertenceAreaAsync(
            responsavel.Id,
            projeto.AreaId,
            cancellationToken);

        if (!pertenceAreaProjeto)
        {
            throw new InvalidOperationException(
                "O responsavel da tarefa deve pertencer a area do projeto.");
        }

        var dataAtual = DateTime.UtcNow;
        var tarefa = new Tarefa
        {
            Id = Guid.NewGuid(),
            Titulo = tituloNormalizado,
            Descricao = descricaoNormalizada,
            Status = StatusTarefa.Pendente,
            Prioridade = entrada.Prioridade,
            ProjetoId = entrada.ProjetoId,
            ResponsavelUsuarioId = entrada.ResponsavelUsuarioId,
            DataCriacao = dataAtual,
            DataPrazo = entrada.DataPrazo,
            DataConclusao = null
        };

        await repositorioTarefa.AdicionarAsync(tarefa, cancellationToken);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);
        await notificadorTempoRealTarefas.NotificarAtribuicaoAsync(
            tarefa.ResponsavelUsuarioId,
            tarefa.Id,
            tarefa.ProjetoId,
            tarefa.Titulo,
            reatribuicao: false,
            cancellationToken);

        var area = await repositorioArea.ObterPorIdAsync(projeto.AreaId, cancellationToken);
        return MapearParaResposta(tarefa, responsavel, area?.Nome, dataAtual);
    }

    private static TarefaResposta MapearParaResposta(
        Tarefa tarefa,
        Usuario responsavel,
        string? areaNome,
        DateTime dataReferencia)
    {
        return new TarefaResposta
        {
            Id = tarefa.Id,
            Titulo = tarefa.Titulo,
            Descricao = tarefa.Descricao,
            Status = tarefa.Status,
            Prioridade = tarefa.Prioridade,
            ProjetoId = tarefa.ProjetoId,
            ResponsavelUsuarioId = tarefa.ResponsavelUsuarioId,
            ResponsavelNome = responsavel.Nome,
            ResponsavelEmail = responsavel.Email,
            AreaNome = areaNome,
            DataCriacao = tarefa.DataCriacao,
            DataPrazo = tarefa.DataPrazo,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(dataReferencia)
        };
    }
}
