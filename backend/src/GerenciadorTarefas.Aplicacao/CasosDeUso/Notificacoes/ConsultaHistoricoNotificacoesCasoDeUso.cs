using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Notificacoes;

public sealed class ConsultaHistoricoNotificacoesCasoDeUso : IConsultaHistoricoNotificacoesCasoDeUso
{
    private const int LimitePadrao = 50;
    private const int LimiteMaximo = 200;
    private readonly IRepositorioNotificacao repositorioNotificacao;

    public ConsultaHistoricoNotificacoesCasoDeUso(IRepositorioNotificacao repositorioNotificacao)
    {
        this.repositorioNotificacao = repositorioNotificacao;
    }

    public async Task<IReadOnlyCollection<NotificacaoResposta>> ListarAsync(
        ConsultaHistoricoNotificacoesEntrada? entrada = null,
        CancellationToken cancellationToken = default)
    {
        if (entrada?.ResponsavelId.HasValue == true && entrada.ResponsavelId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Quando informado, o identificador do responsavel deve ser valido.",
                nameof(entrada));
        }

        var limiteNormalizado = NormalizarLimite(entrada?.Limite ?? LimitePadrao);
        var notificacoes = await repositorioNotificacao.ListarRecentesAsync(
            entrada?.ResponsavelId,
            limiteNormalizado,
            cancellationToken);

        return notificacoes
            .Select(notificacao => new NotificacaoResposta
            {
                Id = notificacao.Id,
                ResponsavelId = notificacao.ResponsavelId,
                TarefaId = notificacao.TarefaId,
                ProjetoId = notificacao.ProjetoId,
                TituloTarefa = notificacao.TituloTarefa,
                Mensagem = notificacao.Mensagem,
                Reatribuicao = notificacao.Reatribuicao,
                DataCriacao = notificacao.DataCriacao
            })
            .ToList();
    }

    private static int NormalizarLimite(int limite)
    {
        if (limite <= 0)
        {
            return LimitePadrao;
        }

        return limite > LimiteMaximo ? LimiteMaximo : limite;
    }
}
