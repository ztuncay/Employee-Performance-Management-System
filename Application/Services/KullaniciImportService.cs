using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Domain.Enums;
using PerformansSitesi.Infrastructure.Data;

namespace PerformansSitesi.Application.Services;

public static class KullaniciImportService
{
    public static readonly string[] ExpectedHeaders =
    {
        "Ad",
        "Kullanıcı Adı",
        "Email",
        "Rol"
    };

    public class ImportRow
    {
        public int RowNo { get; set; }
        public string Ad { get; set; } = "";
        public string KullaniciAdi { get; set; } = "";
        public string Email { get; set; } = "";
        public string RolStr { get; set; } = "";
        
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
        public List<KullaniciInfo> YeniKullanicilar { get; set; } = new();
        public List<string> Hatalar { get; set; } = new();
        
        public bool Success => !Hatalar.Any();
    }
    
    public class KullaniciInfo
    {
        public string Ad { get; set; } = "";
        public string KullaniciAdi { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "";
        public string VarsayilanSifre { get; set; } = "Sifre123!";
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

            var validRoles = Enum.GetNames(typeof(Rol)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            int maxRows = previewMaxRow <= 0 ? rows.Count : Math.Min(rows.Count, previewMaxRow + 1);

            for (int r = 1; r < Math.Min(rows.Count, maxRows); r++)
            {
                var row = rows[r];
                var cells = row.Elements<Cell>().ToList();

                var importRow = new ImportRow { RowNo = r + 1 };

                importRow.Ad = (GetCellValue(cells.ElementAtOrDefault(0), workbookPart) ?? "").Trim();
                importRow.KullaniciAdi = (GetCellValue(cells.ElementAtOrDefault(1), workbookPart) ?? "").Trim();
                importRow.Email = (GetCellValue(cells.ElementAtOrDefault(2), workbookPart) ?? "").Trim();
                importRow.RolStr = (GetCellValue(cells.ElementAtOrDefault(3), workbookPart) ?? "").Trim();

                if (string.IsNullOrWhiteSpace(importRow.Ad)
                    && string.IsNullOrWhiteSpace(importRow.KullaniciAdi)
                    && string.IsNullOrWhiteSpace(importRow.Email)
                    && string.IsNullOrWhiteSpace(importRow.RolStr))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(importRow.Ad)) 
                    importRow.Errors.Add("Ad boş olamaz.");
                
                if (string.IsNullOrWhiteSpace(importRow.KullaniciAdi)) 
                    importRow.Errors.Add("Kullanıcı Adı boş olamaz.");
                else if (!IsValidUsername(importRow.KullaniciAdi))
                    importRow.Errors.Add($"Kullanıcı Adı geçersiz: '{importRow.KullaniciAdi}'. Sadece harf, rakam, nokta ve alt çizgi kullanılabilir.");
                
                if (string.IsNullOrWhiteSpace(importRow.Email)) 
                    importRow.Errors.Add("Email boş olamaz.");
                else if (!IsValidEmail(importRow.Email))
                    importRow.Errors.Add($"Email formatı geçersiz: '{importRow.Email}'");
                
                if (string.IsNullOrWhiteSpace(importRow.RolStr)) 
                    importRow.Errors.Add("Rol boş olamaz.");
                else if (!validRoles.Contains(importRow.RolStr))
                    importRow.Errors.Add($"Rol geçersiz: '{importRow.RolStr}'. Geçerli roller: {string.Join(", ", validRoles)}");

                result.Rows.Add(importRow);
            }

            var dupKullaniciAdi = result.Rows
                .Where(x => !string.IsNullOrWhiteSpace(x.KullaniciAdi) && x.Errors.Count == 0)
                .GroupBy(x => x.KullaniciAdi.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupKullaniciAdi.Count > 0)
                result.Warnings.Add($"Excel içinde mükerrer Kullanıcı Adı: {string.Join(", ", dupKullaniciAdi.Take(10))}{(dupKullaniciAdi.Count > 10 ? " ..." : "")}");

            var dupEmails = result.Rows
                .Where(x => !string.IsNullOrWhiteSpace(x.Email) && x.Errors.Count == 0)
                .GroupBy(x => x.Email.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupEmails.Count > 0)
                result.Warnings.Add($"Excel içinde mükerrer Email: {string.Join(", ", dupEmails.Take(10))}{(dupEmails.Count > 10 ? " ..." : "")}");

            return result;
        }
        catch (Exception ex)
        {
            result.FatalError = $"Dosya işlenirken hata: {ex.Message}";
            return result;
        }
    }
    
    public static async Task<ImportResult> DoImportAsync(
        PerformansDbContext db,
        IPasswordHasher<Kullanici> hasher,
        List<ImportRow> rows)
    {
        var result = new ImportResult();
        
        var tumKullanicilar = await db.Kullanicilar.ToListAsync();
        const int batchSize = 50;
        var batch = new List<Kullanici>();

        foreach (var row in rows.Where(r => r.Errors.Count == 0))
        {
            var kullaniciAdiKey = row.KullaniciAdi.Trim();
            var emailKey = row.Email.Trim();
            
            var existing = tumKullanicilar.FirstOrDefault(k => 
                k.KullaniciAdi.Equals(kullaniciAdiKey, StringComparison.OrdinalIgnoreCase) ||
                k.Email.Equals(emailKey, StringComparison.OrdinalIgnoreCase));
            
            if (existing != null)
            {
                existing.AdSoyad = row.Ad;
                existing.KullaniciAdi = row.KullaniciAdi;
                existing.Email = row.Email;
                
                if (Enum.TryParse<Rol>(row.RolStr, ignoreCase: true, out var newRol))
                {
                    existing.Rol = newRol;
                }
                
                result.Guncellenen++;
            }
            else
            {
                var varsayilanSifre = "Sifre123!";
                var yeniKullanici = new Kullanici
                {
                    AdSoyad = row.Ad,
                    Email = row.Email,
                    KullaniciAdi = row.KullaniciAdi,
                    Rol = Enum.Parse<Rol>(row.RolStr, ignoreCase: true),
                    SifreHash = hasher.HashPassword(null!, varsayilanSifre),
                    FailedLoginCount = 0,
                    LockoutEnd = null
                };
                
                batch.Add(yeniKullanici);
                tumKullanicilar.Add(yeniKullanici);
                result.Eklenen++;
                
                result.YeniKullanicilar.Add(new KullaniciInfo
                {
                    Ad = yeniKullanici.AdSoyad,
                    KullaniciAdi = yeniKullanici.KullaniciAdi,
                    Email = yeniKullanici.Email,
                    Rol = yeniKullanici.Rol.ToString(),
                    VarsayilanSifre = varsayilanSifre
                });

                if (batch.Count >= batchSize)
                {
                    db.Kullanicilar.AddRange(batch);
                    await db.SaveChangesAsync();
                    batch.Clear();
                }
            }
        }

        if (batch.Count > 0)
        {
            db.Kullanicilar.AddRange(batch);
            await db.SaveChangesAsync();
        }

        if (result.Guncellenen > 0)
        {
            await db.SaveChangesAsync();
        }

        return result;
    }
    
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_');
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
