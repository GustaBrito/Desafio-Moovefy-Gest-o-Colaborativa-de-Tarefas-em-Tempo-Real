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
        var areaIds = projetos.Select(projeto => projeto.AreaId).Distinct().ToArray();
        var gestorIds = projetos
            .Where(projeto => projeto.GestorUsuarioId.HasValue)
            .Select(projeto => projeto.GestorUsuarioId!.Value)
            .Distinct()
            .ToArray();

        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);
        var gestores = await repositorioUsuario.ObterPorIdsAsync(gestorIds, cancellationToken);

        var areasPorId = areas.ToDictionary(area => area.Id);
        var gestoresPorId = gestores.ToDictionary(gestor => gestor.Id);

        return projetos.Select(projeto =>
        {
            areasPorId.TryGetValue(projeto.AreaId, out var area);
            var gestorNome = projeto.GestorUsuarioId.HasValue
                && gestoresPorId.TryGetValue(projeto.GestorUsuarioId.Value, out var gestor)
                    ? gestor.Nome
                    : null;

            return new ProjetoResposta
            {
                Id = projeto.Id,
                Nome = projeto.Nome,
                Descricao = projeto.Descricao,
                AreaId = projeto.AreaId,
                AreaNome = area?.Nome ?? "Area nao encontrada",
                GestorUsuarioId = projeto.GestorUsuarioId,
                GestorNome = gestorNome,
                DataCriacao = projeto.DataCriacao
            };
        }).ToList();
    }
}
