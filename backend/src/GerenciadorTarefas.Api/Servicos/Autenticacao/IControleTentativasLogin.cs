namespace GerenciadorTarefas.Api.Servicos.Autenticacao;

public interface IControleTentativasLogin
{
    bool EstaBloqueado(string emailNormalizado, out TimeSpan tempoRestante);
    void RegistrarFalha(string emailNormalizado);
    void LimparFalhas(string emailNormalizado);
}
