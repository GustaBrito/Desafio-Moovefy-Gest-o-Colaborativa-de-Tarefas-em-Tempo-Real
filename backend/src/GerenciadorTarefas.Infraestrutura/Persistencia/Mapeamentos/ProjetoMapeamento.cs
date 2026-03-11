using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class ProjetoMapeamento : IEntityTypeConfiguration<Projeto>
{
    public void Configure(EntityTypeBuilder<Projeto> builder)
    {
        builder.ToTable("projetos");

        builder.HasKey(projeto => projeto.Id);

        builder.Property(projeto => projeto.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(projeto => projeto.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(projeto => projeto.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(1000);

        builder.Property(projeto => projeto.AreaId)
            .HasColumnName("area_id")
            .IsRequired();

        builder.Property(projeto => projeto.CriadoPorUsuarioId)
            .HasColumnName("criado_por_usuario_id");

        builder.Property(projeto => projeto.GestorUsuarioId)
            .HasColumnName("gestor_usuario_id");

        builder.Property(projeto => projeto.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.HasOne(projeto => projeto.Area)
            .WithMany()
            .HasForeignKey(projeto => projeto.AreaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_projetos_areas_area_id");

        builder.HasOne(projeto => projeto.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(projeto => projeto.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_projetos_usuarios_criado_por");

        builder.HasOne(projeto => projeto.GestorUsuario)
            .WithMany()
            .HasForeignKey(projeto => projeto.GestorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_projetos_usuarios_gestor");

        builder.HasIndex(projeto => projeto.Nome)
            .HasDatabaseName("ix_projetos_nome");

        builder.HasIndex(projeto => projeto.AreaId)
            .HasDatabaseName("ix_projetos_area_id");
    }
}
