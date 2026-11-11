using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using CoreApi.Common;
using CoreApi.Services;

namespace CoreApi.Infrastructure;

public class AuthCheckFilter : IAsyncAuthorizationFilter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly AuthOptions _opt;
    private readonly ErrorCodeService _errService;
    private readonly ILogger<AuthCheckFilter> _logger;

    public AuthCheckFilter(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IOptions<AuthOptions> opt,
        ErrorCodeService errService,
        ILogger<AuthCheckFilter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _opt = opt.Value;
        _errService = errService;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (HttpMethods.IsOptions(context.HttpContext.Request.Method))
            return;

        var path = context.HttpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/APIs/t/swagger", StringComparison.OrdinalIgnoreCase))
            return;

        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
            return;

        if (_opt.DisableAuth)
            return;

        string token = "";
        if (context.HttpContext.Request.Headers.TryGetValue(_opt.HeaderName, out var extractedToken) && !string.IsNullOrWhiteSpace(extractedToken.ToString()))
        {
            token = extractedToken.ToString().Trim();
        }
        else if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader) && !string.IsNullOrWhiteSpace(authHeader.ToString()))
        {
            var raw = authHeader.ToString().Trim();
            token = raw.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? raw.Substring("Bearer ".Length).Trim() : raw;
        }

        var userId = await ResolveUserIdFromBodyAsync(context);

        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("AuthCheck failed: missing token. Path={Path}", path);
            var errText = await _errService.GetErrorTextAsync("401", "인증 토큰이 필요합니다.");
            context.Result = new JsonResult(ApiResponse.Error("401", errText));
            return;
        }

        var cacheKey = "AUTH_OK_" + token;
        if (_cache.TryGetValue(cacheKey, out string? cachedUserId) && !string.IsNullOrWhiteSpace(cachedUserId))
        {
            context.HttpContext.Items["UserId"] = cachedUserId;
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("AuthCore");
            using var req = new HttpRequestMessage(HttpMethod.Post, _opt.ValidatePath);
            var jsonContent = JsonSerializer.Serialize(new ValidateRequest { user = userId });
            req.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            req.Headers.Add(_opt.HeaderName, token.Trim());
            if (!string.IsNullOrWhiteSpace(_opt.InternalKey))
            {
                req.Headers.Add(_opt.InternalKeyHeaderName, _opt.InternalKey);
            }

            using var res = await client.SendAsync(req, context.HttpContext.RequestAborted);

            if (!res.IsSuccessStatusCode)
            {
                _logger.LogWarning("AuthCheck failed: rejected by AuthCore. Path={Path} Status={StatusCode}", path, res.StatusCode);
                var errText = await _errService.GetErrorTextAsync("401", "유효하지 않거나 만료된 인증 토큰입니다.");
                context.Result = new JsonResult(ApiResponse.Error("401", errText));
                return;
            }

            var resString = await res.Content.ReadAsStringAsync(context.HttpContext.RequestAborted);
            var resModel = JsonSerializer.Deserialize<AuthValidationResponse>(resString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (resModel == null || resModel.success != "777" || resModel.resultjson == null || !resModel.resultjson.valid)
            {
                var errText = await _errService.GetErrorTextAsync("401", "인증 검증 실패: 유효하지 않은 토큰입니다.");
                context.Result = new JsonResult(ApiResponse.Error("401", errText));
                return;
            }

            var resolvedUserId = string.IsNullOrWhiteSpace(resModel.resultjson.userId) ? userId : resModel.resultjson.userId;
            context.HttpContext.Items["UserId"] = resolvedUserId;
            _cache.Set(cacheKey, resolvedUserId, TimeSpan.FromSeconds(Math.Max(1, _opt.CacheSeconds)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuthCheck exception on path={Path}", path);
            var errText = await _errService.GetErrorTextAsync("500", "인증 서비스 연동 중 오류가 발생했습니다.");
            context.Result = new JsonResult(ApiResponse.Error("500", errText));
        }
    }

    private static async Task<string> ResolveUserIdFromBodyAsync(AuthorizationFilterContext context)
    {
        string userId = string.Empty;

        if (HttpMethods.IsPost(context.HttpContext.Request.Method))
        {
            try
            {
                context.HttpContext.Request.EnableBuffering();
                context.HttpContext.Request.Body.Position = 0;
                using var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, false, 1024, true);
                var body = await reader.ReadToEndAsync(context.HttpContext.RequestAborted);
                context.HttpContext.Request.Body.Position = 0;
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                        doc.RootElement.TryGetProperty("user", out var p) &&
                        p.ValueKind == JsonValueKind.String)
                    {
                        userId = p.GetString()?.Trim() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // Ignore JSON parse errors and fallback to X-User-Id header
            }
        }

        if (string.IsNullOrWhiteSpace(userId) &&
            context.HttpContext.Request.Headers.TryGetValue("X-User-Id", out var headerUserId) &&
            !string.IsNullOrWhiteSpace(headerUserId.ToString()))
        {
            userId = headerUserId.ToString().Trim();
        }

        return userId;
    }
}
