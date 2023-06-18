using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DiaFactoApi.Models;

[Serializable]
[Table("Subjects")]
public class Subject
{
    [Key] public int Id { get; set; }
    [JsonIgnore] public Group Group { get; set; } = null!;
    [JsonIgnore] public required int GroupId { get; set; }

    public required string Name { get; set; }
    public string Teacher { get; set; } = "";
    public string Info { get; set; } = "";
    
    [JsonIgnore] public List<SubjectTime> SubjectTimes { get; set; } = null!;
}