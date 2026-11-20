using AcademiaMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcademiaMVC.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        // As tabelas são criadas/atualizadas por migrations. Não substitui Add-Migration/Update-Database.
        if (!await db.Database.CanConnectAsync())
            return;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in new[] { "Admin", "Aluno" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var email = configuration["AdminSeed:Email"] ?? "admin@academia.local";
        var password = configuration["AdminSeed:Password"] ?? "Admin@12345";
        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (!await db.Modalidades.AnyAsync())
        {
            db.Modalidades.AddRange(
                new Modalidade { Nome = "Musculação", Descricao = "Treinamento com pesos e aparelhos.", Horario = "Segunda a sexta, 06h às 22h", LimiteVagas = 30, PrecoMensal = 89.90m, ImagemUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=900", Ativa = true },
                new Modalidade { Nome = "Treinamento Funcional", Descricao = "Exercícios dinâmicos para força, equilíbrio e condicionamento.", Horario = "Terças e quintas, 19h", LimiteVagas = 15, PrecoMensal = 109.90m, ImagemUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=900", Ativa = true },
                new Modalidade { Nome = "Pilates", Descricao = "Atividades de mobilidade, postura e controle corporal.", Horario = "Segundas e quartas, 18h", LimiteVagas = 12, PrecoMensal = 119.90m, ImagemUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=900", Ativa = true }
            );
            await db.SaveChangesAsync();
        }
    }
}
