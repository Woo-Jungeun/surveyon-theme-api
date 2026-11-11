using System.Text.Json.Serialization;

namespace CoreApi.Common;

/// <summary>
/// HRC 표준 API Return JSON 응답 모델 (HTTP 200 기반)
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public string Success { get; set; } = "777";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "성공적으로 처리되었습니다.";

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "성공적으로 처리되었습니다.")
    {
        return new ApiResponse<T>
        {
            Success = "777",
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Error(string errorCode, string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = errorCode,
            Message = message,
            Data = data
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "성공적으로 처리되었습니다.")
    {
        return new ApiResponse
        {
            Success = "777",
            Message = message,
            Data = null
        };
    }

    public static ApiResponse Error(string errorCode, string message)
    {
        return new ApiResponse
        {
            Success = errorCode,
            Message = message,
            Data = null
        };
    }
}
