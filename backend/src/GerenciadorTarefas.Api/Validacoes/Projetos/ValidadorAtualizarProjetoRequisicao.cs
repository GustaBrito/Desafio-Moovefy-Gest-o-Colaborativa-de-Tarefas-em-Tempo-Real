using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;

namespace GerenciadorTarefas.Api.Validacoes.Projetos;

public sealed class ValidadorAtualizarProjetoRequisicao : AbstractValidator<AtualizarProjetoRequisicao>
{
    public ValidadorAtualizarProjetoRequisicao()
    {
        RuleFor(requisicao => requisicao.Nome)
            .NotEmpty()
            .WithMessage("O nome do projeto deve ser informado.")
            .MaximumLength(150)
            .WithMessage("O nome do projeto deve ter no maximo 150 caracteres.");

        RuleFor(requisicao => requisicao.Descricao)
            .MaximumLength(1000)
            .WithMessage("A descricao do projeto deve ter no maximo 1000 caracteres.");
    }
}
