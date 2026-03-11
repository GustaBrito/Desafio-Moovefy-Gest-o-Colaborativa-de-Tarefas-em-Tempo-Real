using GerenciadorTarefas.Aplicacao.Contratos.Areas;
using GerenciadorTarefas.Aplicacao.Modelos.Areas;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Areas;

public sealed class CriarAreaCasoDeUso : ICriarAreaCasoDeUso
{
    private readonly IRepositorioArea repositorioArea;

    public CriarAreaCasoDeUso(IRepositorioArea repositorioArea)
    {
        this.repositorioArea = repositorioArea;
    }

    public async Task<AreaResposta> ExecutarAsync(
        CriarAreaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var nomeNormalizado = entrada.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nomeNormalizado))
        {
            throw new ArgumentException("O nome da area deve ser informado.", nameof(entrada));
        }

        if (nomeNormalizado.Length > 120)
        {
            throw new ArgumentException("O nome da area deve ter no maximo 120 caracteres.", nameof(entrada));
        }

        var codigoNormalizado = string.IsNullOrWhiteSpace(entrada.Codigo)
            ? null
            : entrada.Codigo.Trim();

        if (codigoNormalizado is not null && codigoNormalizado.Length > 60)
        {
            throw new ArgumentException("O codigo da area deve ter no maximo 60 caracteres.", nameof(entrada));
        }

        var areaComMesmoNome = await repositorioArea.ObterPorNomeAsync(nomeNormalizado, cancellationToken);
        if (areaComMesmoNome is not null)
        {
            throw new InvalidOperationException("Ja existe uma area com o nome informado.");
        }

        if (codigoNormalizado is not null)
        {
            var areaComMesmoCodigo = await repositorioArea.ObterPorCodigoAsync(codigoNormalizado, cancellationToken);
            if (areaComMesmoCodigo is not null)
            {
                throw new InvalidOperationException("Ja existe uma area com o codigo informado.");
            }
        }

        var area = new Area
        {
            Id = Guid.NewGuid(),
            Nome = nomeNormalizado,
            Codigo = codigoNormalizado,
            Ativa = entrada.Ativa
        };

        await repositorioArea.AdicionarAsync(area, cancellationToken);
        await repositorioArea.SalvarAlteracoesAsync(cancellationToken);

        return new AreaResposta
        {
            Id = area.Id,
            Nome = area.Nome,
            Codigo = area.Codigo,
            Ativa = area.Ativa
        };
    }
}
