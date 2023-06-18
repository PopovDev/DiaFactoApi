using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Models;

[Serializable]
[Table("Students")]
public class Student
{
    [Key] public int Id { get; set; }
    public required int GroupId { get; set; }
    [JsonIgnore] public Group Group { get; set; } = null!;
    
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public string Info { get; set; } = "";
    public required string Password { get; set; }
}