using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CoreApi.Data;

namespace CoreApi.Services;

public class ErrorCodeService
{
    private readonly PostgresHoDbContext _hoDb;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ErrorCodeService> _logger;

    public ErrorCodeService(PostgresHoDbContext hoDb, IMemoryCache cache, ILogger<ErrorCodeService> logger)
    {
        _hoDb = hoDb;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetErrorTextAsync(string errorCode, string? fallbackMessage = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
            return fallbackMessage ?? "오류가 발생했습니다.";

        var cacheKey = $"ERR_TEXT_{errorCode.Trim()}";
        if (_cache.TryGetValue(cacheKey, out string? cachedText) && !string.IsNullOrWhiteSpace(cachedText))
        {
            return cachedText;
        }

        try
        {
            var entity = await _hoDb.ProErrorCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ErrorCode == errorCode.Trim(), ct);

            if (entity != null && !string.IsNullOrWhiteSpace(entity.ErrorText))
            {
                var text = entity.ErrorText.Trim();
                _cache.Set(cacheKey, text, TimeSpan.FromMinutes(10));
                return text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query pro_error_code for code: {Code}", errorCode);
        }

        var defaultFallback = fallbackMessage ?? $"요청 처리 중 오류가 발생하였습니다. (Code: {errorCode})";
        _cache.Set(cacheKey, defaultFallback, TimeSpan.FromMinutes(1));
        return defaultFallback;
    }
}
