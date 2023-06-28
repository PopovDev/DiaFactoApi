namespace DiaFactoApi.Models.Api.SubjectTime;

public static class SubjectTimeMappers
{
    public static Models.SubjectTime ToSubjectTime(this CreateSubjectTimeRequest request) =>
        new()
        {
            Id = 0,
            SubjectId = request.SubjectId,
            DayNumber = request.DayNumber,
            TimeStart = request.TimeStart,
            TimeEnd = request.TimeEnd,
            WeekType = request.WeekType
        };
    
    public static SubjectTimeDisplay ToSubjectTimeDisplay(this Models.SubjectTime subjectTime) =>
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