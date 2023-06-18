using System.ComponentModel.DataAnnotations;

namespace DiaFactoApi.Models;

public class SubjectTime
{
    [Key] public int Id { get; set; }
    public Subject Subject { get; set; } = null!;
    public int SubjectId { get; set; }
    
    public int DayNumber { get; set; }
    public TimeSpan TimeStart { get; set; }
    public TimeSpan TimeEnd { get; set; }
    public WeekType WeekType { get; set; }
}

public enum WeekType
{
    All,
    Nominator,
    Denominator
}