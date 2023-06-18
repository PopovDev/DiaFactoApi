using System.Text;
using DiaFactoApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var dbConnectionString = configuration.GetConnectionString("DefaultConnection");

builder.Services.AddOptions<AppConfig>()
    .BindConfiguration(AppConfig.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
var appConfig = configuration.GetSection(AppConfig.SectionName).Get<AppConfig>() ??
                throw new Exception("AppConfig is not configured");


builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = appConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = appConfig.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.Secret)),
            ValidateIssuerSigningKey = true,
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSingleton<ExternalAuthKeyHolder>();
builder.Services.AddDbContext<DiaFactoDbContext>(options => options.UseNpgsql(dbConnectionString));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsPolicyBuilder =>
    {
        corsPolicyBuilder
            .WithOrigins(appConfig.AllowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});


var app = builder.Build();

app.UseCors();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});
app.UseMiddleware<CookieAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<DiaFactoDbContext>();
context.Database.Migrate();

app.Run();