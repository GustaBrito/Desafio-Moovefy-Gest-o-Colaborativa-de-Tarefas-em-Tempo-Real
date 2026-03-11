using GerenciadorTarefas.Aplicacao.Contratos.Notificacoes;
using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/notificacoes")]
public sealed class ControladorNotificacoes : ControllerBase
{
    private readonly IConsultaHistoricoNotificacoesCasoDeUso consultaHistoricoNotificacoesCasoDeUso;
    private readonly IConsultaUsuariosCasoDeUso consultaUsuariosCasoDeUso;

    public ControladorNotificacoes(
        IConsultaHistoricoNotificacoesCasoDeUso consultaHistoricoNotificacoesCasoDeUso,
        IConsultaUsuariosCasoDeUso consultaUsuariosCasoDeUso)
    {
        this.consultaHistoricoNotificacoesCasoDeUso = consultaHistoricoNotificacoesCasoDeUso;
        this.consultaUsuariosCasoDeUso = consultaUsuariosCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<NotificacaoResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<NotificacaoResposta>>>> ListarHistoricoAsync(
        [FromQuery] Guid? responsavelUsuarioId,
        [FromQuery] int limite = 50,
        CancellationToken cancellationToken = default)
    {
        var contexto = User.ObterContextoUsuarioAutenticado();
        var responsavelConsulta = responsavelUsuarioId;

        if (!contexto.EhAdministrativo)
        {
            if (responsavelUsuarioId.HasValue && responsavelUsuarioId.Value != contexto.UsuarioId)
            {
                return Forbid();
            }

            responsavelConsulta = contexto.UsuarioId;
        }
        else if (contexto.EhAdmin
            && responsavelUsuarioId.HasValue
            && responsavelUsuarioId.Value != contexto.UsuarioId)
        {
            var usuarioConsulta = await consultaUsuariosCasoDeUso.ObterPorIdAsync(
                responsavelUsuarioId.Value,
                cancellationToken);

            if (!usuarioConsulta.AreaIds.Any(contexto.AreaIds.Contains))
            {
                return Forbid();
            }
        }

        var entrada = new ConsultaHistoricoNotificacoesEntrada
        {
            ResponsavelUsuarioId = responsavelConsulta,
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
