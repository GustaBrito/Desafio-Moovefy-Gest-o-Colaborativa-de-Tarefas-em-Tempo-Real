using GerenciadorTarefas.Api.Contratos.Respostas;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadorTarefas.Api.Configuracoes;

public static class ConfiguracaoPadraoRespostaApi
{
    public static IServiceCollection AdicionarPadraoRespostaApi(this IServiceCollection servicos)
    {
        servicos.Configure<ApiBehaviorOptions>(opcoes =>
        {
            opcoes.InvalidModelStateResponseFactory = contexto =>
            {
                var errosValidacao = contexto.ModelState
                    .Where(entrada => entrada.Value?.Errors.Count > 0)
                    .SelectMany(entrada =>
                        entrada.Value!.Errors.Select(erro =>
                            new ErroValidacaoResposta
                            {
                                Campo = entrada.Key,
                                Mensagem = string.IsNullOrWhiteSpace(erro.ErrorMessage)
                                    ? "Valor invalido."
                                    : erro.ErrorMessage
                            }))
                    .ToList();

                var respostaErro = new RespostaErroApi
                {
                    Status = StatusCodes.Status400BadRequest,
                    Codigo = "validacao_invalida",
                    Mensagem = "Existem erros de validacao na requisicao.",
                    CodigoRastreio = contexto.HttpContext.TraceIdentifier,
                    ErrosValidacao = errosValidacao
                };

                return new BadRequestObjectResult(respostaErro);
            };
        });

        return servicos;
    }
}
