using System.Security.Claims;
using AcademiaMVC.Data;
using AcademiaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcademiaMVC.Controllers;

[Authorize]
public class MatriculasController : Controller
{
    private readonly ApplicationDbContext _context;
    public MatriculasController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Minhas()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UsuarioId == uid);
        if (aluno == null)
        {
            TempData["Aviso"] = "Complete seu perfil antes de solicitar uma matrícula.";
            return RedirectToAction("MeuPerfil", "Alunos");
        }
        var itens = await _context.Matriculas.Include(m => m.Modalidade)
            .Where(m => m.AlunoId == aluno.Id).OrderByDescending(m => m.DataMatricula).ToListAsync();
        return View(itens);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Solicitar(int modalidadeId)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.UsuarioId == uid);
        if (aluno == null)
        {
            TempData["Aviso"] = "Complete seu perfil antes de solicitar uma matrícula.";
            return RedirectToAction("MeuPerfil", "Alunos");
        }

        var modalidade = await _context.Modalidades.FindAsync(modalidadeId);
        if (modalidade == null) return NotFound();

        // Regra 1: somente modalidades ativas aceitam novas matrículas.
        if (!modalidade.Ativa)
        {
            TempData["Erro"] = "Esta modalidade está temporariamente indisponível.";
            return RedirectToAction("Index", "Modalidades");
        }

        // Regra 2: o mesmo aluno não pode ter duas matrículas ativas na mesma modalidade.
        var duplicada = await _context.Matriculas.AnyAsync(m =>
            m.AlunoId == aluno.Id && m.ModalidadeId == modalidadeId && m.Status == "Ativa");
        if (duplicada)
        {
            TempData["Erro"] = "Você já possui uma matrícula ativa nesta modalidade.";
            return RedirectToAction("Index", "Modalidades");
        }

        // Regra 3: respeita o limite de vagas configurado para a modalidade.
        var ocupadas = await _context.Matriculas.CountAsync(m =>
            m.ModalidadeId == modalidadeId && m.Status == "Ativa");
        if (ocupadas >= modalidade.LimiteVagas)
        {
            TempData["Erro"] = "Não há vagas disponíveis nesta modalidade.";
            return RedirectToAction("Index", "Modalidades");
        }

        _context.Matriculas.Add(new Matricula
        {
            AlunoId = aluno.Id,
            ModalidadeId = modalidade.Id,
            DataMatricula = DateTime.Today,
            Status = "Ativa",
            ValorMensal = modalidade.PrecoMensal
        });
        await _context.SaveChangesAsync();
        TempData["Sucesso"] = "Solicitação de matrícula realizada com sucesso.";
        return RedirectToAction(nameof(Minhas));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var item = await _context.Matriculas.Include(m => m.Aluno).FirstOrDefaultAsync(m => m.Id == id);
        if (item == null) return NotFound();
        if (item.Aluno!.UsuarioId != uid && !User.IsInRole("Admin")) return Forbid();
        if (item.Status == "Ativa")
        {
            item.Status = "Cancelada";
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Matrícula cancelada.";
        }
        return User.IsInRole("Admin") ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(Minhas));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var itens = await _context.Matriculas.Include(m => m.Aluno).Include(m => m.Modalidade)
            .OrderByDescending(m => m.DataMatricula).ToListAsync();
        return View(itens);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Matriculas.Include(m => m.Aluno).Include(m => m.Modalidade)
            .FirstOrDefaultAsync(m => m.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        CarregarListas();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Matricula matricula)
    {
        var modalidade = await _context.Modalidades.FindAsync(matricula.ModalidadeId);
        if (modalidade == null) ModelState.AddModelError(nameof(matricula.ModalidadeId), "Modalidade inválida.");
        else
        {
            if (!modalidade.Ativa) ModelState.AddModelError(nameof(matricula.ModalidadeId), "A modalidade está inativa.");
            var ocupadas = await _context.Matriculas.CountAsync(m => m.ModalidadeId == modalidade.Id && m.Status == "Ativa");
            if (matricula.Status == "Ativa" && ocupadas >= modalidade.LimiteVagas)
                ModelState.AddModelError("", "Limite de vagas atingido.");
            if (matricula.Status == "Ativa" && await _context.Matriculas.AnyAsync(m =>
                m.AlunoId == matricula.AlunoId && m.ModalidadeId == matricula.ModalidadeId && m.Status == "Ativa"))
                ModelState.AddModelError("", "O aluno já possui matrícula ativa nesta modalidade.");
            matricula.ValorMensal = modalidade.PrecoMensal;
        }
        if (!ModelState.IsValid) { CarregarListas(matricula); return View(matricula); }
        _context.Add(matricula);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Matriculas.FindAsync(id);
        if (item == null) return NotFound();
        CarregarListas(item);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, Matricula matricula)
    {
        if (id != matricula.Id) return NotFound();
        var modalidade = await _context.Modalidades.FindAsync(matricula.ModalidadeId);
        if (modalidade == null) ModelState.AddModelError(nameof(matricula.ModalidadeId), "Modalidade inválida.");
        else matricula.ValorMensal = modalidade.PrecoMensal;
        if (!ModelState.IsValid) { CarregarListas(matricula); return View(matricula); }
        _context.Update(matricula);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Matriculas.Include(m => m.Aluno).Include(m => m.Modalidade)
            .FirstOrDefaultAsync(m => m.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Matriculas.FindAsync(id);
        if (item == null) return NotFound();
        _context.Matriculas.Remove(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void CarregarListas(Matricula? item = null)
    {
        ViewData["AlunoId"] = new SelectList(_context.Alunos.OrderBy(a => a.Nome), "Id", "Nome", item?.AlunoId);
        ViewData["ModalidadeId"] = new SelectList(_context.Modalidades.OrderBy(m => m.Nome), "Id", "Nome", item?.ModalidadeId);
    }
}
