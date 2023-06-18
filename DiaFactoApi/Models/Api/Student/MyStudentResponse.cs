using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Student;

[Serializable]
public record MyStudentResponse
{
    [JsonPropertyName("id")] public required int Id { get; init; }
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("shortName")] public required string ShortName { get; init; }
    [JsonPropertyName("info")] public required string Info { get; init; }
    
    public static MyStudentResponse FromStudent(Models.Student student) =>
        new()
        {
            Id = student.Id,
            Name = student.Name,
            ShortName = student.ShortName,
            Info = student.Info,
        };
}