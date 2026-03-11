using GerenciadorTarefas.Aplicacao.Contratos.Tarefas;
using GerenciadorTarefas.Aplicacao.Modelos.Tarefas;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Tarefas;

public sealed class AtualizarStatusTarefaCasoDeUso : IAtualizarStatusTarefaCasoDeUso
{
    private readonly IRepositorioTarefa repositorioTarefa;
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioArea repositorioArea;

    public AtualizarStatusTarefaCasoDeUso(
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

    public async Task<TarefaResposta> ExecutarAsync(
        Guid id,
        AtualizarStatusTarefaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da tarefa deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        if (!Enum.IsDefined(entrada.Status))
        {
            throw new ArgumentException("O status informado e invalido.", nameof(entrada));
        }

        var tarefa = await repositorioTarefa.ObterPorIdAsync(id, cancellationToken);
        if (tarefa is null)
        {
            throw new KeyNotFoundException($"Tarefa com id '{id}' nao foi encontrada.");
        }

        var dataAtual = DateTime.UtcNow;
        tarefa.AtualizarStatus(entrada.Status, dataAtual);

        repositorioTarefa.Atualizar(tarefa);
        await repositorioTarefa.SalvarAlteracoesAsync(cancellationToken);

        var projeto = await repositorioProjeto.ObterPorIdAsync(tarefa.ProjetoId, cancellationToken);
        var responsavel = await repositorioUsuario.ObterPorIdAsync(tarefa.ResponsavelUsuarioId, cancellationToken);
        var area = projeto is null
            ? null
            : await repositorioArea.ObterPorIdAsync(projeto.AreaId, cancellationToken);

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
            AreaNome = area?.Nome,
            DataCriacao = tarefa.DataCriacao,
            DataPrazo = tarefa.DataPrazo,
            DataConclusao = tarefa.DataConclusao,
            EstaAtrasada = tarefa.EstaAtrasada(dataAtual)
        };
    }
}
