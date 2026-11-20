using AcademiaMVC.Data;
using AcademiaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademiaMVC.Controllers;

public class ModalidadesController : Controller
{
    private readonly ApplicationDbContext _context;
    public ModalidadesController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index(string? busca)
    {
        var query = _context.Modalidades.AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(m => m.Nome.Contains(busca) || m.Descricao.Contains(busca));
        ViewBag.Busca = busca;
        return View(await query.OrderBy(m => m.Nome).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Modalidades.FirstOrDefaultAsync(m => m.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Modalidade modalidade)
    {
        if (!ModelState.IsValid) return View(modalidade);
        _context.Add(modalidade);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Modalidade cadastrada.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Modalidades.FindAsync(id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Modalidade modalidade)
    {
        if (id != modalidade.Id) return NotFound();
        if (!ModelState.IsValid) return View(modalidade);
        _context.Update(modalidade);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Modalidade atualizada.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Modalidades.FirstOrDefaultAsync(m => m.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Modalidades.FindAsync(id);
        if (item == null) return NotFound();
        if (await _context.Matriculas.AnyAsync(m => m.ModalidadeId == id))
        {
            TempData["Erro"] = "Não é possível excluir uma modalidade que possui matrículas. Desative-a no lugar.";
            return RedirectToAction(nameof(Index));
        }
        _context.Modalidades.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Modalidade excluída.";
        return RedirectToAction(nameof(Index));
    }
}
