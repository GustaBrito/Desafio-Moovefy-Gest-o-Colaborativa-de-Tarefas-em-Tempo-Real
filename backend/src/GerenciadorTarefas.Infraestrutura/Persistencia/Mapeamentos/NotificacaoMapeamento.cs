using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class NotificacaoMapeamento : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.ToTable("notificacoes");

        builder.HasKey(notificacao => notificacao.Id);

        builder.Property(notificacao => notificacao.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(notificacao => notificacao.ResponsavelUsuarioId)
            .HasColumnName("responsavel_usuario_id")
            .IsRequired();

        builder.Property(notificacao => notificacao.TarefaId)
            .HasColumnName("tarefa_id")
            .IsRequired();

        builder.Property(notificacao => notificacao.ProjetoId)
            .HasColumnName("projeto_id")
            .IsRequired();

        builder.Property(notificacao => notificacao.TituloTarefa)
            .HasColumnName("titulo_tarefa")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(notificacao => notificacao.Mensagem)
            .HasColumnName("mensagem")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(notificacao => notificacao.Reatribuicao)
            .HasColumnName("reatribuicao")
            .IsRequired();

        builder.Property(notificacao => notificacao.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.HasOne(notificacao => notificacao.ResponsavelUsuario)
            .WithMany()
            .HasForeignKey(notificacao => notificacao.ResponsavelUsuarioId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notificacoes_usuarios_responsavel_usuario_id");

        builder.HasOne(notificacao => notificacao.Tarefa)
            .WithMany()
            .HasForeignKey(notificacao => notificacao.TarefaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notificacoes_tarefas_tarefa_id");

        builder.HasOne(notificacao => notificacao.Projeto)
            .WithMany()
            .HasForeignKey(notificacao => notificacao.ProjetoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notificacoes_projetos_projeto_id");

        builder.HasIndex(notificacao => notificacao.ResponsavelUsuarioId)
            .HasDatabaseName("ix_notificacoes_responsavel_usuario_id");

        builder.HasIndex(notificacao => notificacao.DataCriacao)
            .HasDatabaseName("ix_notificacoes_data_criacao");
    }
}
