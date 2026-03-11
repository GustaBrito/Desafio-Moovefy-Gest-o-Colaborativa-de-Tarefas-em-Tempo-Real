using GerenciadorTarefas.Aplicacao.Contratos.Areas;
using GerenciadorTarefas.Aplicacao.Modelos.Areas;
using GerenciadorTarefas.Dominio.Contratos;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Areas;

public sealed class AtualizarAreaCasoDeUso : IAtualizarAreaCasoDeUso
{
    private readonly IRepositorioArea repositorioArea;

    public AtualizarAreaCasoDeUso(IRepositorioArea repositorioArea)
    {
        this.repositorioArea = repositorioArea;
    }

    public async Task<AreaResposta> ExecutarAsync(
        Guid id,
        AtualizarAreaEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador da area deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var area = await repositorioArea.ObterPorIdAsync(id, cancellationToken);
        if (area is null)
        {
            throw new KeyNotFoundException($"Area com id '{id}' nao foi encontrada.");
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
        if (areaComMesmoNome is not null && areaComMesmoNome.Id != area.Id)
        {
            throw new InvalidOperationException("Ja existe uma area com o nome informado.");
        }

        if (codigoNormalizado is not null)
        {
            var areaComMesmoCodigo = await repositorioArea.ObterPorCodigoAsync(codigoNormalizado, cancellationToken);
            if (areaComMesmoCodigo is not null && areaComMesmoCodigo.Id != area.Id)
            {
                throw new InvalidOperationException("Ja existe uma area com o codigo informado.");
            }
        }

        area.Nome = nomeNormalizado;
        area.Codigo = codigoNormalizado;
        area.Ativa = entrada.Ativa;

        repositorioArea.Atualizar(area);
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
