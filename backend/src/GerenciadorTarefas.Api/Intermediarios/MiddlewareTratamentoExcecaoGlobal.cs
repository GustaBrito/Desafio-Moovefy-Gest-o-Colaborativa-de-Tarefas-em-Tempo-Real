using GerenciadorTarefas.Api.Contratos.Respostas;

namespace GerenciadorTarefas.Api.Intermediarios;

public sealed class MiddlewareTratamentoExcecaoGlobal
{
    private readonly RequestDelegate proximo;
    private readonly ILogger<MiddlewareTratamentoExcecaoGlobal> logger;
    private readonly IHostEnvironment ambiente;

    public MiddlewareTratamentoExcecaoGlobal(
        RequestDelegate proximo,
        ILogger<MiddlewareTratamentoExcecaoGlobal> logger,
        IHostEnvironment ambiente)
    {
        this.proximo = proximo;
        this.logger = logger;
        this.ambiente = ambiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await proximo(contexto);
        }
        catch (Exception excecao)
        {
            await TratarExcecaoAsync(contexto, excecao);
        }
    }

    private async Task TratarExcecaoAsync(HttpContext contexto, Exception excecao)
    {
        logger.LogError(excecao, "Erro nao tratado durante o processamento da requisicao.");

        var (status, codigo, mensagemPadrao, detalhePadrao) = MapearExcecao(excecao);

        var respostaErro = new RespostaErroApi
        {
            Status = status,
            Codigo = codigo,
            Mensagem = mensagemPadrao,
            Detalhe = ambiente.IsDevelopment() ? excecao.Message : detalhePadrao,
            CodigoRastreio = contexto.TraceIdentifier
        };

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json";

        await contexto.Response.WriteAsJsonAsync(
            respostaErro,
            cancellationToken: contexto.RequestAborted);
    }

    private static (int status, string codigo, string mensagemPadrao, string detalhePadrao) MapearExcecao(
        Exception excecao)
    {
        return excecao switch
        {
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "operacao_invalida",
                "A operacao solicitada nao pode ser concluida.",
                "A operacao solicitada nao pode ser concluida."),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "parametro_invalido",
                "Um ou mais parametros informados sao invalidos.",
                "Um ou mais parametros informados sao invalidos."),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "recurso_nao_encontrado",
                "O recurso solicitado nao foi localizado.",
                "O recurso solicitado nao foi localizado."),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "acesso_negado",
                "Voce nao possui permissao para executar esta operacao.",
                "Voce nao possui permissao para executar esta operacao."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "erro_interno",
                "Ocorreu um erro interno no servidor.",
                "Ocorreu um erro interno no servidor.")
        };
    }
}
