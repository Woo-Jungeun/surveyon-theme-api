namespace CoreApi.Data.Entities;

/// <summary>
/// 고객사 브랜드 프리셋 목록 (public.theme_preset)
/// </summary>
public class ThemePreset
{
    public int PresetId { get; set; }
    public string PresetCode { get; set; } = string.Empty;
    public string PresetName { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#2563EB";
    public string SecondaryColor { get; set; } = "#3B82F6";
    public string SurfaceColor { get; set; } = "#EFF6FF";
    public bool IsDefault { get; set; } = false;
    public bool IsSystem { get; set; } = false;
    public int SortOrder { get; set; } = 100;
    public string UseYn { get; set; } = "Y";
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

