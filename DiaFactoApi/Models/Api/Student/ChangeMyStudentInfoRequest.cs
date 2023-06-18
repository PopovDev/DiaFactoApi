using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Student;

[Serializable]
public record ChangeMyStudentInfoRequest
{
    [JsonPropertyName("shortName")] public string? ShortName { get; init; }
    [JsonPropertyName("info")] public string? Info { get; init; }
}