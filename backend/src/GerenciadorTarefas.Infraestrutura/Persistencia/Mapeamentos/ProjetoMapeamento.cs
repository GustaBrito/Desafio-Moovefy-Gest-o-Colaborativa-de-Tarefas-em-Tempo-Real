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

        builder.Property(projeto => projeto.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.HasIndex(projeto => projeto.Nome)
            .HasDatabaseName("ix_projetos_nome");
    }
}
