using GerenciadorTarefas.Api.Contratos.Requisicoes.Autenticacao;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Contratos.Respostas.Autenticacao;
using GerenciadorTarefas.Api.Configuracoes;
using GerenciadorTarefas.Api.Servicos.Autenticacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Route("api/autenticacao")]
public sealed class ControladorAutenticacao : ControllerBase
{
    private readonly IServicoAutenticacao servicoAutenticacao;

    public ControladorAutenticacao(IServicoAutenticacao servicoAutenticacao)
    {
        this.servicoAutenticacao = servicoAutenticacao;
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
        var resultado = await servicoAutenticacao.AutenticarAsync(
            requisicao.Email,
            requisicao.Senha,
            cancellationToken);

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
