using System.ComponentModel.DataAnnotations;

namespace AcademiaMVC.Models;

public class Modalidade
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nome { get; set; } = "";

    [Required, StringLength(500)]
    public string Descricao { get; set; } = "";

    [Required, StringLength(120)]
    public string Horario { get; set; } = "";

    [Range(1, 500)]
    [Display(Name = "Limite de vagas")]
    public int LimiteVagas { get; set; }

    [Range(0, 10000)]
    [Display(Name = "Preço mensal")]
    public decimal PrecoMensal { get; set; }

    [Display(Name = "URL da imagem")]
    public string? ImagemUrl { get; set; }

    public bool Ativa { get; set; } = true;
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
