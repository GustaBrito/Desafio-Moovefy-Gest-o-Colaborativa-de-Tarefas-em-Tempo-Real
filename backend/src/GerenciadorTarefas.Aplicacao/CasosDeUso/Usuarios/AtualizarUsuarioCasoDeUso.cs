using GerenciadorTarefas.Aplicacao.Contratos.Seguranca;
using GerenciadorTarefas.Aplicacao.Contratos.Usuarios;
using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;
using GerenciadorTarefas.Dominio.Contratos;
using GerenciadorTarefas.Dominio.Entidades;

namespace GerenciadorTarefas.Aplicacao.CasosDeUso.Usuarios;

public sealed class AtualizarUsuarioCasoDeUso : IAtualizarUsuarioCasoDeUso
{
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioArea repositorioArea;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly IServicoHashSenha servicoHashSenha;

    public AtualizarUsuarioCasoDeUso(
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
        Guid id,
        AtualizarUsuarioEntrada entrada,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do usuario deve ser informado.", nameof(id));
        }

        if (entrada is null)
        {
            throw new ArgumentNullException(nameof(entrada));
        }

        var usuario = await repositorioUsuario.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
        {
            throw new KeyNotFoundException($"Usuario com id '{id}' nao foi encontrado.");
        }

        var nomeNormalizado = entrada.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nomeNormalizado) || nomeNormalizado.Length > 150)
        {
            throw new ArgumentException("O nome do usuario deve ser informado e ter no maximo 150 caracteres.", nameof(entrada));
        }

        var emailNormalizado = entrada.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(emailNormalizado) || emailNormalizado.Length > 200 || !emailNormalizado.Contains('@'))
        {
            throw new ArgumentException("O email informado para o usuario e invalido.", nameof(entrada));
        }

        var usuarioComMesmoEmail = await repositorioUsuario.ObterPorEmailAsync(emailNormalizado, cancellationToken);
        if (usuarioComMesmoEmail is not null && usuarioComMesmoEmail.Id != usuario.Id)
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

        usuario.Nome = nomeNormalizado;
        usuario.Email = emailNormalizado;
        usuario.PerfilGlobal = entrada.PerfilGlobal;
        usuario.Ativo = entrada.Ativo;

        if (!string.IsNullOrWhiteSpace(entrada.NovaSenha))
        {
            if (entrada.NovaSenha.Trim().Length < 8)
            {
                throw new ArgumentException(
                    "Quando informada, a nova senha do usuario deve possuir no minimo 8 caracteres.",
                    nameof(entrada));
            }

            usuario.SenhaHash = servicoHashSenha.GerarHash(entrada.NovaSenha);
        }

        repositorioUsuario.Atualizar(usuario);
        await repositorioUsuario.SalvarAlteracoesAsync(cancellationToken);

        await repositorioUsuarioArea.RemoverPorUsuarioIdAsync(usuario.Id, cancellationToken);
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
