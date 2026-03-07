using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;

public sealed class AtualizarStatusTarefaRequisicao
{
    public StatusTarefa Status { get; set; }
}
