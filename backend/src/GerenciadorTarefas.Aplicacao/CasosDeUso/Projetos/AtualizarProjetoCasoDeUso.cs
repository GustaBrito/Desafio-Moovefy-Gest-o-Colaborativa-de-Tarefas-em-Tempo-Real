using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class AtualizarProjetoCasoDeUso : IAtualizarProjetoCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;

    public AtualizarProjetoCasoDeUso(IRepositorioProjeto repositorioProjeto)
    {
        this.repositorioProjeto = repositorioProjeto;
    }

    public async Task<ProjetoResposta> ExecutarAsync(
        Guid id,
        AtualizarProjetoEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do projeto deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var projeto = await repositorioProjeto.ObterPorIdAsync(id, cancellationToken);
        if (projeto is null)
        {
            throw new KeyNotFoundException($"Projeto com id '{id}' nao foi encontrado.");
        }

        var nomeNormalizado = entrada.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nomeNormalizado))
        {
            throw new ArgumentException("O nome do projeto deve ser informado.", nameof(entrada));
        }

        if (nomeNormalizado.Length > 150)
        {
            throw new ArgumentException("O nome do projeto deve ter no maximo 150 caracteres.", nameof(entrada));
        }

        var descricaoNormalizada = string.IsNullOrWhiteSpace(entrada.Descricao)
            ? null
            : entrada.Descricao.Trim();

        if (descricaoNormalizada is not null && descricaoNormalizada.Length > 1000)
        {
            throw new ArgumentException("A descricao do projeto deve ter no maximo 1000 caracteres.", nameof(entrada));
        }

        projeto.Nome = nomeNormalizado;
        projeto.Descricao = descricaoNormalizada;

        repositorioProjeto.Atualizar(projeto);
        await repositorioProjeto.SalvarAlteracoesAsync(cancellationToken);

        return new ProjetoResposta
        {
            Id = projeto.Id,
            Nome = projeto.Nome,
            Descricao = projeto.Descricao,
            DataCriacao = projeto.DataCriacao
        };
    }
}
