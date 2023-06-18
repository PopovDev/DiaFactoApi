using System.ComponentModel.DataAnnotations;

namespace DiaFactoApi;

[Serializable]
public record AppConfig
{
    public const string SectionName = "AppConfig";
    
    [MinLength(16)] public required string Secret { get; init; }

    public required TimeSpan TokenLifetime { get; init; }
    [MinLength(3)] public required string Issuer { get; init; }
    [MinLength(3)] public required string Audience { get; init; }
    public required string CookieName { get; init; }
    public required string AllowedOrigins { get; init; }
}