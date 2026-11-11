namespace CoreApi.Common;

public static class ErrorCodes
{
    public const string Success = "777";
    public const string Unauthorized = "401";
    public const string Forbidden = "403";
    public const string NotFound = "404";
    public const string BadRequest = "400";
    public const string InternalServerError = "500";
    public const string TokenRequired = "AUTH_TOKEN_REQUIRED";
    public const string TokenInvalid = "AUTH_TOKEN_INVALID";
}
