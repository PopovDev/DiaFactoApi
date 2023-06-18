using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Auth;

[Serializable]
public record LoginResponseModel
{
    [JsonPropertyName("token")] public required string Token { get; init; }
    [JsonPropertyName("expiresAt")] public required DateTimeOffset ExpiresAt { get; init; }
    [JsonPropertyName("userId")] public required int UserId { get; init; }
    [JsonPropertyName("groupId")] public required int GroupId { get; init; }
    [Range(0, 1)]
    [JsonPropertyName("loginMode")] public required LoginRequestMode LoginMode { get; init; }
}