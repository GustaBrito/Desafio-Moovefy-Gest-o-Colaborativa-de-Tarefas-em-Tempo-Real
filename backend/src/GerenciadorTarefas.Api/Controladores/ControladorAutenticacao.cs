using GerenciadorTarefas.Api.Contratos.Requisicoes.Autenticacao;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Contratos.Respostas.Autenticacao;
using GerenciadorTarefas.Api.Servicos.Autenticacao;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost("login")]
    [ProducesResponseType(typeof(RespostaSucessoApi<LoginResposta>), StatusCodes.Status200OK)]
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
                Perfil = resultado.Perfil,
                TokenAcesso = resultado.TokenAcesso,
                TipoToken = resultado.TipoToken,
                ExpiraEmUtc = resultado.ExpiraEmUtc
            },
            CodigoRastreio = HttpContext.TraceIdentifier
        };

        return Ok(resposta);
    }
}
