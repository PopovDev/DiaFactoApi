using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Group;

[Serializable]
public record ChangeGroupInfoRequest
{
    [JsonPropertyName("info")] public required string Info { get; init; }
}