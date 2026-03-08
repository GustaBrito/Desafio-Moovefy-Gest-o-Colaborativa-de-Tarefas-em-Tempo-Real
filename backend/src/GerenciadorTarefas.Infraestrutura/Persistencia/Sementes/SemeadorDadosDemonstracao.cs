using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;

public static class SemeadorDadosDemonstracao
{
    private static readonly Guid ResponsavelAdministradorId =
        Guid.Parse("8c519a4d-3f6d-4d0b-8b77-6ee8f5735990");

    private static readonly Guid ResponsavelColaboradorId =
        Guid.Parse("f3af6b8c-58de-4225-a1d2-838b22f2d08e");

    private static readonly Guid ProjetoPlataformaId =
        Guid.Parse("6d3535f6-0b9e-44f4-9f00-53db1c270ec9");

    private static readonly Guid ProjetoAplicativoId =
        Guid.Parse("30f7f6a1-408e-4fa5-a5c0-1ce6b1ea5f85");

    private static readonly Guid ProjetoDadosId =
        Guid.Parse("7b3ca3ed-64b7-491a-bbd4-bd66adfb6f9b");

    private static readonly Guid TarefaBacklogRefinamentoId =
        Guid.Parse("e17e2236-ec1c-4dfa-95cb-58dd17d7bf8d");

    private static readonly Guid TarefaMigracaoApiId =
        Guid.Parse("6b62c278-4d58-4f17-95d7-5bc9e7f79b84");

    private static readonly Guid TarefaPipelineCiId =
        Guid.Parse("f0ca7d7e-9b88-4b2b-b100-f411e4265f09");

    private static readonly Guid TarefaHistoricoNotificacoesId =
        Guid.Parse("9236c40a-dd27-4b03-9015-c35a9d2b242f");

    private static readonly Guid TarefaRelatorioLegadoId =
        Guid.Parse("be2ea81d-2395-46ed-bfbe-17dbbc8ff56f");

    public static async Task AplicarAsync(
        ContextoBancoDados contextoBancoDados,
        CancellationToken cancellationToken = default)
    {
        var existeProjetos = await contextoBancoDados.Projetos.AnyAsync(cancellationToken);
        var existeTarefas = await contextoBancoDados.Tarefas.AnyAsync(cancellationToken);

        if (existeProjetos || existeTarefas)
        {
            return;
        }

        var dataReferencia = DateTime.UtcNow;
        var projetos = CriarProjetosDemonstracao(dataReferencia);
        var tarefas = CriarTarefasDemonstracao(dataReferencia);
        var notificacoes = CriarNotificacoesDemonstracao(dataReferencia);

        await contextoBancoDados.Projetos.AddRangeAsync(projetos, cancellationToken);
        await contextoBancoDados.Tarefas.AddRangeAsync(tarefas, cancellationToken);
        await contextoBancoDados.Notificacoes.AddRangeAsync(notificacoes, cancellationToken);

        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<Projeto> CriarProjetosDemonstracao(DateTime dataReferencia)
    {
        return
        [
            new Projeto
            {
                Id = ProjetoPlataformaId,
                Nome = "Plataforma Corporativa",
                Descricao = "Evolucao da plataforma central de operacoes internas.",
                DataCriacao = dataReferencia.AddDays(-20)
            },
            new Projeto
            {
                Id = ProjetoAplicativoId,
                Nome = "Aplicativo do Cliente",
                Descricao = "Melhorias de experiencia de uso no aplicativo mobile.",
                DataCriacao = dataReferencia.AddDays(-15)
            },
            new Projeto
            {
                Id = ProjetoDadosId,
                Nome = "Modernizacao de Dados",
                Descricao = "Reestruturacao de integrações e governanca de dados.",
                DataCriacao = dataReferencia.AddDays(-12)
            }
        ];
    }

    private static IReadOnlyCollection<Tarefa> CriarTarefasDemonstracao(DateTime dataReferencia)
    {
        return
        [
            new Tarefa
            {
                Id = TarefaBacklogRefinamentoId,
                Titulo = "Refinar backlog do trimestre",
                Descricao = "Revisar prioridades com o time de produto.",
                Status = StatusTarefa.Pendente,
                Prioridade = PrioridadeTarefa.Media,
                ProjetoId = ProjetoPlataformaId,
                ResponsavelId = ResponsavelAdministradorId,
                DataCriacao = dataReferencia.AddDays(-6),
                DataPrazo = dataReferencia.AddDays(4)
            },
            new Tarefa
            {
                Id = TarefaMigracaoApiId,
                Titulo = "Concluir migracao da API legada",
                Descricao = "Finalizar endpoints faltantes e validar observabilidade.",
                Status = StatusTarefa.EmAndamento,
                Prioridade = PrioridadeTarefa.Alta,
                ProjetoId = ProjetoDadosId,
                ResponsavelId = ResponsavelColaboradorId,
                DataCriacao = dataReferencia.AddDays(-9),
                DataPrazo = dataReferencia.AddDays(-1)
            },
            new Tarefa
            {
                Id = TarefaPipelineCiId,
                Titulo = "Implementar pipeline CI principal",
                Descricao = "Executar build, testes e analise estatico no fluxo de pull request.",
                Status = StatusTarefa.Concluida,
                Prioridade = PrioridadeTarefa.Urgente,
                ProjetoId = ProjetoPlataformaId,
                ResponsavelId = ResponsavelAdministradorId,
                DataCriacao = dataReferencia.AddDays(-10),
                DataPrazo = dataReferencia.AddDays(-3),
                DataConclusao = dataReferencia.AddDays(-4)
            },
            new Tarefa
            {
                Id = TarefaHistoricoNotificacoesId,
                Titulo = "Adicionar historico de notificacoes",
                Descricao = "Persistir eventos de atribuicao e reatribuicao para auditoria.",
                Status = StatusTarefa.Concluida,
                Prioridade = PrioridadeTarefa.Alta,
                ProjetoId = ProjetoAplicativoId,
                ResponsavelId = ResponsavelColaboradorId,
                DataCriacao = dataReferencia.AddDays(-11),
                DataPrazo = dataReferencia.AddDays(-7),
                DataConclusao = dataReferencia.AddDays(-5)
            },
            new Tarefa
            {
                Id = TarefaRelatorioLegadoId,
                Titulo = "Cancelar relatorio legado",
                Descricao = "Iniciativa interrompida por mudanca de estrategia.",
                Status = StatusTarefa.Cancelada,
                Prioridade = PrioridadeTarefa.Baixa,
                ProjetoId = ProjetoAplicativoId,
                ResponsavelId = ResponsavelAdministradorId,
                DataCriacao = dataReferencia.AddDays(-8),
                DataPrazo = dataReferencia.AddDays(2)
            }
        ];
    }

    private static IReadOnlyCollection<Notificacao> CriarNotificacoesDemonstracao(DateTime dataReferencia)
    {
        return
        [
            new Notificacao
            {
                Id = Guid.Parse("85f137ad-0212-4637-b2bd-630a4e38944e"),
                ResponsavelId = ResponsavelColaboradorId,
                TarefaId = TarefaMigracaoApiId,
                ProjetoId = ProjetoDadosId,
                TituloTarefa = "Concluir migracao da API legada",
                Mensagem = "Voce foi atribuido para continuar a migracao da API legada.",
                Reatribuicao = false,
                DataCriacao = dataReferencia.AddDays(-2)
            },
            new Notificacao
            {
                Id = Guid.Parse("1efe7a17-7f9f-446e-a57a-9b6e1ad7dd01"),
                ResponsavelId = ResponsavelAdministradorId,
                TarefaId = TarefaPipelineCiId,
                ProjetoId = ProjetoPlataformaId,
                TituloTarefa = "Implementar pipeline CI principal",
                Mensagem = "A tarefa foi reatribuida para concluir a configuracao de CI.",
                Reatribuicao = true,
                DataCriacao = dataReferencia.AddDays(-6)
            }
        ];
    }
}
