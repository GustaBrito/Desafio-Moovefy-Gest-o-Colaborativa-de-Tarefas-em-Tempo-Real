using GerenciadorTarefas.Aplicacao.Contratos.Areas;
using GerenciadorTarefas.Aplicacao.Modelos.Areas;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Areas;

public sealed class ConsultaAreasCasoDeUso : IConsultaAreasCasoDeUso
{
    private readonly IRepositorioArea repositorioArea;

    public ConsultaAreasCasoDeUso(IRepositorioArea repositorioArea)
    {
        this.repositorioArea = repositorioArea;
    }

    public async Task<IReadOnlyCollection<AreaResposta>> ListarAsync(
        bool somenteAtivas = false,
        CancellationToken cancellationToken = default)
    {
        var areas = await repositorioArea.ListarAsync(somenteAtivas, cancellationToken);
        return areas.Select(MapearResposta).ToList();
    }

    public async Task<AreaResposta> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da area deve ser informado.", nameof(id));
        }

        var area = await repositorioArea.ObterPorIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new KeyNotFoundException($"Area com id '{id}' nao foi encontrada.");
        }

        return MapearResposta(area);
    }

    private static AreaResposta MapearResposta(Dominio.Entidades.Area area)
    {
        return new AreaResposta
        {
            Id = area.Id,
            Nome = area.Nome,
            Codigo = area.Codigo,
            Ativa = area.Ativa
        };
    }
}
