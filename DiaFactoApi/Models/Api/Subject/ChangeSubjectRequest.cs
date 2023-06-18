using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Subject;

[Serializable]
public record ChangeSubjectRequest
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("teacher")] public string? Teacher { get; init; }
    [JsonPropertyName("info")] public string? Info { get; init; }
}

