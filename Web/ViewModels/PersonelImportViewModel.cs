using Microsoft.AspNetCore.Http;

namespace PerformansSitesi.Web.ViewModels;

public class PersonelImportViewModel
{
    public IFormFile? Excel { get; set; }

    public bool PreviewReady { get; set; }
    public bool CanImport { get; set; }

    public string? Error { get; set; }
    public List<string> Warnings { get; set; } = new();

    public List<string> Headers { get; set; } = new();

    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }

    public List<RowPreview> PreviewRows { get; set; } = new();

    public string? ImportLog { get; set; }

    public class RowPreview
    {
        public int RowNo { get; set; }

        public string SicilNo { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public string Gorev { get; set; } = "";
        public string ProjeAdi { get; set; } = "";
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public string Mudurluk { get; set; } = "";
        public string Lokasyon { get; set; } = "";

        public string Yonetici1 { get; set; } = "";
        public string Yonetici2 { get; set; } = "";
        public string NihaiYonetici { get; set; } = "";

        public bool Shifted { get; set; }

        public List<string> Notes { get; set; } = new();
        public List<string> RowErrors { get; set; } = new();
    }
}
