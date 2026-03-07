using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class CriarProjetoCasoDeUso : ICriarProjetoCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;

    public CriarProjetoCasoDeUso(IRepositorioProjeto repositorioProjeto)
    {
        this.repositorioProjeto = repositorioProjeto;
    }

    public async Task<ProjetoResposta> ExecutarAsync(
        CriarProjetoEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
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

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = nomeNormalizado,
            Descricao = descricaoNormalizada,
            DataCriacao = DateTime.UtcNow
        };

        await repositorioProjeto.AdicionarAsync(projeto, cancellationToken);
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
