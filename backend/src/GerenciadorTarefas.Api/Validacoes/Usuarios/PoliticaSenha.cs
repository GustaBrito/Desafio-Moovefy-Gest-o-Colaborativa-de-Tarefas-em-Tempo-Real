using System.Text.RegularExpressions;

namespace GerenciadorTarefas.Api.Validacoes.Usuarios;

public static partial class PoliticaSenha
{
    public const string MensagemRequisitos =
        "A senha deve ter entre 10 e 200 caracteres e conter letra maiuscula, letra minuscula, numero e caractere especial.";

    public static bool AtendeRequisitos(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
        {
            return false;
        }

        return RegexSenhaForte().IsMatch(senha);
    }

    [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{10,200}$")]
    private static partial Regex RegexSenhaForte();
}
