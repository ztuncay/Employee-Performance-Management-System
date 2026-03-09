using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerformansSitesi.Domain.Entities;
using PerformansSitesi.Infrastructure.Data;
using System.Security.Claims;

namespace PerformansSitesi.Controllers;

[Authorize(Roles = "Admin,IK,SistemAdmin")]
public class ReportsController : Controller
{
    private readonly PerformansDbContext _db;

    public ReportsController(PerformansDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var donemler = await _db.Donemler
            .AsNoTracking()
            .OrderByDescending(x => x.AktifMi)
            .ThenByDescending(x => x.BaslangicTarihi)
            .ToListAsync();

        return View(donemler);
    }

    /// <summary>
    /// Detayl� performans raporu - ki�i bilgileri, g�rev ve sorumluluk, yetkinlik format�nda
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportDetailedPerformanceReport(int? donemId)
    {
        var selectedDonemId = await ResolveDonemId(donemId);
        if (selectedDonemId == null)
            return BadRequest("Gecerli bir donem bulunamadi. Lutfen donem olusturun veya gecerli donemId gonderin.");

        var donem = await _db.Donemler
            .AsNoTracking()
            .FirstAsync(d => d.DonemId == selectedDonemId.Value);

        var sorular = await _db.PerformansSorulari
            .AsNoTracking()
            .OrderBy(s => s.SiraNo)
            .ToListAsync();

        var gorevSorular = sorular.Where(s => s.SiraNo >= 1 && s.SiraNo <= 6).ToList();
        var yetkinlikSorular = sorular.Where(s => s.SiraNo >= 7 && s.SiraNo <= 11).ToList();

        var data = await _db.Personeller
            .AsNoTracking()
            .Include(p => p.Yonetici1)
            .Include(p => p.Yonetici2)
            .Include(p => p.NihaiYonetici)
            .Select(p => new
            {
                Personel = p,
                Degerlendirme = _db.Degerlendirmeler
                    .Include(d => d.Detaylar)
                    .FirstOrDefault(d => d.PersonelId == p.PersonelId && d.DonemId == selectedDonemId.Value)
            })
            .OrderBy(x => x.Personel.SicilNo)
            .ToListAsync();

        using var ms = new MemoryStream();
        using (var spreadsheetDocument = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();
            stylesPart.Stylesheet.Save();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Performans Raporu"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            uint rowIndex = 1;

            var donemRow = new Row { RowIndex = rowIndex++ };
            donemRow.Append(CreateStyledCell("D�nem", 2));
            donemRow.Append(CreateStyledCell(donem.Ad, 2));
            sheetData.Append(donemRow);

            rowIndex++;

            var headerRow1 = new Row { RowIndex = rowIndex++ };
            headerRow1.Append(CreateStyledCell("K��� B�LG�LER�", 2));
            for (int i = 0; i < 12; i++) headerRow1.Append(CreateStyledCell("", 2));
            headerRow1.Append(CreateStyledCell("G�REV VE SORUMLULUK DE�ERLEND�RMES�", 2));
            for (int i = 0; i < gorevSorular.Count - 1; i++) headerRow1.Append(CreateStyledCell("", 2));
            headerRow1.Append(CreateStyledCell("YETK�NL�K DE�ERLEND�RMES�", 2));
            for (int i = 0; i < yetkinlikSorular.Count - 1; i++) headerRow1.Append(CreateStyledCell("", 2));
            sheetData.Append(headerRow1);

            var headerRow2 = new Row { RowIndex = rowIndex++ };
            headerRow2.Append(CreateStyledCell("Sicil No", 3));
            headerRow2.Append(CreateStyledCell("Personel Ad� Soyad�", 3));
            headerRow2.Append(CreateStyledCell("G�revi", 3));
            headerRow2.Append(CreateStyledCell("��e Giri� Tarihi", 3));
            headerRow2.Append(CreateStyledCell("Proje Ad�", 3));
            headerRow2.Append(CreateStyledCell("M�d�rl�k", 3));
            headerRow2.Append(CreateStyledCell("Lokasyon", 3));
            headerRow2.Append(CreateStyledCell("1. Y�netici", 3));
            headerRow2.Append(CreateStyledCell("2. Y�netici", 3));
            headerRow2.Append(CreateStyledCell("B�lge M�d�r�", 3));
            headerRow2.Append(CreateStyledCell("Durum", 3));
            headerRow2.Append(CreateStyledCell("Toplam Puan", 3));
            headerRow2.Append(CreateStyledCell("Genel Sonu�", 3));
            foreach (var s in gorevSorular)
                headerRow2.Append(CreateStyledCell(s.SoruBaslik, 3));
            foreach (var s in yetkinlikSorular)
                headerRow2.Append(CreateStyledCell(s.SoruBaslik, 3));
            sheetData.Append(headerRow2);

            foreach (var item in data)
            {
                var p = item.Personel;
                var deg = item.Degerlendirme;

                var dataRow = new Row { RowIndex = rowIndex++ };
                dataRow.Append(CreateStyledCell(p.SicilNo, 1));
                dataRow.Append(CreateStyledCell(p.AdSoyad, 1));
                dataRow.Append(CreateStyledCell(p.Gorev ?? "", 1));
                dataRow.Append(CreateStyledCell(p.IseGirisTarihi?.ToString("dd.MM.yyyy") ?? "", 1));
                dataRow.Append(CreateStyledCell(p.ProjeAdi ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Mudurluk ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Lokasyon ?? p.Mudurluk ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Yonetici1?.AdSoyad ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Yonetici2?.AdSoyad ?? "", 1));
                dataRow.Append(CreateStyledCell(p.NihaiYonetici?.AdSoyad ?? "", 1));
                dataRow.Append(CreateStyledCell(deg?.Durum.ToString() ?? "De�erlendirme Yok", 1));
                dataRow.Append(CreateNumberStyledCell(deg?.ToplamPuan ?? 0, 1));
                dataRow.Append(CreateStyledCell(deg?.GenelSonuc ?? "", 1));

                foreach (var soru in gorevSorular)
                {
                    var detay = deg?.Detaylar?.FirstOrDefault(d => d.SoruId == soru.SoruId);
                    dataRow.Append(CreateNumberStyledCell(detay?.Yonetici1Puan ?? 0, 1));
                }

                foreach (var soru in yetkinlikSorular)
                {
                    var detay = deg?.Detaylar?.FirstOrDefault(d => d.SoruId == soru.SoruId);
                    dataRow.Append(CreateNumberStyledCell(detay?.Yonetici1Puan ?? 0, 1));
                }

                sheetData.Append(dataRow);
            }

            var columns = new Columns();
            uint totalCols = (uint)(13 + gorevSorular.Count + yetkinlikSorular.Count);
            for (uint i = 1; i <= totalCols; i++)
            {
                columns.Append(new Column { Min = i, Max = i, Width = 15, CustomWidth = true });
            }
            worksheetPart.Worksheet.InsertAt(columns, 0);

            worksheetPart.Worksheet.Save();
        }

        var bytes = ms.ToArray();

        await AddAudit("Export", $"Detailed Performance Report: DonemId={selectedDonemId} Rows={data.Count}");

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Performans_Detayli_Rapor_{donem.Ad}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    /// <summary>
    /// �zet performans raporu - temel bilgiler ve sonu�lar
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportDistributionReport(int? donemId)
    {
        var selectedDonemId = await ResolveDonemId(donemId);
        if (selectedDonemId == null)
            return BadRequest("Gecerli bir donem bulunamadi. Lutfen donem olusturun veya gecerli donemId gonderin.");

        var donem = await _db.Donemler
            .AsNoTracking()
            .FirstAsync(d => d.DonemId == selectedDonemId.Value);

        var data = await _db.Personeller
            .AsNoTracking()
            .Include(p => p.Yonetici1)
            .Include(p => p.Yonetici2)
            .Include(p => p.NihaiYonetici)
            .Select(p => new
            {
                Personel = p,
                Degerlendirme = _db.Degerlendirmeler
                            .FirstOrDefault(d => d.PersonelId == p.PersonelId && d.DonemId == selectedDonemId.Value)
            })
            .OrderBy(x => x.Personel.SicilNo)
            .ToListAsync();

        using var ms = new MemoryStream();
        using (var spreadsheetDocument = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheetDocument.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateStylesheet();
            stylesPart.Stylesheet.Save();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
            var sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "�zet Rapor"
            };
            sheets.Append(sheet);

            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>()!;

            uint rowIndex = 1;

            var donemRow = new Row { RowIndex = rowIndex++ };
            donemRow.Append(CreateStyledCell("D�nem", 2));
            donemRow.Append(CreateStyledCell(donem.Ad, 2));
            sheetData.Append(donemRow);

            rowIndex++;

            var headerRow = new Row { RowIndex = rowIndex++ };
            headerRow.Append(CreateStyledCell("Sicil No", 3));
            headerRow.Append(CreateStyledCell("Personel Ad� Soyad�", 3));
            headerRow.Append(CreateStyledCell("G�revi", 3));
            headerRow.Append(CreateStyledCell("��e Giri� Tarihi", 3));
            headerRow.Append(CreateStyledCell("��ten ��k�� Tarihi", 3));
            headerRow.Append(CreateStyledCell("Proje Ad�", 3));
            headerRow.Append(CreateStyledCell("M�d�rl�k", 3));
            headerRow.Append(CreateStyledCell("Lokasyon", 3));
            headerRow.Append(CreateStyledCell("1. Y�netici", 3));
            headerRow.Append(CreateStyledCell("2. Y�netici", 3));
            headerRow.Append(CreateStyledCell("B�lge M�d�r�", 3));
            headerRow.Append(CreateStyledCell("Durum", 3));
            headerRow.Append(CreateStyledCell("Toplam Puan", 3));
            headerRow.Append(CreateStyledCell("Genel Sonu�", 3));
            sheetData.Append(headerRow);

            foreach (var item in data)
            {
                var p = item.Personel;
                var deg = item.Degerlendirme;

                var dataRow = new Row { RowIndex = rowIndex++ };
                dataRow.Append(CreateStyledCell(p.SicilNo, 1));
                dataRow.Append(CreateStyledCell(p.AdSoyad, 1));
                dataRow.Append(CreateStyledCell(p.Gorev ?? "", 1));
                dataRow.Append(CreateStyledCell(p.IseGirisTarihi?.ToString("dd.MM.yyyy") ?? "", 1));
                dataRow.Append(CreateStyledCell(p.IstenCikisTarihi?.ToString("dd.MM.yyyy") ?? "", 1));
                dataRow.Append(CreateStyledCell(p.ProjeAdi ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Mudurluk ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Lokasyon ?? p.Mudurluk ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Yonetici1?.AdSoyad ?? "", 1));
                dataRow.Append(CreateStyledCell(p.Yonetici2?.AdSoyad ?? "", 1));
                dataRow.Append(CreateStyledCell(p.NihaiYonetici?.AdSoyad ?? "", 1));
                
                string durumText = deg == null ? "De�erlendirme Yok" : deg.Durum.ToString().Replace("Asamasi", "A�amas�");
                dataRow.Append(CreateStyledCell(durumText, 1));
                dataRow.Append(CreateNumberStyledCell(deg?.ToplamPuan ?? 0, 1));
                dataRow.Append(CreateStyledCell(deg?.GenelSonuc ?? "", 1));

                sheetData.Append(dataRow);
            }

            var columns = new Columns();
            for (uint i = 1; i <= 14; i++)
            {
                columns.Append(new Column { Min = i, Max = i, Width = 15, CustomWidth = true });
            }
            worksheetPart.Worksheet.InsertAt(columns, 0);

            worksheetPart.Worksheet.Save();
        }

        var bytes = ms.ToArray();

        await AddAudit("Export", $"Summary Performance Report: DonemId={selectedDonemId} Rows={data.Count}");

        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Ozet_Performans_Raporu_{donem.Ad}_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private async Task<int?> ResolveDonemId(int? donemId)
    {
        if (donemId.HasValue)
        {
            if (donemId.Value <= 0)
                return null;

            var exists = await _db.Donemler.AsNoTracking()
                .AnyAsync(d => d.DonemId == donemId.Value);

            return exists ? donemId.Value : null;
        }

        return await _db.Donemler
            .OrderByDescending(x => x.AktifMi)
            .ThenByDescending(x => x.BaslangicTarihi)
            .Select(x => (int?)x.DonemId)
            .FirstOrDefaultAsync();
    }

    private static Stylesheet CreateStylesheet()
    {
        var stylesheet = new Stylesheet();
        var fonts = new Fonts();
        fonts.Append(new Font());
        fonts.Append(new Font(new Bold(), new FontSize { Val = 11 }));
        fonts.Append(new Font(new Bold(), new FontSize { Val = 12 }, new Color { Rgb = "FFFFFFFF" }));
        fonts.Count = (uint)fonts.ChildElements.Count;

        var fills = new Fills();
        fills.Append(new Fill(new PatternFill { PatternType = PatternValues.None }));
        fills.Append(new Fill(new PatternFill { PatternType = PatternValues.Gray125 }));
        fills.Append(new Fill(new PatternFill(new ForegroundColor { Rgb = "FFE31E24" }) { PatternType = PatternValues.Solid }));
        fills.Append(new Fill(new PatternFill(new ForegroundColor { Rgb = "FFF0F0F0" }) { PatternType = PatternValues.Solid }));
        fills.Count = (uint)fills.ChildElements.Count;

        var borders = new Borders();
        borders.Append(new Border());
        borders.Append(new Border(
            new LeftBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new RightBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new TopBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin },
            new BottomBorder(new Color { Auto = true }) { Style = BorderStyleValues.Thin }
        ));
        borders.Count = (uint)borders.ChildElements.Count;

        var cellFormats = new CellFormats();
        cellFormats.Append(new CellFormat());
        cellFormats.Append(new CellFormat { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true });
        cellFormats.Append(new CellFormat { FontId = 2, FillId = 2, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center } });
        cellFormats.Append(new CellFormat { FontId = 1, FillId = 3, BorderId = 1, ApplyFont = true, ApplyFill = true, ApplyBorder = true, Alignment = new Alignment { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center } });
        cellFormats.Count = (uint)cellFormats.ChildElements.Count;

        stylesheet.Fonts = fonts;
        stylesheet.Fills = fills;
        stylesheet.Borders = borders;
        stylesheet.CellFormats = cellFormats;
        return stylesheet;
    }

    private static Cell CreateStyledCell(string text, uint styleIndex) =>
        new() { DataType = CellValues.InlineString, InlineString = new InlineString(new Text(text ?? "")), StyleIndex = styleIndex };

    private static Cell CreateNumberStyledCell(int? number, uint styleIndex) =>
        new() { DataType = CellValues.Number, CellValue = new CellValue((number ?? 0).ToString()), StyleIndex = styleIndex };

    private static Cell CreateTextCell(string text) =>
        new() { DataType = CellValues.InlineString, InlineString = new InlineString(new Text(text ?? "")) };

    private static Cell CreateNumberCell(double number) =>
        new() { DataType = CellValues.Number, CellValue = new CellValue(number.ToString()) };

    private async Task AddAudit(string eventType, string note)
    {
        try
        {
            int? userId = null;
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idStr, out var id)) userId = id;

            _db.AuditLogs.Add(new AuditLog
            {
                EventType = eventType,
                UserId = userId,
                UserName = User?.Identity?.Name ?? "Anonymous",
                UserRole = User.FindFirstValue(ClaimTypes.Role) ?? "Anonymous",
                Method = string.IsNullOrWhiteSpace(HttpContext.Request.Method) ? "UNKNOWN" : HttpContext.Request.Method.ToUpperInvariant(),
                Path = string.IsNullOrWhiteSpace(HttpContext.Request.Path) ? "/" : HttpContext.Request.Path,
                QueryString = HttpContext.Request.QueryString.HasValue ? HttpContext.Request.QueryString.Value : string.Empty,
                IpAddress = string.IsNullOrWhiteSpace(HttpContext.Connection.RemoteIpAddress?.ToString()) ? "Unknown" : HttpContext.Connection.RemoteIpAddress!.ToString(),
                UserAgent = string.IsNullOrWhiteSpace(HttpContext.Request.Headers.UserAgent) ? "Unknown" : HttpContext.Request.Headers.UserAgent.ToString(),
                Note = note,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }
        catch { }
    }
}
