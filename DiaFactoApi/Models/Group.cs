using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Models;

[Serializable]
[Table("Groups")]
[Index(nameof(Name), IsUnique = true)]
public class Group
{
    [Key] public int Id { get; set; }

    public required string Name { get; set; }

    public required string Info { get; set; }

    [JsonIgnore] public List<Subject> Subjects { get; set; } = new();
    [JsonIgnore] public List<Student> Students { get; set; } = new();
}