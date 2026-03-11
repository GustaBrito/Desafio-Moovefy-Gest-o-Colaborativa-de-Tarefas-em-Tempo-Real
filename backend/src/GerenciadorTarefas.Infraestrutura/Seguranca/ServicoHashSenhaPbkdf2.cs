using System.Security.Cryptography;
using GerenciadorTarefas.Aplicacao.Contratos.Seguranca;

namespace GerenciadorTarefas.Infraestrutura.Seguranca;

public sealed class ServicoHashSenhaPbkdf2 : IServicoHashSenha
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public string GerarHash(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new ArgumentException("A senha deve ser informada.", nameof(senha));
        }

        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha,
            salt,
            Iteracoes,
            HashAlgorithmName.SHA256,
            TamanhoHash);

        return $"PBKDF2${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string senha, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(senhaHash))
        {
            return false;
        }

        var partes = senhaHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length != 4 || !string.Equals(partes[0], "PBKDF2", StringComparison.Ordinal))
        {
            return false;
        }

        if (!int.TryParse(partes[1], out var iteracoes) || iteracoes <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] hashArmazenado;
        try
        {
            salt = Convert.FromBase64String(partes[2]);
            hashArmazenado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var hashInformado = Rfc2898DeriveBytes.Pbkdf2(
            senha,
            salt,
            iteracoes,
            HashAlgorithmName.SHA256,
            hashArmazenado.Length);

        return CryptographicOperations.FixedTimeEquals(hashArmazenado, hashInformado);
    }
}
