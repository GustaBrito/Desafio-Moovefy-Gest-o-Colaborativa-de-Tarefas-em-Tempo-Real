using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;
using GerenciadorTarefas.Api.Contratos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/notificacoes")]
public sealed class ControladorNotificacoes : ControllerBase
{
    private readonly IConsultaHistoricoNotificacoesCasoDeUso consultaHistoricoNotificacoesCasoDeUso;

    public ControladorNotificacoes(IConsultaHistoricoNotificacoesCasoDeUso consultaHistoricoNotificacoesCasoDeUso)
    {
        this.consultaHistoricoNotificacoesCasoDeUso = consultaHistoricoNotificacoesCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<NotificacaoResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<NotificacaoResposta>>>> ListarHistoricoAsync(
        [FromQuery] Guid? responsavelId,
        [FromQuery] int limite = 50,
        CancellationToken cancellationToken = default)
    {
        var entrada = new ConsultaHistoricoNotificacoesEntrada
        {
            ResponsavelId = responsavelId,
            Limite = limite
        };

        var notificacoes = await consultaHistoricoNotificacoesCasoDeUso.ListarAsync(entrada, cancellationToken);
        var resposta = new RespostaSucessoApi<IReadOnlyCollection<NotificacaoResposta>>
        {
            Mensagem = "Historico de notificacoes obtido com sucesso.",
            Dados = notificacoes,
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
