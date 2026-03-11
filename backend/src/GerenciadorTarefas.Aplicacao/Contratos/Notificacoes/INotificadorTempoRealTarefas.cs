namespace GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;

public interface INotificadorTempoRealTarefas
{
    Task NotificarAtribuicaoAsync(
        Guid responsavelUsuarioId,
        Guid tarefaId,
        Guid projetoId,
        string tituloTarefa,
        bool reatribuicao,
        CancellationToken cancellationToken = default);
}
