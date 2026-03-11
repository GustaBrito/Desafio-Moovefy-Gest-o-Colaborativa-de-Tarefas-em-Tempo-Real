using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Mapeamentos;

public sealed class UsuarioMapeamento : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(usuario => usuario.Nome)
            .HasColumnName("nome")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(usuario => usuario.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(600)
            .IsRequired();

        builder.Property(usuario => usuario.PerfilGlobal)
            .HasColumnName("perfil_global")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(usuario => usuario.Ativo)
            .HasColumnName("ativo")
            .IsRequired();

        builder.Property(usuario => usuario.DataCriacao)
            .HasColumnName("data_criacao")
            .IsRequired();

        builder.Property(usuario => usuario.UltimoAcesso)
            .HasColumnName("ultimo_acesso");

        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("ux_usuarios_email");
    }
}
