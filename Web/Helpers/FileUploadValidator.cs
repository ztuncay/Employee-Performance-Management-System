using System.Drawing;
using Microsoft.AspNetCore.Http;

namespace PerformansSitesi.Web.Helpers;

/// <summary>
/// Dosya upload g�venlik kontrolleri i�in yard�mc� s�n�f
/// </summary>
public static class FileUploadValidator
{
    // �zin verilen dosya uzant�lar� (whitelist)
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    // �zin verilen MIME tipleri
    private static readonly string[] AllowedMimeTypes = 
    { 
        "image/jpeg", 
        "image/jpg", 
        "image/png", 
        "image/gif", 
        "image/webp" 
    };
    
    // Maksimum dosya boyutu (2MB)
    private const long MaxFileSize = 2 * 1024 * 1024; // 2MB in bytes
    
    /// <summary>
    /// Resim dosyas� g�venlik kontrol� yapar
    /// </summary>
    /// <param name="file">Y�klenecek dosya</param>
    /// <param name="errorMessage">Hata mesaj� (��k�� parametresi)</param>
    /// <returns>Ge�erli ise true, de�ilse false</returns>
    public static bool ValidateImageFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        // 1. Null ve boyut kontrol�
        if (file == null || file.Length == 0)
        {
            errorMessage = "Dosya se�ilmedi veya dosya bo�.";
            return false;
        }
        
        // 2. Dosya boyutu kontrol�
        if (file.Length > MaxFileSize)
        {
            errorMessage = $"Dosya boyutu �ok b�y�k. Maksimum {MaxFileSize / 1024 / 1024}MB olabilir.";
            return false;
        }
        
        // 3. Dosya uzant�s� kontrol�
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(fileExtension))
        {
            errorMessage = "Dosya t�r� desteklenmiyor. L�tfen JPG, PNG, GIF veya WebP y�kleyin.";
            return false;
        }
        
        // 4. MIME tipi kontrol�
        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            errorMessage = "Dosya MIME tipi ge�ersiz.";
            return false;
        }
        
        // 5. Dosya i�eri�i kontrol� (Magic bytes)
        try
        {
            using (var stream = file.OpenReadStream())
            {
                if (stream.Length == 0)
                {
                    errorMessage = "Dosya i�eri�i bo�.";
                    return false;
                }
                
                // �lk 4 byte'� oku (magic bytes)
                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                
                // JPEG magic bytes: FF D8 FF
                if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    if (buffer[0] != 0xFF || buffer[1] != 0xD8 || buffer[2] != 0xFF)
                    {
                        errorMessage = "Dosya ger�ekten JPEG de�il.";
                        return false;
                    }
                }
                
                // PNG magic bytes: 89 50 4E 47
                if (fileExtension == ".png")
                {
                    if (buffer[0] != 0x89 || buffer[1] != 0x50 || buffer[2] != 0x4E || buffer[3] != 0x47)
                    {
                        errorMessage = "Dosya ger�ekten PNG de�il.";
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Dosya do�rulan�rken hata olu�tu: {ex.Message}";
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Excel dosyas� do�rulama
    /// </summary>
    public static bool ValidateExcelFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (file == null || file.Length == 0)
        {
            errorMessage = "Dosya se�ilmedi.";
            return false;
        }
        
        // Maksimum dosya boyutu (Excel i�in 10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            errorMessage = "Dosya �ok b�y�k. Maksimum 10MB olabilir.";
            return false;
        }
        
        var fileName = Path.GetFileName(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".xlsx", ".xls" };
        
        if (!allowedExtensions.Any(ext => fileName.EndsWith(ext)))
        {
            errorMessage = "Sadece Excel dosyalar� (.xlsx, .xls) kabul edilir.";
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// CSV dosyas� do�rulama
    /// </summary>
    public static bool ValidateCsvFile(IFormFile file, out string errorMessage)
    {
        errorMessage = string.Empty;
        
        if (file == null || file.Length == 0)
        {
            errorMessage = "Dosya se�ilmedi.";
            return false;
        }
        
        if (file.Length > 5 * 1024 * 1024) // 5MB
        {
            errorMessage = "Dosya �ok b�y�k. Maksimum 5MB olabilir.";
            return false;
        }
        
        var fileName = Path.GetFileName(file.FileName).ToLowerInvariant();
        if (!fileName.EndsWith(".csv"))
        {
            errorMessage = "Sadece CSV dosyalar� kabul edilir.";
            return false;
        }
        
        return true;
    }
}
