namespace CoreApi.Data.Entities;

/// <summary>
/// 브랜드 테마 변경 이력 로그 (public.theme_history)
/// </summary>
public class ThemeHistory
{
    public long HistoryId { get; set; }
    public string ProjectNum { get; set; } = string.Empty;
    public string ActionType { get; set; } = "SAVE";
    public string PrimaryColor { get; set; } = "#2563EB";
    public string SecondaryColor { get; set; } = "#3B82F6";
    public string SurfaceColor { get; set; } = "#EFF6FF";
    public bool IsDefault { get; set; } = false;
    public string? ExtractedPalette { get; set; }
    public string? TableThemeJson { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}

