using FluentValidation;
using GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;

namespace GerenciadorTarefas.Api.Validacoes.Tarefas;

public sealed class ValidadorAtualizarStatusTarefaRequisicao : AbstractValidator<AtualizarStatusTarefaRequisicao>
{
    public ValidadorAtualizarStatusTarefaRequisicao()
    {
        RuleFor(requisicao => requisicao.Status)
            .IsInEnum()
            .WithMessage("O status informado e invalido.");
    }
}
