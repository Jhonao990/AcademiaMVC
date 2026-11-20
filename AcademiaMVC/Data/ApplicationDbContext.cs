using AcademiaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AcademiaMVC.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Aluno> Alunos => Set<Aluno>();
    public DbSet<Modalidade> Modalidades => Set<Modalidade>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Aluno>().HasIndex(a => a.UsuarioId).IsUnique();
        builder.Entity<Aluno>().HasOne(a => a.Usuario)
            .WithMany().HasForeignKey(a => a.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Matricula>().HasOne(m => m.Aluno)
            .WithMany(a => a.Matriculas).HasForeignKey(m => m.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Matricula>().HasOne(m => m.Modalidade)
            .WithMany(m => m.Matriculas).HasForeignKey(m => m.ModalidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Modalidade>().Property(m => m.PrecoMensal).HasPrecision(10, 2);

        builder.Entity<Matricula>().Property(m => m.ValorMensal).HasPrecision(10, 2);
    }
}
