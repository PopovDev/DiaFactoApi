using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DiaFactoApi;

public class CookieAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppConfig _appConfig;
    
    public CookieAuthMiddleware(RequestDelegate next, IOptions<AppConfig> appConfig)
    {
        _next = next;
        _appConfig = appConfig.Value;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(_appConfig.CookieName, out var cookie))
        {
            context.Request.Headers.TryAdd("Authorization", $"Bearer {cookie}");
        }
        context.Response.Headers.Add("X-Xss-Protection", @"1; mode=block");
        context.Response.Headers.Add("X-Frame-Options", @"DENY");
        context.Response.Headers.Add("X-Content-Type-Options", @"nosniff");
        await _next(context);
    }
}