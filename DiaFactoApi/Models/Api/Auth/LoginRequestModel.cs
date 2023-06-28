using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Auth;

[Serializable]
public record LoginRequestModel
{
    [JsonPropertyName("groupId")] public required int GroupId { get; set; }
    [JsonPropertyName("userId")] public required int UserId { get; set; }

    [MinLength(3)]
    [JsonPropertyName("password")]
    public required string Password { get; set; }
}