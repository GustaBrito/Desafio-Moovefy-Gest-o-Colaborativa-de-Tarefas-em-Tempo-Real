using GerenciadorTarefas.Aplicacao.Contratos.Projetos;
using GerenciadorTarefas.Aplicacao.Modelos.Projetos;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Projetos;

public sealed class ConsultaProjetosCasoDeUso : IConsultaProjetosCasoDeUso
{
    private readonly IRepositorioProjeto repositorioProjeto;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuario repositorioUsuario;

    public ConsultaProjetosCasoDeUso(
        IRepositorioProjeto repositorioProjeto,
        IRepositorioArea repositorioArea,
        IRepositorioUsuario repositorioUsuario)
    {
        this.repositorioProjeto = repositorioProjeto;
        this.repositorioArea = repositorioArea;
        this.repositorioUsuario = repositorioUsuario;
    }

    public async Task<IReadOnlyCollection<ProjetoResposta>> ListarAsync(
        IReadOnlyCollection<Guid>? areaIdsPermitidas = null,
        CancellationToken cancellationToken = default)
    {
        var projetos = await repositorioProjeto.ListarAsync(areaIdsPermitidas, cancellationToken);
        return await MapearParaRespostaAsync(projetos, cancellationToken);
    }

    public async Task<ProjetoResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
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

        var respostas = await MapearParaRespostaAsync([projeto], cancellationToken);
        return respostas.Single();
    }

    private async Task<IReadOnlyCollection<ProjetoResposta>> MapearParaRespostaAsync(
        IReadOnlyCollection<Projeto> projetos,
        CancellationToken cancellationToken)
    {
        var areaIds = projetos
            .SelectMany(projeto =>
                projeto.AreasVinculadas.Count > 0
                    ? projeto.AreasVinculadas.Select(vinculo => vinculo.AreaId)
                    : [projeto.AreaId])
            .Distinct()
            .ToArray();
        var usuarioIds = projetos
            .SelectMany(projeto =>
            {
                var ids = projeto.UsuariosVinculados.Select(vinculo => vinculo.UsuarioId).ToList();
                if (projeto.GestorUsuarioId.HasValue && !ids.Contains(projeto.GestorUsuarioId.Value))
                {
                    ids.Add(projeto.GestorUsuarioId.Value);
                }

                return ids;
            })
            .Distinct()
            .ToArray();

        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);
        var usuarios = await repositorioUsuario.ObterPorIdsAsync(usuarioIds, cancellationToken);

        var areasPorId = areas.ToDictionary(area => area.Id);
        var usuariosPorId = usuarios.ToDictionary(usuario => usuario.Id);

        return projetos.Select(projeto =>
        {
            var areaIdsProjeto = projeto.AreasVinculadas
                .Select(vinculo => vinculo.AreaId)
                .Distinct()
                .ToList();
            if (areaIdsProjeto.Count == 0)
            {
                areaIdsProjeto.Add(projeto.AreaId);
            }

            var areaPrincipalId = areaIdsProjeto.Contains(projeto.AreaId)
                ? projeto.AreaId
                : areaIdsProjeto[0];
            areasPorId.TryGetValue(areaPrincipalId, out var areaPrincipal);

            var usuarioIdsProjeto = projeto.UsuariosVinculados
                .Select(vinculo => vinculo.UsuarioId)
                .Distinct()
                .ToList();
            if (projeto.GestorUsuarioId.HasValue && !usuarioIdsProjeto.Contains(projeto.GestorUsuarioId.Value))
            {
                usuarioIdsProjeto.Add(projeto.GestorUsuarioId.Value);
            }

            var gestorNome = projeto.GestorUsuarioId.HasValue
                && usuariosPorId.TryGetValue(projeto.GestorUsuarioId.Value, out var gestor)
                    ? gestor.Nome
                    : null;

            return new ProjetoResposta
            {
                Id = projeto.Id,
                Nome = projeto.Nome,
                Descricao = projeto.Descricao,
                AreaId = areaPrincipalId,
                AreaNome = areaPrincipal?.Nome ?? "Area nao encontrada",
                AreaIds = areaIdsProjeto,
                AreasNomes = areaIdsProjeto
                    .Where(areaId => areasPorId.ContainsKey(areaId))
                    .Select(areaId => areasPorId[areaId].Nome)
                    .ToList(),
                GestorUsuarioId = projeto.GestorUsuarioId,
                GestorNome = gestorNome,
                UsuarioIdsVinculados = usuarioIdsProjeto,
                UsuariosNomesVinculados = usuarioIdsProjeto
                    .Where(usuarioId => usuariosPorId.ContainsKey(usuarioId))
                    .Select(usuarioId => usuariosPorId[usuarioId].Nome)
                    .ToList(),
                DataCriacao = projeto.DataCriacao
            };
        }).ToList();
    }
}
