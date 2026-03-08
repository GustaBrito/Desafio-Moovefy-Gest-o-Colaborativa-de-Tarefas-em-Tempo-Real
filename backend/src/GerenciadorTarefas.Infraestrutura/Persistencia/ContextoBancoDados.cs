using GerenciadorTarefas.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GerenciadorTarefas.Infraestrutura.Persistencia;

public sealed class ContextoBancoDados : DbContext
{
    public ContextoBancoDados(DbContextOptions<ContextoBancoDados> options)
        : base(options)
    {
    }

    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContextoBancoDados).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
