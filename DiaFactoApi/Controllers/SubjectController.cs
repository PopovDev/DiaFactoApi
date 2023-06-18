using System.Security.Claims;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.Subject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/subject")]
public class SubjectController : ControllerBase
{
    private readonly ILogger<SubjectController> _logger;
    private readonly DiaFactoDbContext _db;

    public SubjectController(ILogger<SubjectController> logger, DiaFactoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    private async Task<Subject?> GetSubjectById(int id)
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups
            .Include(g => g.Subjects)
            .FirstOrDefaultAsync(g => g.Id == int.Parse(myGroupId));
        return myGroup?.Subjects.FirstOrDefault(s => s.Id == id);
    }


    [HttpPost("create")]
    public async Task<Results<Created<SubjectDisplay>, BadRequest>> CreateSubject(CreateSubjectRequest request)
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups.FindAsync(int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.BadRequest();

        var subject = new Subject
        {
            Name = request.Name,
            Info = request.Info,
            Group = myGroup,
            GroupId = myGroup.Id,
            Teacher = request.Teacher,
        };
        await _db.Subjects.AddAsync(subject);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Group {GroupId} created subject {SubjectId}", myGroup.Id, subject.Id);
        var display = SubjectDisplay.FromSubject(subject);
        return TypedResults.Created($"subject/{subject.Id}", display);
    }

    [HttpGet]
    public async Task<Ok<SubjectDisplay[]>> GetSubjects()
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups
            .Include(g => g.Subjects)
            .FirstOrDefaultAsync(g => g.Id == int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.Ok(Array.Empty<SubjectDisplay>());

        var subjects = myGroup.Subjects.Select(SubjectDisplay.FromSubject).ToArray();
        return TypedResults.Ok(subjects);
    }

    [HttpGet("{subjectId:int}")]
    public async Task<Results<Ok<SubjectDisplay>, BadRequest>> GetSubject(int subjectId)
    {
        var subject = await GetSubjectById(subjectId);
        if (subject is null)
            return TypedResults.BadRequest();
        
        return TypedResults.Ok(SubjectDisplay.FromSubject(subject));
    }

    [HttpDelete("{subjectId:int}")]
    public async Task<Results<NoContent, BadRequest>> DeleteSubject(int subjectId)
    {
        var subject = await GetSubjectById(subjectId);
        if (subject is null)
            return TypedResults.BadRequest();
        
        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Subject [{SubjectId}] deleted", subject.Id);
        return TypedResults.NoContent();
    }

    [HttpPut("{subjectId:int}")]
    public async Task<Results<NoContent, BadRequest>> ChangeSubject(int subjectId, ChangeSubjectRequest request)
    {
        var subject = await GetSubjectById(subjectId);
        if (subject is null)
            return TypedResults.BadRequest();
        
        if (request.Name is null && request.Info is null && request.Teacher is null)
            return TypedResults.BadRequest();

        if (request.Name is not null)
            subject.Name = request.Name;

        if (request.Info is not null)
            subject.Info = request.Info;

        if (request.Teacher is not null)
            subject.Teacher = request.Teacher;
        
        await _db.SaveChangesAsync();
        _logger.LogInformation("Subject [{SubjectId}] changed", subject.Id);
        return TypedResults.NoContent();
    }
}