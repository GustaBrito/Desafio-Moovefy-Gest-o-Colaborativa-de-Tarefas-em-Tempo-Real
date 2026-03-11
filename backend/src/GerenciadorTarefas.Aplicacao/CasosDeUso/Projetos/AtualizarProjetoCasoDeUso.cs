using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class AtualizarProjetoCasoDeUso : IAtualizarProjetoCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuario repositorioUsuario;

    public AtualizarProjetoCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioArea repositorioArea,
        IRepositorioUsuario repositorioUsuario)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioArea = repositorioArea;
        this.repositorioUsuario = repositorioUsuario;
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

        if (entrada.AreaId == Guid.Empty)
        {
            throw new ArgumentException("A area do projeto deve ser informada.", nameof(entrada));
        }

        var area = await repositorioArea.ObterPorIdAsync(entrada.AreaId, cancellationToken);
        if (area is null || !area.Ativa)
        {
            throw new InvalidOperationException("A area informada para o projeto e invalida ou inativa.");
        }

        Usuario? gestor = null;
        if (entrada.GestorUsuarioId.HasValue)
        {
            gestor = await repositorioUsuario.ObterPorIdAsync(entrada.GestorUsuarioId.Value, cancellationToken);
            if (gestor is null || !gestor.Ativo)
            {
                throw new InvalidOperationException("O gestor informado e invalido ou inativo.");
            }
        }

        projeto.Nome = nomeNormalizado;
        projeto.Descricao = descricaoNormalizada;
        projeto.AreaId = entrada.AreaId;
        projeto.GestorUsuarioId = entrada.GestorUsuarioId;

        repositorioProjeto.Atualizar(projeto);
        await repositorioProjeto.SalvarAlteracoesAsync(cancellationToken);

        return new ProjetoResposta
        {
            Id = projeto.Id,
            Nome = projeto.Nome,
            Descricao = projeto.Descricao,
            AreaId = projeto.AreaId,
            AreaNome = area.Nome,
            GestorUsuarioId = projeto.GestorUsuarioId,
            GestorNome = gestor?.Nome,
            DataCriacao = projeto.DataCriacao
        };
    }
}
