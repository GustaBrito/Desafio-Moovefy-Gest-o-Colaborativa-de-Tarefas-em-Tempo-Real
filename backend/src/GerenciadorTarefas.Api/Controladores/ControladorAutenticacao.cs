using GerenciadorTarefas.Api.Contratos.Requisicoes.Autenticacao;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Contratos.Respostas.Autenticacao;
using GerenciadorTarefas.Api.Configuracoes;
using GerenciadorTarefas.Api.Modelos.Autenticacao;
using GerenciadorTarefas.Api.Servicos.Autenticacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Globalization;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/autenticacao")]
public sealed class ControladorAutenticacao : ControllerBase
{
    private readonly IServicoAutenticacao servicoAutenticacao;
    private readonly IControleTentativasLogin controleTentativasLogin;

    public ControladorAutenticacao(
        IServicoAutenticacao servicoAutenticacao,
        IControleTentativasLogin controleTentativasLogin)
    {
        this.servicoAutenticacao = servicoAutenticacao;
        this.controleTentativasLogin = controleTentativasLogin;
    }

    [AllowAnonymous]
    [HttpGet("saude")]
    [ProducesResponseType(typeof(RespostaSucessoApi<object>), StatusCodes.Status200OK)]
    public ActionResult<RespostaSucessoApi<object>> ObterSaude()
    {
        var resposta = new RespostaSucessoApi<object>
        {
            Mensagem = "API disponivel.",
            Dados = new { status = "ok" },
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting(ConfiguracaoRateLimiting.NomePoliticaRateLimitLogin)]
    [ProducesResponseType(typeof(RespostaSucessoApi<LoginResposta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErroApi), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RespostaSucessoApi<LoginResposta>>> LoginAsync(
        [FromBody] LoginRequisicao requisicao,
        CancellationToken cancellationToken)
    {
        var emailNormalizado = requisicao.Email.Trim().ToLowerInvariant();
        if (controleTentativasLogin.EstaBloqueado(emailNormalizado, out var tempoRestante))
        {
            var segundosRestantes = Math.Max(1, (int)Math.Ceiling(tempoRestante.TotalSeconds));
            Response.Headers.RetryAfter = segundosRestantes.ToString(CultureInfo.InvariantCulture);

            return StatusCode(StatusCodes.Status429TooManyRequests, new RespostaErroApi
            {
                Status = StatusCodes.Status429TooManyRequests,
                Codigo = "login_bloqueado_temporariamente",
                Mensagem = "Muitas tentativas de autenticacao.",
                Detalhe = "Aguarde alguns instantes antes de tentar novamente.",
                CodigoRastreio = HttpContext.TraceIdentifier
            });
        }

        ResultadoAutenticacao resultado;
        try
        {
            resultado = await servicoAutenticacao.AutenticarAsync(
                emailNormalizado,
                requisicao.Senha,
                cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            controleTentativasLogin.RegistrarFalha(emailNormalizado);
            throw;
        }

        controleTentativasLogin.LimparFalhas(emailNormalizado);

        var resposta = new RespostaSucessoApi<LoginResposta>
        {
            Mensagem = "Autenticacao realizada com sucesso.",
            Dados = new LoginResposta
            {
                UsuarioId = resultado.UsuarioId,
                Nome = resultado.Nome,
                Email = resultado.Email,
                PerfilGlobal = resultado.PerfilGlobal,
                AreaIds = resultado.AreaIds,
                TokenAcesso = resultado.TokenAcesso,
                TipoToken = resultado.TipoToken,
                ExpiraEmUtc = resultado.ExpiraEmUtc
            },
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
