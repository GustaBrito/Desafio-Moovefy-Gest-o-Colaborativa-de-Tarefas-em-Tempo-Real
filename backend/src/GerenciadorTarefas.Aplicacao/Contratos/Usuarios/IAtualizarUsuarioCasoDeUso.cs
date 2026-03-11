using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

namespace GerenciadorTarefas.Aplicacao.Contratos.Usuarios;

public interface IAtualizarUsuarioCasoDeUso
{
    Task<UsuarioResposta> ExecutarAsync(
        Guid id,
        AtualizarUsuarioEntrada entrada,
        CancellationToken cancellationToken = default);
}
