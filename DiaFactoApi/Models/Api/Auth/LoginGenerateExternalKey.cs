using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Auth;

[Serializable]
public record LoginGenerateExternalKeyResponse
{
    [JsonPropertyName("key")] public required string Key { get; init; }
    [JsonPropertyName("expires")] public required DateTimeOffset Expires { get; init; }
}