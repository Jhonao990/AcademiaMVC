using System.ComponentModel.DataAnnotations;

namespace AcademiaMVC.Models;

public class Matricula
{
    public int Id { get; set; }

    [Display(Name = "Aluno")]
    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [Display(Name = "Modalidade")]
    public int ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    [Display(Name = "Data da matrícula")]
    [DataType(DataType.Date)]
    public DateTime DataMatricula { get; set; } = DateTime.Today;

    [Required, StringLength(20)]
    public string Status { get; set; } = "Ativa";

    [Display(Name = "Valor mensal contratado")]
    [Range(0, 10000)]
    public decimal ValorMensal { get; set; }
}
