using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.Subject;

[Serializable]
public record SubjectDisplay
{
    [JsonPropertyName("id")] public required int Id { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("teacher")] public required string Teacher { get; set; }
    [JsonPropertyName("info")] public required string Info { get; set; }

    public static SubjectDisplay FromSubject(Models.Subject subject) =>
        new()
        {
            Id = subject.Id,
            Name = subject.Name,
            Teacher = subject.Teacher,
            Info = subject.Info
        };
}