using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Subject;

[Serializable]
public record CreateSubjectRequest
{
    [MinLength(3)]
    [MaxLength(64)]
    [JsonPropertyName("name")] public required string Name { get; init; }
    [MinLength(3)]
    [MaxLength(64)]
    [JsonPropertyName("teacher")] public required string Teacher { get; init; }
    [JsonPropertyName("info")] public required string Info { get; init; }
}