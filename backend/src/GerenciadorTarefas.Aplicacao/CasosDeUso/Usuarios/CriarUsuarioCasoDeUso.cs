using GerenciadorTarefas.Aplicacao.Contratos.Seguranca;
using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;

public sealed class CriarUsuarioCasoDeUso : ICriarUsuarioCasoDeUso
{
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly IServicoHashSenha servicoHashSenha;

    public CriarUsuarioCasoDeUso(
        IRepositorioUsuario repositorioUsuario,
        IRepositorioArea repositorioArea,
        IRepositorioUsuarioArea repositorioUsuarioArea,
        IServicoHashSenha servicoHashSenha)
    {
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioArea = repositorioArea;
        this.repositorioUsuarioArea = repositorioUsuarioArea;
        this.servicoHashSenha = servicoHashSenha;
    }

    public async Task<UsuarioResposta> ExecutarAsync(
        CriarUsuarioEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var nomeNormalizado = entrada.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nomeNormalizado) || nomeNormalizado.Length > 150)
        {
            throw new ArgumentException("O nome do usuario deve ser informado e ter no maximo 150 caracteres.", nameof(entrada));
        }

        var emailNormalizado = entrada.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(emailNormalizado) || emailNormalizado.Length > 200)
        {
            throw new ArgumentException("O email do usuario deve ser informado e ter no maximo 200 caracteres.", nameof(entrada));
        }

        if (!emailNormalizado.Contains('@'))
        {
            throw new ArgumentException("O email informado para o usuario e invalido.", nameof(entrada));
        }

        if (string.IsNullOrWhiteSpace(entrada.Senha) || entrada.Senha.Trim().Length < 8)
        {
            throw new ArgumentException("A senha do usuario deve possuir no minimo 8 caracteres.", nameof(entrada));
        }

        var usuarioExistente = await repositorioUsuario.ObterPorEmailAsync(emailNormalizado, cancellationToken);
        if (usuarioExistente is not null)
        {
            throw new InvalidOperationException("Ja existe usuario cadastrado com o email informado.");
        }

        var areaIds = entrada.AreaIds
            .Where(areaId => areaId != Guid.Empty)
            .Distinct()
            .ToArray();

        if (areaIds.Length == 0)
        {
            throw new ArgumentException("Ao menos uma area deve ser vinculada ao usuario.", nameof(entrada));
        }

        var areas = await repositorioArea.ListarPorIdsAsync(areaIds, cancellationToken);
        if (areas.Count != areaIds.Length)
        {
            throw new InvalidOperationException("Uma ou mais areas informadas nao existem.");
        }

        if (areas.Any(area => !area.Ativa))
        {
            throw new InvalidOperationException("Nao e permitido vincular usuario em area inativa.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nomeNormalizado,
            Email = emailNormalizado,
            SenhaHash = servicoHashSenha.GerarHash(entrada.Senha),
            PerfilGlobal = entrada.PerfilGlobal,
            Ativo = entrada.Ativo,
            DataCriacao = DateTime.UtcNow
        };

        await repositorioUsuario.AdicionarAsync(usuario, cancellationToken);
        await repositorioUsuario.SalvarAlteracoesAsync(cancellationToken);

        await repositorioUsuarioArea.AdicionarEmLoteAsync(
            areaIds.Select(areaId => new UsuarioArea
            {
                UsuarioId = usuario.Id,
                AreaId = areaId
            }).ToList(),
            cancellationToken);
        await repositorioUsuarioArea.SalvarAlteracoesAsync(cancellationToken);

        var areasPorId = areas.ToLookup(area => area.Id);
        return await MapeadorUsuarioResposta.MapearAsync(
            usuario,
            areaIds,
            areasPorId,
            repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync,
            cancellationToken);
    }
}
