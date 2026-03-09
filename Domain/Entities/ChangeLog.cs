using System.ComponentModel.DataAnnotations;

namespace PerformansSitesi.Domain.Entities;

/// <summary>
/// Site kodunda yapılan değişiklikleri kaydeder
/// </summary>
public class ChangeLog
{
    [Key]
    public int ChangeId { get; set; }

    /// <summary>
    /// Değişiklik zamanı
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Değişikliği yapan kullanıcı
    /// </summary>
    public int? UserId { get; set; }
    public string? UserName { get; set; }

    /// <summary>
    /// Değişiklik kategorisi (UI, Backend, Database, Configuration, vb.)
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// Hangi dosya/modül değişti
    /// </summary>
    public string Module { get; set; } = "";

    /// <summary>
    /// Değişiklik açıklaması
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Önceki değer (opsiyonel)
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Yeni değer (opsiyonel)
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Git commit hash (eğer Git kullanılıyorsa)
    /// </summary>
    public string? CommitHash { get; set; }

    /// <summary>
    /// Değişiklik nedeni
    /// </summary>
    public string? Reason { get; set; }
}
