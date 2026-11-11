using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CoreApi.Common;
using CoreApi.Data;
using CoreApi.Data.Entities;

namespace CoreApi.Controllers;

/// <summary>
/// H-SRT 고객사 브랜드 테마 설정 API 컨트롤러 (/APIs/t/theme/*)
/// </summary>
[ApiController]
[Route("APIs/t/theme")]
[Route("theme")]
public class ThemeController : ControllerBase
{
    private const string DefaultPrimary = "#2563EB";
    private const string DefaultSecondary = "#3B82F6";
    private const string DefaultSurface = "#EFF6FF";

    private readonly AppDbContext _db;
    private readonly ILogger<ThemeController> _logger;

    public ThemeController(AppDbContext db, ILogger<ThemeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 1. 프로젝트별 브랜드 테마 조회 (POST /APIs/t/theme/get)
    /// </summary>
    [HttpPost("get")]
    public async Task<IActionResult> GetProjectTheme([FromBody] ThemeGetRequest? request, CancellationToken ct)
    {
        var projectNum = request?.Projectnum?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(projectNum))
        {
            return Ok(ApiResponse.Ok(new
            {
                projectnum = string.Empty,
                projectname = string.Empty,
                primary = DefaultPrimary,
                secondary = DefaultSecondary,
                surface = DefaultSurface,
                isDefault = true,
                extractedPalette = Array.Empty<string>()
            }));
        }

        var entity = await _db.ThemeBrands
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjectNum == projectNum, ct);

        if (entity == null)
        {
            return Ok(ApiResponse.Ok(new
            {
                projectnum = projectNum,
                projectname = string.Empty,
                primary = DefaultPrimary,
                secondary = DefaultSecondary,
                surface = DefaultSurface,
                isDefault = true,
                extractedPalette = Array.Empty<string>()
            }));
        }

        return Ok(ApiResponse.Ok(new
        {
            projectnum = entity.ProjectNum,
            projectname = entity.ProjectName ?? string.Empty,
            primary = string.IsNullOrWhiteSpace(entity.PrimaryColor) ? DefaultPrimary : entity.PrimaryColor,
            secondary = string.IsNullOrWhiteSpace(entity.SecondaryColor) ? DefaultSecondary : entity.SecondaryColor,
            surface = string.IsNullOrWhiteSpace(entity.SurfaceColor) ? DefaultSurface : entity.SurfaceColor,
            isDefault = entity.IsDefault,
            extractedPalette = ParsePalette(entity.ExtractedPalette),
            createdBy = entity.CreatedBy,
            updatedBy = entity.UpdatedBy,
            createdAt = entity.CreatedAt,
            updatedAt = entity.UpdatedAt
        }));
    }

    /// <summary>
    /// 2. 프로젝트별 브랜드 테마 저장/초기화 및 이력 기록 (POST /APIs/t/theme/save)
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> SaveProjectTheme([FromBody] ThemeSaveRequest? request, CancellationToken ct)
    {
        var projectNum = request?.Projectnum?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(projectNum))
        {
            return Ok(ApiResponse.Error("400", "프로젝트 번호(projectnum)가 필요합니다."));
        }

        var actorUser = ResolveActorUser(request?.User);
        var isDefault = request?.IsDefault ?? false;

        var primaryColor = isDefault
            ? DefaultPrimary
            : (string.IsNullOrWhiteSpace(request?.Primary) ? DefaultPrimary : request!.Primary!.Trim());

        var secondaryColor = isDefault
            ? DefaultSecondary
            : (string.IsNullOrWhiteSpace(request?.Secondary) ? DefaultSecondary : request!.Secondary!.Trim());

        var surfaceColor = isDefault
            ? DefaultSurface
            : (string.IsNullOrWhiteSpace(request?.Surface) ? DefaultSurface : request!.Surface!.Trim());

        var paletteArray = isDefault
            ? Array.Empty<string>()
            : (request?.ExtractedPalette?
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToArray() ?? Array.Empty<string>());

        var paletteJson = JsonSerializer.Serialize(paletteArray);
        var now = DateTime.UtcNow;

        var entity = await _db.ThemeBrands
            .FirstOrDefaultAsync(x => x.ProjectNum == projectNum, ct);

        if (entity == null)
        {
            entity = new ThemeBrand
            {
                ProjectNum = projectNum,
                ProjectName = string.IsNullOrWhiteSpace(request?.Projectname) ? null : request!.Projectname!.Trim(),
                PrimaryColor = primaryColor,
                SecondaryColor = secondaryColor,
                SurfaceColor = surfaceColor,
                IsDefault = isDefault,
                ExtractedPalette = paletteJson,
                CreatedBy = actorUser,
                UpdatedBy = actorUser,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.ThemeBrands.Add(entity);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request?.Projectname))
            {
                entity.ProjectName = request.Projectname.Trim();
            }
            entity.PrimaryColor = primaryColor;
            entity.SecondaryColor = secondaryColor;
            entity.SurfaceColor = surfaceColor;
            entity.IsDefault = isDefault;
            entity.ExtractedPalette = paletteJson;
            entity.UpdatedBy = actorUser;
            entity.UpdatedAt = now;
        }

        var history = new ThemeHistory
        {
            ProjectNum = projectNum,
            ActionType = isDefault ? "RESET" : "SAVE",
            PrimaryColor = primaryColor,
            SecondaryColor = secondaryColor,
            SurfaceColor = surfaceColor,
            IsDefault = isDefault,
            ExtractedPalette = paletteJson,
            ChangedBy = actorUser,
            ChangedAt = now
        };
        _db.ThemeHistories.Add(history);

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "BrandTheme saved: ProjectNum={ProjectNum}, Action={Action}, IsDefault={IsDefault}, User={User}",
            projectNum, history.ActionType, isDefault, actorUser);

        return Ok(ApiResponse.Ok(new
        {
            projectnum = entity.ProjectNum,
            projectname = entity.ProjectName ?? string.Empty,
            primary = entity.PrimaryColor,
            secondary = entity.SecondaryColor,
            surface = entity.SurfaceColor,
            isDefault = entity.IsDefault,
            extractedPalette = paletteArray,
            updatedBy = entity.UpdatedBy,
            updatedAt = entity.UpdatedAt
        }, isDefault ? "기본 테마로 초기화되었습니다." : "브랜드 테마가 저장되었습니다."));
    }

    /// <summary>
    /// 3. 고객사 브랜드 프리셋 목록 조회 (POST /APIs/t/theme/presets)
    /// </summary>
    [HttpPost("presets")]
    public async Task<IActionResult> GetThemePresets([FromBody] ThemePresetRequest? request, CancellationToken ct)
    {
        var list = await _db.ThemePresets
            .AsNoTracking()
            .Where(x => x.UseYn == "Y")
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.PresetId)
            .ToListAsync(ct);

        var result = list.Select(x => new
        {
            preset_id = x.PresetId,
            presetId = x.PresetId,
            preset_code = x.PresetCode,
            code = x.PresetCode,
            preset_name = x.PresetName,
            name = x.PresetName,
            primary_color = x.PrimaryColor,
            primary = x.PrimaryColor,
            secondary_color = x.SecondaryColor,
            secondary = x.SecondaryColor,
            surface_color = x.SurfaceColor,
            surface = x.SurfaceColor,
            is_default = x.IsDefault,
            isDefault = x.IsDefault,
            is_system = x.IsSystem,
            isSystem = x.IsSystem,
            sort_order = x.SortOrder,
            sortOrder = x.SortOrder
        }).ToList();

        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>
    /// 4. 프로젝트별 브랜드 테마 변경 이력 조회 (POST /APIs/t/theme/history)
    /// </summary>
    [HttpPost("history")]
    public async Task<IActionResult> GetThemeHistory([FromBody] ThemeHistoryRequest? request, CancellationToken ct)
    {
        var projectNum = request?.Projectnum?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(projectNum))
        {
            return Ok(ApiResponse.Ok(Array.Empty<object>()));
        }

        var list = await _db.ThemeHistories
            .AsNoTracking()
            .Where(x => x.ProjectNum == projectNum)
            .OrderByDescending(x => x.ChangedAt)
            .Take(20)
            .ToListAsync(ct);

        var result = list.Select(x => new
        {
            historyId = x.HistoryId,
            history_id = x.HistoryId,
            projectnum = x.ProjectNum,
            project_num = x.ProjectNum,
            actionType = x.ActionType,
            action_type = x.ActionType,
            primary = x.PrimaryColor,
            primary_color = x.PrimaryColor,
            secondary = x.SecondaryColor,
            secondary_color = x.SecondaryColor,
            surface = x.SurfaceColor,
            surface_color = x.SurfaceColor,
            isDefault = x.IsDefault,
            is_default = x.IsDefault,
            extractedPalette = ParsePalette(x.ExtractedPalette),
            changedBy = x.ChangedBy,
            changed_by = x.ChangedBy,
            changedAt = x.ChangedAt,
            changed_at = x.ChangedAt
        }).ToList();

        return Ok(ApiResponse.Ok(result));
    }

    private string ResolveActorUser(string? requestUser)
    {
        if (!string.IsNullOrWhiteSpace(requestUser))
            return requestUser.Trim();

        if (HttpContext.Items.TryGetValue("UserId", out var ctxUser) &&
            ctxUser is string s &&
            !string.IsNullOrWhiteSpace(s))
        {
            return s.Trim();
        }

        if (Request.Headers.TryGetValue("X-User-Id", out var headerUser) &&
            !string.IsNullOrWhiteSpace(headerUser.ToString()))
        {
            return headerUser.ToString().Trim();
        }

        return "system";
    }

    private static string[] ParsePalette(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();

        try
        {
            var arr = JsonSerializer.Deserialize<string[]>(json);
            return arr ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public class ThemeGetRequest
    {
        public string? User { get; set; }
        public string? Projectnum { get; set; }
    }

    public class ThemeSaveRequest
    {
        public string? User { get; set; }
        public string? Projectnum { get; set; }
        public string? Projectname { get; set; }
        public string? Primary { get; set; }
        public string? Secondary { get; set; }
        public string? Surface { get; set; }
        public bool IsDefault { get; set; }
        public List<string>? ExtractedPalette { get; set; }
    }

    public class ThemePresetRequest
    {
        public string? User { get; set; }
    }

    public class ThemeHistoryRequest
    {
        public string? User { get; set; }
        public string? Projectnum { get; set; }
    }
}

