using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Areas;

namespace GerenciadorTarefas.Api.Validacoes.Areas;

public sealed class ValidadorAtualizarAreaRequisicao : AbstractValidator<AtualizarAreaRequisicao>
{
    public ValidadorAtualizarAreaRequisicao()
    {
        RuleFor(requisicao => requisicao.Nome)
            .NotEmpty()
            .WithMessage("O nome da area deve ser informado.")
            .MaximumLength(120)
            .WithMessage("O nome da area deve ter no maximo 120 caracteres.");

        RuleFor(requisicao => requisicao.Codigo)
            .MaximumLength(60)
            .WithMessage("O codigo da area deve ter no maximo 60 caracteres.");
    }
}
