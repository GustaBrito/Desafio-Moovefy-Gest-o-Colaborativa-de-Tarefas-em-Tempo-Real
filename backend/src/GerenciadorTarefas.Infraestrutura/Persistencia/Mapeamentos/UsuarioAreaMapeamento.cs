using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class UsuarioAreaMapeamento : IEntityTypeConfiguration<UsuarioArea>
{
    public void Configure(EntityTypeBuilder<UsuarioArea> builder)
    {
        builder.ToTable("usuarios_areas");

        builder.HasKey(vinculo => new { vinculo.UsuarioId, vinculo.AreaId });

        builder.Property(vinculo => vinculo.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(vinculo => vinculo.AreaId)
            .HasColumnName("area_id")
            .IsRequired();

        builder.HasOne(vinculo => vinculo.Usuario)
            .WithMany(usuario => usuario.AreasVinculadas)
            .HasForeignKey(vinculo => vinculo.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_usuarios_areas_usuarios_usuario_id");

        builder.HasOne(vinculo => vinculo.Area)
            .WithMany(area => area.UsuariosVinculados)
            .HasForeignKey(vinculo => vinculo.AreaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_usuarios_areas_areas_area_id");

        builder.HasIndex(vinculo => vinculo.AreaId)
            .HasDatabaseName("ix_usuarios_areas_area_id");
    }
}
