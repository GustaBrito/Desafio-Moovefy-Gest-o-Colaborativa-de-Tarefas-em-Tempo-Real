using GerenciadorTarefas.Aplicacao.Contratos.Areas;
using GerenciadorTarefas.Aplicacao.Modelos.Areas;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Areas;
using GerenciadorTarefas.Api.Contratos.Respostas;
using GerenciadorTarefas.Api.Seguranca;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/areas")]
public sealed class ControladorAreas : ControllerBase
{
    private readonly IConsultaAreasCasoDeUso consultaAreasCasoDeUso;
    private readonly ICriarAreaCasoDeUso criarAreaCasoDeUso;
    private readonly IAtualizarAreaCasoDeUso atualizarAreaCasoDeUso;

    public ControladorAreas(
        IConsultaAreasCasoDeUso consultaAreasCasoDeUso,
        ICriarAreaCasoDeUso criarAreaCasoDeUso,
        IAtualizarAreaCasoDeUso atualizarAreaCasoDeUso)
    {
        this.consultaAreasCasoDeUso = consultaAreasCasoDeUso;
        this.criarAreaCasoDeUso = criarAreaCasoDeUso;
        this.atualizarAreaCasoDeUso = atualizarAreaCasoDeUso;
    }

    [HttpGet]
    [ProducesResponseType(typeof(RespostaSucessoApi<IReadOnlyCollection<AreaResposta>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<IReadOnlyCollection<AreaResposta>>>> ListarAsync(
        [FromQuery] bool somenteAtivas = false,
        CancellationToken cancellationToken = default)
    {
        var areas = await consultaAreasCasoDeUso.ListarAsync(somenteAtivas, cancellationToken);
        return Ok(new RespostaSucessoApi<IReadOnlyCollection<AreaResposta>>
        {
            Mensagem = "Areas listadas com sucesso.",
            Dados = areas,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaSucessoApi<AreaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<AreaResposta>>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var area = await consultaAreasCasoDeUso.ObterPorIdAsync(id, cancellationToken);
        return Ok(new RespostaSucessoApi<AreaResposta>
        {
            Mensagem = "Area obtida com sucesso.",
            Dados = area,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpPost]
    [Authorize(Policy = PoliticasAutorizacao.AdministracaoAreas)]
    [ProducesResponseType(typeof(RespostaSucessoApi<AreaResposta>), StatusCodes.Status201Created)]
    public async Task<ActionResult<RespostaSucessoApi<AreaResposta>>> CriarAsync(
        [FromBody] CriarAreaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var areaCriada = await criarAreaCasoDeUso.ExecutarAsync(new CriarAreaEntrada
        {
            Nome = requisicao.Nome,
            Codigo = requisicao.Codigo,
            Ativa = requisicao.Ativa
        }, cancellationToken);

        return CreatedAtAction(nameof(ObterPorIdAsync), new { id = areaCriada.Id }, new RespostaSucessoApi<AreaResposta>
        {
            Mensagem = "Area criada com sucesso.",
            Dados = areaCriada,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PoliticasAutorizacao.AdministracaoAreas)]
    [ProducesResponseType(typeof(RespostaSucessoApi<AreaResposta>), StatusCodes.Status200OK)]
    public async Task<ActionResult<RespostaSucessoApi<AreaResposta>>> AtualizarAsync(
        Guid id,
        [FromBody] AtualizarAreaRequisicao requisicao,
        CancellationToken cancellationToken = default)
    {
        var areaAtualizada = await atualizarAreaCasoDeUso.ExecutarAsync(id, new AtualizarAreaEntrada
        {
            Nome = requisicao.Nome,
            Codigo = requisicao.Codigo,
            Ativa = requisicao.Ativa
        }, cancellationToken);

        return Ok(new RespostaSucessoApi<AreaResposta>
        {
            Mensagem = "Area atualizada com sucesso.",
            Dados = areaAtualizada,
            CodigoRastreio = HttpContext.TraceIdentifier
        });
    }
}
