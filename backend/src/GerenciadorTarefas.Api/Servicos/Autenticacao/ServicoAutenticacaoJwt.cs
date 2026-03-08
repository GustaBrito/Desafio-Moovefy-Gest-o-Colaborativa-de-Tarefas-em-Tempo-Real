using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GerenciadorTarefas.Api.Modelos.Autenticacao;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GerenciadorTarefas.Api.Servicos.Autenticacao;

public sealed class ServicoAutenticacaoJwt : IServicoAutenticacao
{
    private readonly ConfiguracaoJwt configuracaoJwt;

    public ServicoAutenticacaoJwt(IOptions<ConfiguracaoJwt> opcoesConfiguracaoJwt)
    {
        configuracaoJwt = opcoesConfiguracaoJwt.Value;
    }

    public Task<ResultadoAutenticacao> AutenticarAsync(
        string email,
        string senha,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        var usuario = configuracaoJwt.Usuarios.FirstOrDefault(usuarioConfigurado =>
            string.Equals(usuarioConfigurado.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(usuarioConfigurado.Senha, senha, StringComparison.Ordinal));

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciais de acesso invalidas.");
        }

        var dataExpiracao = DateTime.UtcNow.AddMinutes(configuracaoJwt.ExpiracaoMinutos);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, usuario.Nome),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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

        return Task.FromResult(new ResultadoAutenticacao
        {
            UsuarioId = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
            TokenAcesso = tokenAcesso,
            TipoToken = "Bearer",
            ExpiraEmUtc = dataExpiracao
        });
    }
}
