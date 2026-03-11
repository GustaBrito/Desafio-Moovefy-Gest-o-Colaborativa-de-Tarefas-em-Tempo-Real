using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class AreaMapeamento : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("areas");

        builder.HasKey(area => area.Id);

        builder.Property(area => area.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(area => area.Nome)
            .HasColumnName("nome")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(area => area.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(60);

        builder.Property(area => area.Ativa)
            .HasColumnName("ativa")
            .IsRequired();

        builder.HasIndex(area => area.Nome)
            .IsUnique()
            .HasDatabaseName("ux_areas_nome");

        builder.HasIndex(area => area.Codigo)
            .IsUnique()
            .HasDatabaseName("ux_areas_codigo")
            .HasFilter("codigo IS NOT NULL");
    }
}
