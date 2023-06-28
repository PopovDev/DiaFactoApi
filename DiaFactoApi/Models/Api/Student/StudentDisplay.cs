using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Student;

[Serializable]
public record StudentDisplay
{
    [JsonPropertyName("id")] public required int Id { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("shortName")] public required string ShortName { get; init; }
    [JsonPropertyName("info")] public required string Info { get; init; }
    [JsonPropertyName("hasAdminRights")] public required bool HasAdminRights { get; init; }
}