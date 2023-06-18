using System.Text.Json.Serialization;

namespace DiaFactoApi.Models.Api.SubjectTime;

[Serializable]
public class SubjectTimeDisplay
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("subjectId")] public int SubjectId { get; set; }
    [JsonPropertyName("dayNumber")] public int DayNumber { get; set; }
    [JsonPropertyName("timeStart")] public TimeSpan TimeStart { get; set; }
    [JsonPropertyName("timeEnd")] public TimeSpan TimeEnd { get; set; }
    [JsonPropertyName("weekType")] public WeekType WeekType { get; set; }
    
    public static SubjectTimeDisplay FromSubjectTime(Models.SubjectTime subjectTime) =>
        new()
        {
            Id = subjectTime.Id,
            SubjectId = subjectTime.SubjectId,
            DayNumber = subjectTime.DayNumber,
            TimeStart = subjectTime.TimeStart,
            TimeEnd = subjectTime.TimeEnd,
            WeekType = subjectTime.WeekType
        };
}