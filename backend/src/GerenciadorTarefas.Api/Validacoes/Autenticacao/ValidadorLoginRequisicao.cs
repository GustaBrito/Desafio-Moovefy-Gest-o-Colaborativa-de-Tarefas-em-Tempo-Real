using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Autenticacao;

namespace GerenciadorTarefas.Api.Validacoes.Autenticacao;

public sealed class ValidadorLoginRequisicao : AbstractValidator<LoginRequisicao>
{
    public ValidadorLoginRequisicao()
    {
        RuleFor(requisicao => requisicao.Email)
            .NotEmpty()
            .WithMessage("O email deve ser informado.")
            .EmailAddress()
            .WithMessage("O email informado e invalido.")
            .MaximumLength(200)
            .WithMessage("O email deve ter no maximo 200 caracteres.");

        RuleFor(requisicao => requisicao.Senha)
            .NotEmpty()
            .WithMessage("A senha deve ser informada.")
            .MaximumLength(200)
            .WithMessage("A senha deve ter no maximo 200 caracteres.");
    }
}
