using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GerenciadorTarefas.Aplicacao.Contratos.Seguranca;
using GerenciadorTarefas.Api.Modelos.Autenticacao;
using GerenciadorTarefas.Api.Seguranca;
using GerenciadorTarefas.Dominio.Contratos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorTarefas.Api.Servicos.Autenticacao;

public sealed class ServicoAutenticacaoJwt : IServicoAutenticacao
{
    private readonly ConfiguracaoJwt configuracaoJwt;
    private readonly IRepositorioUsuario repositorioUsuario;
    private readonly IRepositorioUsuarioArea repositorioUsuarioArea;
    private readonly IServicoHashSenha servicoHashSenha;

    public ServicoAutenticacaoJwt(
        IOptions<ConfiguracaoJwt> opcoesConfiguracaoJwt,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioUsuarioArea repositorioUsuarioArea,
        IServicoHashSenha servicoHashSenha)
    {
        configuracaoJwt = opcoesConfiguracaoJwt.Value;
        this.repositorioUsuario = repositorioUsuario;
        this.repositorioUsuarioArea = repositorioUsuarioArea;
        this.servicoHashSenha = servicoHashSenha;
    }

    public async Task<ResultadoAutenticacao> AutenticarAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        var emailNormalizado = email.Trim().ToLowerInvariant();
        var usuario = await repositorioUsuario.ObterPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        if (!usuario.Ativo)
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        if (!servicoHashSenha.Verificar(senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        var areaIds = await repositorioUsuarioArea.ListarAreaIdsPorUsuarioIdAsync(usuario.Id, cancellationToken);
        usuario.UltimoAcesso = DateTime.UtcNow;
        repositorioUsuario.Atualizar(usuario);
        await repositorioUsuario.SalvarAlteracoesAsync(cancellationToken);

        var dataExpiracao = DateTime.UtcNow.AddMinutes(configuracaoJwt.ExpiracaoMinutos);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Nome),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.PerfilGlobal.ToString()),
            new(TiposClaimsAutenticacao.PerfilGlobal, usuario.PerfilGlobal.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(areaIds.Select(areaId =>
            new Claim(TiposClaimsAutenticacao.AreaId, areaId.ToString("D"))));

        var credenciaisAssinatura = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracaoJwt.ChaveSecreta)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuracaoJwt.Emissor,
            audience: configuracaoJwt.Publico,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: dataExpiracao,
            signingCredentials: credenciaisAssinatura);

        var tokenAcesso = new JwtSecurityTokenHandler().WriteToken(token);

        return new ResultadoAutenticacao
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            PerfilGlobal = usuario.PerfilGlobal,
            AreaIds = areaIds,
            TokenAcesso = tokenAcesso,
            TipoToken = "Bearer",
            ExpiraEmUtc = dataExpiracao
        };
    }
}
