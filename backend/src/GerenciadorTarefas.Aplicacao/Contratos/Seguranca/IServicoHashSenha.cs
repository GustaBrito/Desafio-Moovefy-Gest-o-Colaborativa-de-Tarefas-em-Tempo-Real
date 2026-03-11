namespace GerenciadorTarefas.Aplicacao.Contratos.Seguranca;

public interface IServicoHashSenha
{
    string GerarHash(string senha);
    bool Verificar(string senha, string senhaHash);
}
