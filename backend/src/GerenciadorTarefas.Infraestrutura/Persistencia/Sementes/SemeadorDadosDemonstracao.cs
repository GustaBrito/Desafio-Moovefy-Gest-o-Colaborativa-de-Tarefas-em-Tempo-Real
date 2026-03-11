using GerenciadorTarefas.Dominio.Entidades;
using GerenciadorTarefas.Dominio.Enumeracoes;
using GerenciadorTarefas.Infraestrutura.Seguranca;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Sementes;

public static class SemeadorDadosDemonstracao
{
    public static readonly Guid AreaMarketingId =
        Guid.Parse("6b7cb48b-89d9-480f-9610-3e2f4f2d51e1");

    public static readonly Guid AreaDesenvolvimentoSoftwareId =
        Guid.Parse("850173be-13f1-4f0d-a7d1-4e81f8604e18");

    public static readonly Guid AreaGestaoId =
        Guid.Parse("ef640fdd-ef74-45f2-a72f-3f4f72c72269");

    public static readonly Guid AreaGeralId =
        Guid.Parse("a5f46885-6b01-4f72-9f4e-c11c00da8df0");

    public static readonly Guid UsuarioSuperAdminId =
        Guid.Parse("8c519a4d-3f6d-4d0b-8b77-6ee8f5735990");

    public static readonly Guid UsuarioAdminMarketingId =
        Guid.Parse("63124f92-0f2c-43fe-8e94-4de8d17dd0b8");

    public static readonly Guid UsuarioAdminDesenvolvimentoId =
        Guid.Parse("7d3655d6-3744-4525-920e-24f4846e4ebe");

    public static readonly Guid UsuarioAdminGestaoId =
        Guid.Parse("c5908755-2e5e-4cd2-a926-373f4eaf8757");

    public static readonly Guid UsuarioColaboradorDesenvolvimentoId =
        Guid.Parse("f3af6b8c-58de-4225-a1d2-838b22f2d08e");

    public static readonly Guid UsuarioColaboradorMarketingId =
        Guid.Parse("4b9a4f8f-d5f4-4f6f-bc8f-eebc8197fcb0");

    public static readonly Guid ProjetoPlataformaId =
        Guid.Parse("6d3535f6-0b9e-44f4-9f00-53db1c270ec9");

    public static readonly Guid ProjetoAplicativoId =
        Guid.Parse("30f7f6a1-408e-4fa5-a5c0-1ce6b1ea5f85");

    public static readonly Guid ProjetoDadosId =
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
        var hasher = new ServicoHashSenhaPbkdf2();
        var dataReferencia = DateTime.UtcNow;

        var areasDemonstracao = CriarAreasDemonstracao();
        var areasExistentesPorId = await contextoBancoDados.Areas
            .ToDictionaryAsync(area => area.Id, cancellationToken);
        foreach (var areaDemonstracao in areasDemonstracao)
        {
            if (areasExistentesPorId.TryGetValue(areaDemonstracao.Id, out var areaExistente))
            {
                areaExistente.Nome = areaDemonstracao.Nome;
                areaExistente.Codigo = areaDemonstracao.Codigo;
                areaExistente.Ativa = areaDemonstracao.Ativa;
                continue;
            }

            await contextoBancoDados.Areas.AddAsync(areaDemonstracao, cancellationToken);
        }

        var usuariosDemonstracao = CriarUsuariosDemonstracao(hasher, dataReferencia);
        var usuariosExistentesPorId = await contextoBancoDados.Usuarios
            .ToDictionaryAsync(usuario => usuario.Id, cancellationToken);
        foreach (var usuarioDemonstracao in usuariosDemonstracao)
        {
            if (usuariosExistentesPorId.TryGetValue(usuarioDemonstracao.Id, out var usuarioExistente))
            {
                usuarioExistente.Nome = usuarioDemonstracao.Nome;
                usuarioExistente.Email = usuarioDemonstracao.Email;
                usuarioExistente.SenhaHash = usuarioDemonstracao.SenhaHash;
                usuarioExistente.PerfilGlobal = usuarioDemonstracao.PerfilGlobal;
                usuarioExistente.Ativo = usuarioDemonstracao.Ativo;
                if (usuarioExistente.DataCriacao == default)
                {
                    usuarioExistente.DataCriacao = usuarioDemonstracao.DataCriacao;
                }

                continue;
            }

            await contextoBancoDados.Usuarios.AddAsync(usuarioDemonstracao, cancellationToken);
        }

        var vinculosDemonstracao = CriarVinculosUsuariosAreas();
        var vinculosExistentes = await contextoBancoDados.UsuariosAreas
            .Select(vinculo => $"{vinculo.UsuarioId:N}|{vinculo.AreaId:N}")
            .ToListAsync(cancellationToken);
        var chavesVinculosExistentes = vinculosExistentes.ToHashSet(StringComparer.Ordinal);
        var vinculosParaAdicionar = vinculosDemonstracao
            .Where(vinculo => !chavesVinculosExistentes.Contains($"{vinculo.UsuarioId:N}|{vinculo.AreaId:N}"))
            .ToArray();
        if (vinculosParaAdicionar.Length > 0)
        {
            await contextoBancoDados.UsuariosAreas.AddRangeAsync(vinculosParaAdicionar, cancellationToken);
        }

        var projetosDemonstracao = CriarProjetosDemonstracao(dataReferencia);
        var projetoIdsExistentes = await contextoBancoDados.Projetos
            .Select(projeto => projeto.Id)
            .ToListAsync(cancellationToken);
        var projetosParaAdicionar = projetosDemonstracao
            .Where(projeto => !projetoIdsExistentes.Contains(projeto.Id))
            .ToArray();
        if (projetosParaAdicionar.Length > 0)
        {
            await contextoBancoDados.Projetos.AddRangeAsync(projetosParaAdicionar, cancellationToken);
        }

        var tarefasDemonstracao = CriarTarefasDemonstracao(dataReferencia);
        var tarefaIdsExistentes = await contextoBancoDados.Tarefas
            .Select(tarefa => tarefa.Id)
            .ToListAsync(cancellationToken);
        var tarefasParaAdicionar = tarefasDemonstracao
            .Where(tarefa => !tarefaIdsExistentes.Contains(tarefa.Id))
            .ToArray();
        if (tarefasParaAdicionar.Length > 0)
        {
            await contextoBancoDados.Tarefas.AddRangeAsync(tarefasParaAdicionar, cancellationToken);
        }

        var notificacoesDemonstracao = CriarNotificacoesDemonstracao(dataReferencia);
        var notificacaoIdsExistentes = await contextoBancoDados.Notificacoes
            .Select(notificacao => notificacao.Id)
            .ToListAsync(cancellationToken);
        var notificacoesParaAdicionar = notificacoesDemonstracao
            .Where(notificacao => !notificacaoIdsExistentes.Contains(notificacao.Id))
            .ToArray();
        if (notificacoesParaAdicionar.Length > 0)
        {
            await contextoBancoDados.Notificacoes.AddRangeAsync(notificacoesParaAdicionar, cancellationToken);
        }

        await contextoBancoDados.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyCollection<Area> CriarAreasDemonstracao()
    {
        return
        [
            new Area
            {
                Id = AreaMarketingId,
                Nome = "Marketing",
                Codigo = "MKT",
                Ativa = true
            },
            new Area
            {
                Id = AreaDesenvolvimentoSoftwareId,
                Nome = "Desenvolvimento de Software",
                Codigo = "DEV",
                Ativa = true
            },
            new Area
            {
                Id = AreaGestaoId,
                Nome = "Gestao",
                Codigo = "GST",
                Ativa = true
            },
            new Area
            {
                Id = AreaGeralId,
                Nome = "Geral",
                Codigo = "GERAL",
                Ativa = true
            }
        ];
    }

    private static IReadOnlyCollection<Usuario> CriarUsuariosDemonstracao(
        ServicoHashSenhaPbkdf2 hasher,
        DateTime dataReferencia)
    {
        return
        [
            new Usuario
            {
                Id = UsuarioSuperAdminId,
                Nome = "Super Admin",
                Email = "superadmin@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("SuperAdmin@123"),
                PerfilGlobal = PerfilGlobalUsuario.SuperAdmin,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-120)
            },
            new Usuario
            {
                Id = UsuarioAdminMarketingId,
                Nome = "Admin Marketing",
                Email = "admin.marketing@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("AdminMarketing@123"),
                PerfilGlobal = PerfilGlobalUsuario.Admin,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-100)
            },
            new Usuario
            {
                Id = UsuarioAdminDesenvolvimentoId,
                Nome = "Admin Desenvolvimento",
                Email = "admin.dev@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("AdminDev@123"),
                PerfilGlobal = PerfilGlobalUsuario.Admin,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-100)
            },
            new Usuario
            {
                Id = UsuarioAdminGestaoId,
                Nome = "Admin Gestao",
                Email = "admin.gestao@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("AdminGestao@123"),
                PerfilGlobal = PerfilGlobalUsuario.Admin,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-90)
            },
            new Usuario
            {
                Id = UsuarioColaboradorDesenvolvimentoId,
                Nome = "Colaborador Desenvolvimento",
                Email = "colaborador.dev@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("ColaboradorDev@123"),
                PerfilGlobal = PerfilGlobalUsuario.Colaborador,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-60)
            },
            new Usuario
            {
                Id = UsuarioColaboradorMarketingId,
                Nome = "Colaborador Marketing",
                Email = "colaborador.marketing@gerenciadortarefas.local",
                SenhaHash = hasher.GerarHash("ColaboradorMkt@123"),
                PerfilGlobal = PerfilGlobalUsuario.Colaborador,
                Ativo = true,
                DataCriacao = dataReferencia.AddDays(-40)
            }
        ];
    }

    private static IReadOnlyCollection<UsuarioArea> CriarVinculosUsuariosAreas()
    {
        return
        [
            new UsuarioArea
            {
                UsuarioId = UsuarioSuperAdminId,
                AreaId = AreaGeralId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioSuperAdminId,
                AreaId = AreaMarketingId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioSuperAdminId,
                AreaId = AreaDesenvolvimentoSoftwareId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioSuperAdminId,
                AreaId = AreaGestaoId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioAdminMarketingId,
                AreaId = AreaMarketingId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioAdminDesenvolvimentoId,
                AreaId = AreaDesenvolvimentoSoftwareId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioAdminGestaoId,
                AreaId = AreaGestaoId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioColaboradorDesenvolvimentoId,
                AreaId = AreaDesenvolvimentoSoftwareId
            },
            new UsuarioArea
            {
                UsuarioId = UsuarioColaboradorMarketingId,
                AreaId = AreaMarketingId
            }
        ];
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
                AreaId = AreaDesenvolvimentoSoftwareId,
                CriadoPorUsuarioId = UsuarioSuperAdminId,
                GestorUsuarioId = UsuarioAdminDesenvolvimentoId,
                DataCriacao = dataReferencia.AddDays(-20)
            },
            new Projeto
            {
                Id = ProjetoAplicativoId,
                Nome = "Aplicativo do Cliente",
                Descricao = "Melhorias de experiencia de uso no aplicativo mobile.",
                AreaId = AreaMarketingId,
                CriadoPorUsuarioId = UsuarioSuperAdminId,
                GestorUsuarioId = UsuarioAdminMarketingId,
                DataCriacao = dataReferencia.AddDays(-15)
            },
            new Projeto
            {
                Id = ProjetoDadosId,
                Nome = "Modernizacao de Dados",
                Descricao = "Reestruturacao de integracoes e governanca de dados.",
                AreaId = AreaGeralId,
                CriadoPorUsuarioId = UsuarioSuperAdminId,
                GestorUsuarioId = UsuarioAdminDesenvolvimentoId,
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
                ResponsavelUsuarioId = UsuarioAdminDesenvolvimentoId,
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
                ResponsavelUsuarioId = UsuarioColaboradorDesenvolvimentoId,
                DataCriacao = dataReferencia.AddDays(-9),
                DataPrazo = dataReferencia.AddDays(-1)
            },
            new Tarefa
            {
                Id = TarefaPipelineCiId,
                Titulo = "Implementar pipeline CI principal",
                Descricao = "Executar build, testes e analise estatica no fluxo de pull request.",
                Status = StatusTarefa.Concluida,
                Prioridade = PrioridadeTarefa.Urgente,
                ProjetoId = ProjetoPlataformaId,
                ResponsavelUsuarioId = UsuarioAdminDesenvolvimentoId,
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
                ResponsavelUsuarioId = UsuarioColaboradorMarketingId,
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
                ResponsavelUsuarioId = UsuarioAdminMarketingId,
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
                ResponsavelUsuarioId = UsuarioColaboradorDesenvolvimentoId,
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
                ResponsavelUsuarioId = UsuarioAdminDesenvolvimentoId,
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
