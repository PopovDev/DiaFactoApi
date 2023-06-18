using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Group;

[Serializable]
public record AnonGroupsResponse
{
    [JsonPropertyName("groups")] public required AnonGroup[] Groups { get; init; }

    [Serializable]
    public record AnonGroup
    {
        [JsonPropertyName("id")] public required int Id { get; init; }
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("students")] public required AnonStudent[] Students { get; init; }
        
        public static AnonGroup FromGroup(Models.Group group) =>
            new()
            {
                Id = group.Id,
                Name = group.Name,
                Students = group.Students.Select(s => new AnonStudent
                {
                    Id = s.Id,
                    ShortName = s.ShortName
                }).ToArray(),
            };
    }

    [Serializable]
    public record AnonStudent
    {
        [JsonPropertyName("id")] public required int Id { get; init; }
        [JsonPropertyName("shortName")] public required string ShortName { get; init; }
    }
}