using System.Text.Json.Serialization;

namespace CoreApi.Common;

/// <summary>
/// HRC 표준 API Return JSON 응답 모델 (HTTP 200 기반)
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; } = 200;

    [JsonPropertyName("success")]
    public string Success { get; set; } = "777";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "성공적으로 처리되었습니다.";

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "성공적으로 처리되었습니다.")
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Success = "777",
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> Error(string errorCode, string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Code = 200,
            Success = errorCode,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "성공적으로 처리되었습니다.")
    {
        return new ApiResponse
        {
            Code = 200,
            Success = "777",
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse Error(string errorCode, string message)
    {
        return new ApiResponse
        {
            Code = 200,
            Success = errorCode,
            Message = message,
            Data = null,
            Timestamp = DateTime.UtcNow
        };
    }
}
