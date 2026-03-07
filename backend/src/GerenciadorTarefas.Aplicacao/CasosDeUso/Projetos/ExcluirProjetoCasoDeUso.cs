using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class ExcluirProjetoCasoDeUso : IExcluirProjetoCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioTarefa repositorioTarefa;

    public ExcluirProjetoCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioTarefa repositorioTarefa)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioTarefa = repositorioTarefa;
    }

    public async Task ExecutarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do projeto deve ser informado.", nameof(id));
        }

        var projeto = await repositorioProjeto.ObterPorIdAsync(id, cancellationToken);
        if (projeto is null)
        {
            throw new KeyNotFoundException($"Projeto com id '{id}' nao foi encontrado.");
        }

        var possuiTarefasVinculadas = await repositorioTarefa.ExistePorProjetoIdAsync(id, cancellationToken);
        if (possuiTarefasVinculadas)
        {
            throw new InvalidOperationException(
                "Nao e permitido excluir projeto com tarefas vinculadas.");
        }

        repositorioProjeto.Remover(projeto);
        await repositorioProjeto.SalvarAlteracoesAsync(cancellationToken);
    }
}
