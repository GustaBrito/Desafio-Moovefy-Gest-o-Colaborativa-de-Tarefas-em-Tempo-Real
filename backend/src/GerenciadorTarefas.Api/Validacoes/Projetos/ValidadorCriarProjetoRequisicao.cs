using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Projetos;

namespace GerenciadorTarefas.Api.Validacoes.Projetos;

public sealed class ValidadorCriarProjetoRequisicao : AbstractValidator<CriarProjetoRequisicao>
{
    public ValidadorCriarProjetoRequisicao()
    {
        RuleFor(requisicao => requisicao.Nome)
            .NotEmpty()
            .WithMessage("O nome do projeto deve ser informado.")
            .MaximumLength(150)
            .WithMessage("O nome do projeto deve ter no maximo 150 caracteres.");

        RuleFor(requisicao => requisicao.Descricao)
            .MaximumLength(1000)
            .WithMessage("A descricao do projeto deve ter no maximo 1000 caracteres.");

        RuleFor(requisicao => requisicao.AreaId)
            .Must((requisicao, areaId) =>
                areaId != Guid.Empty || (requisicao.AreaIds is not null && requisicao.AreaIds.Any()))
            .WithMessage("Informe ao menos uma area para o projeto.");

        RuleFor(requisicao => requisicao.AreaIds)
            .Must(areaIds => areaIds is null || areaIds.All(areaId => areaId != Guid.Empty))
            .WithMessage("Todas as areas vinculadas devem ser validas.");

        RuleFor(requisicao => requisicao.UsuarioIdsVinculados)
            .Must(usuarioIds => usuarioIds is null || usuarioIds.All(usuarioId => usuarioId != Guid.Empty))
            .WithMessage("Todos os usuarios vinculados devem ser validos.");
    }
}
