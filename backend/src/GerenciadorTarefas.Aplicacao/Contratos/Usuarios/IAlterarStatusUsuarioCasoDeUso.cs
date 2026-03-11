using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

namespace GerenciadorTarefas.Aplicacao.Contratos.Usuarios;

public interface IAlterarStatusUsuarioCasoDeUso
{
    Task<UsuarioResposta> ExecutarAsync(
        Guid id,
        AlterarStatusUsuarioEntrada entrada,
        CancellationToken cancellationToken = default);
}
