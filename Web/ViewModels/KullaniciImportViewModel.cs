using PerformansSitesi.Application.Services;

namespace PerformansSitesi.Web.ViewModels;

public class KullaniciImportViewModel
{
    public IFormFile? ExcelFile { get; set; }
    public List<KullaniciImportService.ImportRow> PreviewRows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? FatalError { get; set; }
    public bool PreviewReady { get; set; }
    public bool CanImport { get; set; }
    
    public string ImportLog { get; set; } = "";
}

public class KullaniciImportCheckViewModel
{
    public int EklenenSayi { get; set; }
    public int Guncellenenlerin { get; set; }
    public List<KullaniciImportService.KullaniciInfo> YeniKullanicilar { get; set; } = new();
}
