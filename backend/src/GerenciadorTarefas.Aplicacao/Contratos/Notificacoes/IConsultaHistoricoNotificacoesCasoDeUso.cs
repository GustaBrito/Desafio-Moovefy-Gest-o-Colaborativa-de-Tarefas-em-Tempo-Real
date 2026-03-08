using GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;

namespace GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;

public interface IConsultaHistoricoNotificacoesCasoDeUso
{
    Task<IReadOnlyCollection<NotificacaoResposta>> ListarAsync(
        ConsultaHistoricoNotificacoesEntrada? entrada = null,
        CancellationToken cancellationToken = default);
}
