namespace GerenciadorTarefas.Aplicacao.Modelos.Notificacoes;

public sealed class ConsultaHistoricoNotificacoesEntrada
{
    public Guid? ResponsavelId { get; init; }
    public int Limite { get; init; } = 50;
}
