using System.Text.Json;
using CoreApi.Common;
using CoreApi.Services;

namespace CoreApi.Infrastructure;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ErrorCodeService errService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception caught by middleware on Path={Path}", context.Request.Path);

            context.Response.StatusCode = 200; // Always HTTP 200
            context.Response.ContentType = "application/json";

            var errorCode = "500";
            var errText = await errService.GetErrorTextAsync(errorCode, "서버 내부 오류가 발생하였습니다.");

            var response = ApiResponse.Error(errorCode, errText);
            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
