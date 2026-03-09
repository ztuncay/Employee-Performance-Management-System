using System.Collections.Generic;

namespace PerformansSitesi.Web.ViewModels;

public class ImportCheckViewModel
{
    public int TanimsizY1 { get; set; }
    public int TanimsizY2 { get; set; }
    public int TanimsizNY { get; set; }

    public List<Row> Yonetici1Dagilim { get; set; } = new();
    public List<Row> Yonetici2Dagilim { get; set; } = new();
    public List<Row> BolgeMudurDagilim { get; set; } = new();

    public class Row
    {
        public string AdSoyad { get; set; } = "";
        public int KisiSayisi { get; set; }
    }
}
