using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class CriarProjetoCasoDeUso : ICriarProjetoCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuario repositorioUsuario;

    public CriarProjetoCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioArea repositorioArea,
        IRepositorioUsuario repositorioUsuario)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioArea = repositorioArea;
        this.repositorioUsuario = repositorioUsuario;
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

        var areaIds = entrada.AreaIds
            .Where(areaId => areaId != Guid.Empty)
            .Distinct()
            .ToList();
        if (entrada.AreaIdLegado.HasValue && entrada.AreaIdLegado.Value != Guid.Empty && !areaIds.Contains(entrada.AreaIdLegado.Value))
        {
            areaIds.Insert(0, entrada.AreaIdLegado.Value);
        }

        if (areaIds.Count == 0)
        {
            throw new ArgumentException("A area do projeto deve ser informada.", nameof(entrada));
        }

        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);
        var areasPorId = areas.ToDictionary(area => area.Id);
        var areaInvalida = areaIds.Any(areaId => !areasPorId.TryGetValue(areaId, out var area) || !area.Ativa);
        if (areaInvalida)
        {
            throw new InvalidOperationException("Uma ou mais areas informadas para o projeto sao invalidas ou inativas.");
        }

        var usuarioIdsVinculados = entrada.UsuarioIdsVinculados
            .Where(usuarioId => usuarioId != Guid.Empty)
            .Distinct()
            .ToList();
        if (entrada.GestorUsuarioId.HasValue && !usuarioIdsVinculados.Contains(entrada.GestorUsuarioId.Value))
        {
            usuarioIdsVinculados.Add(entrada.GestorUsuarioId.Value);
        }
        if (entrada.CriadoPorUsuarioId.HasValue && !usuarioIdsVinculados.Contains(entrada.CriadoPorUsuarioId.Value))
        {
            usuarioIdsVinculados.Add(entrada.CriadoPorUsuarioId.Value);
        }

        var usuariosVinculados = usuarioIdsVinculados.Count == 0
            ? []
            : await repositorioUsuario.ObterPorIdsAsync(usuarioIdsVinculados, cancellationToken);
        var usuariosPorId = usuariosVinculados.ToDictionary(usuario => usuario.Id);
        var usuarioInvalido = usuarioIdsVinculados.Any(usuarioId =>
            !usuariosPorId.TryGetValue(usuarioId, out var usuario) || !usuario.Ativo);
        if (usuarioInvalido)
        {
            throw new InvalidOperationException("Um ou mais usuarios vinculados sao invalidos ou inativos.");
        }

        var gestorUsuarioId = entrada.GestorUsuarioId
            ?? (usuarioIdsVinculados.Count > 0 ? usuarioIdsVinculados[0] : null);

        var projeto = new Projeto
        {
            Id = Guid.NewGuid(),
            Nome = nomeNormalizado,
            Descricao = descricaoNormalizada,
            AreaId = areaIds[0],
            CriadoPorUsuarioId = entrada.CriadoPorUsuarioId,
            GestorUsuarioId = gestorUsuarioId,
            DataCriacao = DateTime.UtcNow
        };

        await repositorioProjeto.AdicionarAsync(projeto, cancellationToken);
        await repositorioProjeto.SalvarAlteracoesAsync(cancellationToken);
        await repositorioProjeto.SincronizarAreasVinculadasAsync(projeto.Id, areaIds, cancellationToken);
        await repositorioProjeto.SincronizarUsuariosVinculadosAsync(projeto.Id, usuarioIdsVinculados, cancellationToken);
        await repositorioProjeto.SalvarAlteracoesAsync(cancellationToken);

        var areaPrincipal = areasPorId[projeto.AreaId];
        var gestorNome = gestorUsuarioId.HasValue && usuariosPorId.TryGetValue(gestorUsuarioId.Value, out var gestor)
            ? gestor.Nome
            : null;

        return new ProjetoResposta
        {
            Id = projeto.Id,
            Nome = projeto.Nome,
            Descricao = projeto.Descricao,
            AreaId = projeto.AreaId,
            AreaNome = areaPrincipal.Nome,
            AreaIds = areaIds,
            AreasNomes = areaIds.Select(areaId => areasPorId[areaId].Nome).ToList(),
            GestorUsuarioId = projeto.GestorUsuarioId,
            GestorNome = gestorNome,
            UsuarioIdsVinculados = usuarioIdsVinculados,
            UsuariosNomesVinculados = usuarioIdsVinculados
                .Where(usuarioId => usuariosPorId.ContainsKey(usuarioId))
                .Select(usuarioId => usuariosPorId[usuarioId].Nome)
                .ToList(),
            DataCriacao = projeto.DataCriacao
        };
    }
}
