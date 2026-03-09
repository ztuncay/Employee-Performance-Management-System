using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Web.ViewModels;

public class DonemFormViewModel
{
    public int DonemId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Ad { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    public DateTime BaslangicTarihi { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime BitisTarihi { get; set; }

    public bool AktifMi { get; set; }

    public string? Hata { get; set; }
}
