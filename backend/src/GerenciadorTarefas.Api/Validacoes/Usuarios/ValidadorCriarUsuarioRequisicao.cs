using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Usuarios;

namespace GerenciadorTarefas.Api.Validacoes.Usuarios;

public sealed class ValidadorCriarUsuarioRequisicao : AbstractValidator<CriarUsuarioRequisicao>
{
    public ValidadorCriarUsuarioRequisicao()
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

        RuleFor(requisicao => requisicao.Senha)
            .NotEmpty()
            .WithMessage("A senha deve ser informada.")
            .MinimumLength(10)
            .WithMessage("A senha deve possuir no minimo 10 caracteres.")
            .MaximumLength(200)
            .WithMessage("A senha deve possuir no maximo 200 caracteres.")
            .Must(PoliticaSenha.AtendeRequisitos)
            .WithMessage(PoliticaSenha.MensagemRequisitos);

        RuleFor(requisicao => requisicao.PerfilGlobal)
            .IsInEnum()
            .WithMessage("O perfil global informado e invalido.");

        RuleFor(requisicao => requisicao.AreaIds)
            .NotEmpty()
            .WithMessage("Ao menos uma area deve ser vinculada ao usuario.");
    }
}
