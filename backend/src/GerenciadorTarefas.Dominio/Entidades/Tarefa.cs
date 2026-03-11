using GerenciadorTarefas.Dominio.Enumeracoes;

namespace GerenciadorTarefas.Dominio.Entidades;

public class Tarefa
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public StatusTarefa Status { get; set; } = StatusTarefa.Pendente;
    public PrioridadeTarefa Prioridade { get; set; } = PrioridadeTarefa.Media;
    public Guid ProjetoId { get; set; }
    public Guid ResponsavelUsuarioId { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataPrazo { get; set; }
    public DateTime? DataConclusao { get; set; }

    public Projeto? Projeto { get; set; }
    public Usuario? ResponsavelUsuario { get; set; }

    public bool EstaAtrasada(DateTime dataReferencia)
    {
        if (Status is not (StatusTarefa.Pendente or StatusTarefa.EmAndamento))
        {
            return false;
        }

        return DataPrazo.Date < dataReferencia.Date;
    }

    public void AtualizarStatus(StatusTarefa novoStatus, DateTime dataAtual)
    {
        if (Status == novoStatus)
        {
            return;
        }

        if (!PodeTransitarPara(novoStatus))
        {
            throw new InvalidOperationException(
                $"Transicao de status invalida: {Status} -> {novoStatus}.");
        }

        Status = novoStatus;

        if (novoStatus == StatusTarefa.Concluida)
        {
            DataConclusao = dataAtual;
            return;
        }

        DataConclusao = null;
    }

    public bool PodeTransitarPara(StatusTarefa novoStatus)
    {
        if (Status == novoStatus)
        {
            return true;
        }

        return Status switch
        {
            StatusTarefa.Pendente => novoStatus is StatusTarefa.EmAndamento or StatusTarefa.Cancelada,
            StatusTarefa.EmAndamento => novoStatus is StatusTarefa.Concluida or StatusTarefa.Cancelada,
            StatusTarefa.Concluida => false,
            StatusTarefa.Cancelada => false,
            _ => false
        };
    }
}
