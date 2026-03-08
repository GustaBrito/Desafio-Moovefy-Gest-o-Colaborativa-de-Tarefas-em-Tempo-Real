namespace GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;

public interface INotificadorTempoRealTarefas
{
    Task NotificarAtribuicaoAsync(
        Guid responsavelId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default);
}
