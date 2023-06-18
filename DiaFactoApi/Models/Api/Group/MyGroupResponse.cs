using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Group;

[Serializable]
public record MyGroupResponse
{
    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("info")] public required string Info { get; set; }

    [JsonPropertyName("students")] public required MyStudent[] Students { get; set; }
    
    [Serializable]
    public record MyStudent
    {
        [JsonPropertyName("id")] public required int Id { get; init; }
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("shortName")] public required string ShortName { get; init; }
        [JsonPropertyName("info")] public string Info { get; init; } = "";
    }
    
    public static MyGroupResponse FromGroup(Models.Group group) =>
        new()
        {
            Name = group.Name,
            Info = group.Info,
            Students = group.Students.Select(s => new MyStudent
            {
                Id = s.Id,
                Name = s.Name,
                ShortName = s.ShortName,
                Info = s.Info,
            }).ToArray(),
        };
}