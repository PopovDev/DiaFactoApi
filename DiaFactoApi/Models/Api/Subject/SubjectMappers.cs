namespace DiaFactoApi.Models.Api.Subject;

public static class SubjectMappers
{
    public static SubjectDisplay ToSubjectDisplay(this Models.Subject subject) =>
        new()
        {
            Id = subject.Id,
            Name = subject.Name,
            Teacher = subject.Teacher,
            Info = subject.Info
        };
    
    public static Models.Subject ToSubject(this CreateSubjectRequest request, Models.Group group) =>
        new()
        {
            Id = 0,
            Name = request.Name,
            Teacher = request.Teacher,
            Info = request.Info,
            GroupId = group.Id,
            Group = group
        };
}