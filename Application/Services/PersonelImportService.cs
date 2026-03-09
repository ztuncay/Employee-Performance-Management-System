using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;

namespace PerformansSitesi.Application.Services;

public static class PersonelImportService
{
    public static readonly string[] ExpectedHeaders =
    {
        "Sicil No",
        "Personel Adı Soyadı",
        "Görevi",
        "Proje Adı",
        "İşe Giriş Tarihi",
        "İşten Çıkış Tarihi",
        "Müdürlük",
        "Lokasyon",
        "1. Yönetici",
        "2. Yönetici",
        "Bölge Müdürü"
    };

    public class ImportRow
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
        public List<string> Errors { get; set; } = new();
    }

    public class ParseResult
    {
        public List<string> Headers { get; set; } = new();
        public List<ImportRow> Rows { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? FatalError { get; set; }
    }

    public class ImportResult
    {
        public int Eklenen { get; set; }
        public int Guncellenen { get; set; }
        public List<string> Hatalar { get; set; } = new();
        public bool Success => !Hatalar.Any();
    }

    public static ParseResult Parse(byte[] fileBytes, int previewMaxRow = 500)
    {
        var result = new ParseResult();

        try
        {
            using var ms = new MemoryStream(fileBytes);
            using var spreadsheetDocument = SpreadsheetDocument.Open(ms, false);

            var workbookPart = spreadsheetDocument.WorkbookPart;
            if (workbookPart == null)
            {
                result.FatalError = "Excel dosyası okunamadı.";
                return result;
            }

            var worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
            if (worksheetPart == null)
            {
                result.FatalError = "Excel içinde sayfa bulunamadı.";
                return result;
            }

            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.Elements<SheetData>().FirstOrDefault();

            if (sheetData == null)
            {
                result.FatalError = "Excel verisi bulunamadı.";
                return result;
            }

            var rows = sheetData.Elements<Row>().ToList();

            if (rows.Count == 0)
            {
                result.FatalError = "Excel boş.";
                return result;
            }

            var headerRow = rows.FirstOrDefault();
            if (headerRow == null)
            {
                result.FatalError = "Header satırı bulunamadı.";
                return result;
            }

            var headerCells = headerRow.Elements<Cell>().ToList();

            for (int i = 0; i < ExpectedHeaders.Length; i++)
            {
                var cellValue = GetCellValue(headerCells.ElementAtOrDefault(i), workbookPart);
                result.Headers.Add(cellValue ?? "");
            }

            for (int i = 0; i < ExpectedHeaders.Length; i++)
            {
                var expected = ExpectedHeaders[i];
                var actual = result.Headers.Count > i ? result.Headers[i] : "";

                if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
                {
                    result.FatalError =
                        $"Excel başlıkları beklenen formatta değil. " +
                        $"Sütun {i + 1}: Beklenen='{expected}', Gelen='{actual}'. " +
                        $"Lütfen başlıkları aynen şu sırayla kullanın: {string.Join(" | ", ExpectedHeaders)}";
                    return result;
                }
            }

            int maxRows = previewMaxRow <= 0 ? rows.Count : Math.Min(rows.Count, previewMaxRow + 1);

            for (int r = 1; r < Math.Min(rows.Count, maxRows); r++)
            {
                var row = rows[r];
                var cells = row.Elements<Cell>().ToList();

                var importRow = new ImportRow { RowNo = r + 1 };

                importRow.SicilNo = (GetCellValue(cells.ElementAtOrDefault(0), workbookPart) ?? "").Trim();
                importRow.AdSoyad = (GetCellValue(cells.ElementAtOrDefault(1), workbookPart) ?? "").Trim();
                importRow.Gorev = (GetCellValue(cells.ElementAtOrDefault(2), workbookPart) ?? "").Trim();
                importRow.ProjeAdi = (GetCellValue(cells.ElementAtOrDefault(3), workbookPart) ?? "").Trim();

                var iseGirisStr = GetCellValue(cells.ElementAtOrDefault(4), workbookPart);
                if (!string.IsNullOrWhiteSpace(iseGirisStr) && DateTime.TryParse(iseGirisStr, out var iseGiris))
                    importRow.IseGirisTarihi = iseGiris;

                var istenCikisStr = GetCellValue(cells.ElementAtOrDefault(5), workbookPart);
                if (!string.IsNullOrWhiteSpace(istenCikisStr) && DateTime.TryParse(istenCikisStr, out var istenCikis))
                    importRow.IstenCikisTarihi = istenCikis;

                importRow.Mudurluk = (GetCellValue(cells.ElementAtOrDefault(6), workbookPart) ?? "").Trim();
                importRow.Lokasyon = (GetCellValue(cells.ElementAtOrDefault(7), workbookPart) ?? "").Trim();
                importRow.Yonetici1 = (GetCellValue(cells.ElementAtOrDefault(8), workbookPart) ?? "").Trim();
                importRow.Yonetici2 = (GetCellValue(cells.ElementAtOrDefault(9), workbookPart) ?? "").Trim();
                importRow.NihaiYonetici = (GetCellValue(cells.ElementAtOrDefault(10), workbookPart) ?? "").Trim();

                if (string.IsNullOrWhiteSpace(importRow.SicilNo)
                    && string.IsNullOrWhiteSpace(importRow.AdSoyad)
                    && string.IsNullOrWhiteSpace(importRow.Gorev))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(importRow.SicilNo))
                    importRow.Errors.Add("Sicil No boş olamaz.");

                if (string.IsNullOrWhiteSpace(importRow.AdSoyad))
                    importRow.Errors.Add("Ad Soyad boş olamaz.");

                if (string.IsNullOrWhiteSpace(importRow.Gorev))
                    importRow.Errors.Add("Görev boş olamaz.");

                result.Rows.Add(importRow);
            }

            var dupSicil = result.Rows
                .Where(x => !string.IsNullOrWhiteSpace(x.SicilNo) && x.Errors.Count == 0)
                .GroupBy(x => x.SicilNo.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupSicil.Count > 0)
                result.Warnings.Add($"Excel içinde mükerrer Sicil No: {string.Join(", ", dupSicil.Take(10))}{(dupSicil.Count > 10 ? " ..." : "")}");

            return result;
        }
        catch (Exception ex)
        {
            result.FatalError = $"Dosya işlenirken hata: {ex.Message}";
            return result;
        }
    }

    public static async Task<ImportResult> DoImportAsync(PerformansDbContext db, List<ImportRow> rows)
    {
        var result = new ImportResult();

        var tumPersoneller = await db.Personeller.ToListAsync();
        var tumKullanicilar = await db.Kullanicilar.ToListAsync();
        const int batchSize = 50;
        var batch = new List<Personel>();

        foreach (var row in rows.Where(r => r.Errors.Count == 0))
        {
            var sicilKey = row.SicilNo.Trim();

            var existing = tumPersoneller.FirstOrDefault(p =>
                p.SicilNo.Equals(sicilKey, StringComparison.OrdinalIgnoreCase));

            var yonetici1 = !string.IsNullOrWhiteSpace(row.Yonetici1)
                ? tumKullanicilar.FirstOrDefault(k => k.KullaniciAdi.Equals(row.Yonetici1, StringComparison.OrdinalIgnoreCase))
                : null;

            var yonetici2 = !string.IsNullOrWhiteSpace(row.Yonetici2)
                ? tumKullanicilar.FirstOrDefault(k => k.KullaniciAdi.Equals(row.Yonetici2, StringComparison.OrdinalIgnoreCase))
                : null;

            var nihaiYonetici = !string.IsNullOrWhiteSpace(row.NihaiYonetici)
                ? tumKullanicilar.FirstOrDefault(k => k.KullaniciAdi.Equals(row.NihaiYonetici, StringComparison.OrdinalIgnoreCase))
                : null;

            if (existing != null)
            {
                existing.AdSoyad = row.AdSoyad;
                existing.Gorev = row.Gorev;
                existing.ProjeAdi = row.ProjeAdi;
                existing.IseGirisTarihi = row.IseGirisTarihi;
                existing.IstenCikisTarihi = row.IstenCikisTarihi;
                existing.Mudurluk = row.Mudurluk;
                existing.Lokasyon = row.Lokasyon;
                existing.Yonetici1Id = yonetici1?.KullaniciId;
                existing.Yonetici2Id = yonetici2?.KullaniciId;
                existing.NihaiYoneticiId = nihaiYonetici?.KullaniciId;

                result.Guncellenen++;
            }
            else
            {
                var yeniPersonel = new Personel
                {
                    SicilNo = row.SicilNo,
                    AdSoyad = row.AdSoyad,
                    Gorev = row.Gorev,
                    ProjeAdi = row.ProjeAdi,
                    IseGirisTarihi = row.IseGirisTarihi,
                    IstenCikisTarihi = row.IstenCikisTarihi,
                    Mudurluk = row.Mudurluk,
                    Lokasyon = row.Lokasyon,
                    Yonetici1Id = yonetici1?.KullaniciId,
                    Yonetici2Id = yonetici2?.KullaniciId,
                    NihaiYoneticiId = nihaiYonetici?.KullaniciId,
                    AktifMi = true
                };

                batch.Add(yeniPersonel);
                tumPersoneller.Add(yeniPersonel);
                result.Eklenen++;

                if (batch.Count >= batchSize)
                {
                    db.Personeller.AddRange(batch);
                    await db.SaveChangesAsync();
                    batch.Clear();
                }
            }
        }

        if (batch.Count > 0)
        {
            db.Personeller.AddRange(batch);
            await db.SaveChangesAsync();
        }

        if (result.Guncellenen > 0)
        {
            await db.SaveChangesAsync();
        }

        return result;
    }

    private static string? GetCellValue(Cell? cell, WorkbookPart workbookPart)
    {
        if (cell?.CellValue == null)
            return null;

        var value = cell.CellValue.Text;

        if (cell.DataType?.Value == CellValues.SharedString)
        {
            var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
            if (sharedStringTable != null)
            {
                int index = int.Parse(value);
                value = sharedStringTable.ElementAt(index).InnerText;
            }
        }

        return value;
    }
}
