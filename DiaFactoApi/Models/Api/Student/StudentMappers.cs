namespace DiaFactoApi.Models.Api.Student;

public static class StudentMappers
{
    public static StudentDisplay ToStudentDisplay(this Models.Student student) =>
        new()
        {
            Id = student.Id,
            Name = student.Name,
            ShortName = student.ShortName,
            Info = student.Info,
            HasAdminRights = student.HasAdminRights
        };
}