using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Api.Contratos.Requisicoes.Tarefas;

public sealed class CriarTarefaRequisicao
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public PrioridadeTarefa Prioridade { get; set; }
    public Guid ProjetoId { get; set; }
    public Guid ResponsavelUsuarioId { get; set; }
    public DateTime DataPrazo { get; set; }
}
