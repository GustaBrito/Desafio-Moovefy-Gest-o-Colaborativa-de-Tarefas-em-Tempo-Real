using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class TarefaMapeamento : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("tarefas");

        builder.HasKey(tarefa => tarefa.Id);

        builder.Property(tarefa => tarefa.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(tarefa => tarefa.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(tarefa => tarefa.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(2000);

        builder.Property(tarefa => tarefa.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tarefa => tarefa.Prioridade)
            .HasColumnName("prioridade")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(tarefa => tarefa.ProjetoId)
            .HasColumnName("projeto_id")
            .IsRequired();

        builder.Property(tarefa => tarefa.ResponsavelId)
            .HasColumnName("responsavel_id")
            .IsRequired();

        builder.Property(tarefa => tarefa.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.Property(tarefa => tarefa.DataPrazo)
            .HasColumnName("data_prazo")
            .IsRequired();

        builder.Property(tarefa => tarefa.DataConclusao)
            .HasColumnName("data_conclusao");

        builder.HasOne<Projeto>()
            .WithMany()
            .HasForeignKey(tarefa => tarefa.ProjetoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_tarefas_projetos_projeto_id");

        builder.HasIndex(tarefa => tarefa.ProjetoId)
            .HasDatabaseName("ix_tarefas_projeto_id");

        builder.HasIndex(tarefa => tarefa.Status)
            .HasDatabaseName("ix_tarefas_status");

        builder.HasIndex(tarefa => tarefa.ResponsavelId)
            .HasDatabaseName("ix_tarefas_responsavel_id");

        builder.HasIndex(tarefa => tarefa.DataPrazo)
            .HasDatabaseName("ix_tarefas_data_prazo");
    }
}
