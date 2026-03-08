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

        builder.Property(notificacao => notificacao.ResponsavelId)
            .HasColumnName("responsavel_id")
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

        builder.HasIndex(notificacao => notificacao.ResponsavelId)
            .HasDatabaseName("ix_notificacoes_responsavel_id");

        builder.HasIndex(notificacao => notificacao.DataCriacao)
            .HasDatabaseName("ix_notificacoes_data_criacao");
    }
}
