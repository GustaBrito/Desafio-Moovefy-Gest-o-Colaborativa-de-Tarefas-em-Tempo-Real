using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class ProjetoAreaMapeamento : IEntityTypeConfiguration<ProjetoArea>
{
    public void Configure(EntityTypeBuilder<ProjetoArea> builder)
    {
        builder.ToTable("projetos_areas");

        builder.HasKey(vinculo => new { vinculo.ProjetoId, vinculo.AreaId });

        builder.Property(vinculo => vinculo.ProjetoId)
            .HasColumnName("projeto_id")
            .IsRequired();

        builder.Property(vinculo => vinculo.AreaId)
            .HasColumnName("area_id")
            .IsRequired();

        builder.HasOne(vinculo => vinculo.Projeto)
            .WithMany(projeto => projeto.AreasVinculadas)
            .HasForeignKey(vinculo => vinculo.ProjetoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_projetos_areas_vinculo_projeto");

        builder.HasOne(vinculo => vinculo.Area)
            .WithMany()
            .HasForeignKey(vinculo => vinculo.AreaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_projetos_areas_vinculo_area");

        builder.HasIndex(vinculo => vinculo.AreaId)
            .HasDatabaseName("ix_projetos_areas_area_id");
    }
}
