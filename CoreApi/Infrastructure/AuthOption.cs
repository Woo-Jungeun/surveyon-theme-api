namespace CoreApi.Infrastructure;

public class AuthOptions
{
    public string AuthCoreBaseUrl { get; set; } = "";
    public string ValidatePath { get; set; } = "login/token/validate";
    public string HeaderName { get; set; } = "X-Auth-Token";
    public int CacheSeconds { get; set; } = 30;
    public bool DisableAuth { get; set; } = false;
    public string? InternalKey { get; set; }
    public string InternalKeyHeaderName { get; set; } = "X-Internal-Key";
}

public class ValidateRequest
{
    public string user { get; set; } = "";
}

public class AuthValidationResultJson
{
    public bool valid { get; set; }
    public string userId { get; set; } = "";
}

public class AuthValidationResponse
{
    public string success { get; set; } = "";
    public string message { get; set; } = "";
    public AuthValidationResultJson? resultjson { get; set; }
}
