using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AcademiaMVC.Models;

public class Aluno
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = "";

    [Required, Phone]
    public string Telefone { get; set; } = "";

    [EmailAddress]
    public string Email { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Display(Name = "Conta de acesso")]
    public string UsuarioId { get; set; } = "";

    public IdentityUser? Usuario { get; set; }
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
