using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;

namespace GerenciadorTarefas.Api.Validacoes.Tarefas;

public sealed class ValidadorCriarTarefaRequisicao : AbstractValidator<CriarTarefaRequisicao>
{
    public ValidadorCriarTarefaRequisicao()
    {
        RuleFor(requisicao => requisicao.Titulo)
            .NotEmpty()
            .WithMessage("O titulo da tarefa deve ser informado.")
            .MaximumLength(200)
            .WithMessage("O titulo da tarefa deve ter no maximo 200 caracteres.");

        RuleFor(requisicao => requisicao.Descricao)
            .MaximumLength(2000)
            .WithMessage("A descricao da tarefa deve ter no maximo 2000 caracteres.");

        RuleFor(requisicao => requisicao.Prioridade)
            .IsInEnum()
            .WithMessage("A prioridade informada e invalida.");

        RuleFor(requisicao => requisicao.ProjetoId)
            .NotEmpty()
            .WithMessage("O identificador do projeto deve ser informado.");

        RuleFor(requisicao => requisicao.ResponsavelUsuarioId)
            .NotEmpty()
            .WithMessage("O identificador do responsavel deve ser informado.");

        RuleFor(requisicao => requisicao.DataPrazo)
            .GreaterThan(DateTime.MinValue)
            .WithMessage("A data de prazo da tarefa deve ser informada.");
    }
}
