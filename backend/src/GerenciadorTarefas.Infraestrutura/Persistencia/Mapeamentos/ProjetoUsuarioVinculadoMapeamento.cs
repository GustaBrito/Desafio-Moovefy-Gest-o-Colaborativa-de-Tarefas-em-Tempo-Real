using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class ProjetoUsuarioVinculadoMapeamento : IEntityTypeConfiguration<ProjetoUsuarioVinculado>
{
    public void Configure(EntityTypeBuilder<ProjetoUsuarioVinculado> builder)
    {
        builder.ToTable("projetos_usuarios_vinculados");

        builder.HasKey(vinculo => new { vinculo.ProjetoId, vinculo.UsuarioId });

        builder.Property(vinculo => vinculo.ProjetoId)
            .HasColumnName("projeto_id")
            .IsRequired();

        builder.Property(vinculo => vinculo.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.HasOne(vinculo => vinculo.Projeto)
            .WithMany(projeto => projeto.UsuariosVinculados)
            .HasForeignKey(vinculo => vinculo.ProjetoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_projetos_usuarios_vinculo_projeto");

        builder.HasOne(vinculo => vinculo.Usuario)
            .WithMany()
            .HasForeignKey(vinculo => vinculo.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_projetos_usuarios_vinculo_usuario");

        builder.HasIndex(vinculo => vinculo.UsuarioId)
            .HasDatabaseName("ix_projetos_usuarios_vinculados_usuario_id");
    }
}
