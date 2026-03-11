using GerenciadorTarefas.Aplicacao.Modelos.Usuarios;

namespace GerenciadorTarefas.Aplicacao.Contratos.Usuarios;

public interface ICriarUsuarioCasoDeUso
{
    Task<UsuarioResposta> ExecutarAsync(
        CriarUsuarioEntrada entrada,
        CancellationToken cancellationToken = default);
}
