using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.SubjectTime;

[Serializable]
public record CreateSubjectTimeRequest
{
    [JsonPropertyName("subjectId")] public int SubjectId { get; set; }
    [JsonPropertyName("dayNumber")] public int DayNumber { get; set; }
    [JsonPropertyName("timeStart")] public TimeSpan TimeStart { get; set; }
    [JsonPropertyName("timeEnd")] public TimeSpan TimeEnd { get; set; }
    [JsonPropertyName("weekType")] public WeekType WeekType { get; set; }
}