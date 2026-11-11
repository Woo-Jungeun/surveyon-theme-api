using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using CoreApi.Data;
using CoreApi.Infrastructure;
using CoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. AppSettings & Options binding
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

// 2. Add DbContexts
var pgConn = builder.Configuration.GetConnectionString("Postgres") 
    ?? "server=10.0.12.95; Port=15300; User Id=jewoo; password=YOUR_PASSWORD; database=HTheme;";
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(pgConn));

var hoConn = builder.Configuration.GetConnectionString("PostgresHO")
    ?? "server=10.0.12.95; Port=15300; User Id=jyjang; password=dv5000037$; database=HOpen;";
builder.Services.AddDbContext<PostgresHoDbContext>(options => options.UseNpgsql(hoConn));

// 3. Add MemoryCache & ErrorCodeService
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ErrorCodeService>();

// 4. Configure HttpClient for AuthCore (stest.hrc.kr for Dev vs s.hrc.kr for Prod)
builder.Services.AddHttpClient("AuthCore", (sp, client) =>
{
    var authOpts = sp.GetRequiredService<IOptions<AuthOptions>>().Value;
    var baseUrl = !string.IsNullOrWhiteSpace(authOpts.AuthCoreBaseUrl)
        ? authOpts.AuthCoreBaseUrl
        : (builder.Environment.IsProduction() ? "https://s.hrc.kr/APIs/o/" : "https://stest.hrc.kr/APIs/o/");
    client.BaseAddress = new Uri(baseUrl);
});

// 5. Add Controllers & AuthCheckFilter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuthCheckFilter>();
});

// 6. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 7. Swagger / OpenAPI Configuration with X-Auth-Token header support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "설문온 NewProject Core API", Version = "v1" });
    c.AddSecurityDefinition("X-Auth-Token", new OpenApiSecurityScheme
    {
        Description = "HRC Auth Token Header (e.g. X-Auth-Token)",
        Name = "X-Auth-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-Auth-Token"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 8. Pipeline configuration
app.UsePathBase("/APIs/t");
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Core API v1"));
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
