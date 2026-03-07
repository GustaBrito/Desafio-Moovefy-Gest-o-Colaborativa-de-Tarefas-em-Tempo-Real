using Microsoft.AspNetCore.Mvc;

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

        var (status, titulo, detalhePadrao) = MapearExcecao(excecao);

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = ambiente.IsDevelopment() ? excecao.Message : detalhePadrao,
            Instance = contexto.Request.Path
        };

        problema.Extensions["codigoRastreio"] = contexto.TraceIdentifier;

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/problem+json";

        await contexto.Response.WriteAsJsonAsync(
            problema,
            cancellationToken: contexto.RequestAborted);
    }

    private static (int status, string titulo, string detalhePadrao) MapearExcecao(Exception excecao)
    {
        return excecao switch
        {
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Operacao invalida",
                "A operacao solicitada nao pode ser concluida."),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Parametro invalido",
                "Um ou mais parametros informados sao invalidos."),
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Recurso nao encontrado",
                "O recurso solicitado nao foi localizado."),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Acesso negado",
                "Voce nao possui permissao para executar esta operacao."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro interno no servidor.")
        };
    }
}
