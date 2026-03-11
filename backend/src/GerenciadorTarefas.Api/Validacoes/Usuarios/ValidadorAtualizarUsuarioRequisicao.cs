using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Usuarios;

namespace GerenciadorTarefas.Api.Validacoes.Usuarios;

public sealed class ValidadorAtualizarUsuarioRequisicao : AbstractValidator<AtualizarUsuarioRequisicao>
{
    public ValidadorAtualizarUsuarioRequisicao()
    {
        RuleFor(requisicao => requisicao.Nome)
            .NotEmpty()
            .WithMessage("O nome do usuario deve ser informado.")
            .MaximumLength(150)
            .WithMessage("O nome do usuario deve ter no maximo 150 caracteres.");

        RuleFor(requisicao => requisicao.Email)
            .NotEmpty()
            .WithMessage("O email do usuario deve ser informado.")
            .EmailAddress()
            .WithMessage("O email do usuario e invalido.")
            .MaximumLength(200)
            .WithMessage("O email do usuario deve ter no maximo 200 caracteres.");

        RuleFor(requisicao => requisicao.PerfilGlobal)
            .IsInEnum()
            .WithMessage("O perfil global informado e invalido.");

        RuleFor(requisicao => requisicao.NovaSenha)
            .MinimumLength(8)
            .WithMessage("Quando informada, a nova senha deve possuir no minimo 8 caracteres.")
            .When(requisicao => !string.IsNullOrWhiteSpace(requisicao.NovaSenha));

        RuleFor(requisicao => requisicao.AreaIds)
            .NotEmpty()
            .WithMessage("Ao menos uma area deve ser vinculada ao usuario.");
    }
}
