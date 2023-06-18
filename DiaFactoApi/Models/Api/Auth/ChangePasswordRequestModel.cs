using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Auth;


[Serializable]
public record ChangePasswordRequestModel
{
    [MinLength(3)]
    [JsonPropertyName("oldPassword")] public required string OldPassword { get; set; }
    
    [MinLength(3)]
    [JsonPropertyName("newPassword")] public required string NewPassword { get; set; }
}