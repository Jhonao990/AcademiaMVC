using System.Security.Claims;
using AcademiaMVC.Data;
using AcademiaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaMVC.Controllers;

[Authorize]
public class AlunosController : Controller
{
    private readonly ApplicationDbContext _context;
    public AlunosController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> MeuPerfil()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UsuarioId == uid);
        return View(aluno ?? new Aluno());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MeuPerfil(Aluno model)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var email = User.Identity?.Name ?? "";
        var atual = await _context.Alunos.FirstOrDefaultAsync(a => a.UsuarioId == uid);

        if (atual == null)
        {
            if (await _context.Alunos.AnyAsync(a => a.UsuarioId == uid))
                ModelState.AddModelError("", "Já existe um perfil vinculado a esta conta.");
            model.UsuarioId = uid;
            model.Email = email;
            if (!ModelState.IsValid) return View(model);
            _context.Alunos.Add(model);
        }
        else
        {
            atual.Nome = model.Nome;
            atual.Telefone = model.Telefone;
            atual.DataNascimento = model.DataNascimento;
            atual.Email = email;
            if (!ModelState.IsValid) return View(atual);
        }
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Perfil salvo com sucesso.";
        return RedirectToAction(nameof(MeuPerfil));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? busca)
    {
        var query = _context.Alunos.Include(a => a.Usuario).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(a => a.Nome.Contains(busca) || a.Email.Contains(busca));
        ViewBag.Busca = busca;
        return View(await query.OrderBy(a => a.Nome).ToListAsync());
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Aluno aluno)
    {
        if (await _context.Alunos.AnyAsync(a => a.UsuarioId == aluno.UsuarioId))
            ModelState.AddModelError(nameof(aluno.UsuarioId), "Esta conta já possui um cadastro de aluno.");
        if (!ModelState.IsValid) return View(aluno);
        _context.Add(aluno);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Alunos.Include(a => a.Matriculas).ThenInclude(m => m.Modalidade).FirstOrDefaultAsync(a => a.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Alunos.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Aluno aluno)
    {
        if (id != aluno.Id) return NotFound();
        if (!ModelState.IsValid) return View(aluno);
        _context.Update(aluno);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Alunos.FindAsync(id);
        if (item == null) return NotFound();
        if (await _context.Matriculas.AnyAsync(m => m.AlunoId == id))
        {
            TempData["Erro"] = "Não é possível excluir aluno com histórico de matrículas.";
            return RedirectToAction(nameof(Index));
        }
        _context.Alunos.Remove(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
