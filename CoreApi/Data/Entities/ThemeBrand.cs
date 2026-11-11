namespace CoreApi.Data.Entities;

/// <summary>
/// 프로젝트별 현재 브랜드 테마 (public.theme_brand)
/// </summary>
public class ThemeBrand
{
    public string ProjectNum { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string PrimaryColor { get; set; } = "#2563EB";
    public string SecondaryColor { get; set; } = "#3B82F6";
    public string SurfaceColor { get; set; } = "#EFF6FF";
    public bool IsDefault { get; set; } = false;
    public string? ExtractedPalette { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

