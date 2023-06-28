using System.Net.Mime;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.Subject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/subject")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class SubjectController : AuthBoundController
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
        var myGroup = await GetMyGroup(_db);
        return myGroup?.Subjects.FirstOrDefault(s => s.Id == id);
    }


    [HttpPost("create")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(SubjectDisplay), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Created<SubjectDisplay>, BadRequest>> CreateSubject(CreateSubjectRequest request)
    {
        var myGroup = await GetMyGroup(_db);
        var student = await GetMyStudent(_db);
        if (myGroup is null || student is null || !student.HasAdminRights)
            return TypedResults.BadRequest();
     
        var subject = request.ToSubject(myGroup);
        await _db.Subjects.AddAsync(subject);
        await _db.SaveChangesAsync();
        _logger.LogInformation("[Subject] [Create] [{SubjectId}] {SubjectName}", subject.Id, subject.Name);
        var display = subject.ToSubjectDisplay();
        return TypedResults.Created($"subject/{subject.Id}", display);
    }

    [HttpGet]
    [ProducesResponseType(typeof(SubjectDisplay[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<SubjectDisplay[]>, UnauthorizedHttpResult>> GetSubjects()
    {
        var myGroup = await GetMyGroup(_db);
        if (myGroup is null)
            return TypedResults.Unauthorized();

        var subjects = myGroup.Subjects.Select(x => x.ToSubjectDisplay()).ToArray();
        return TypedResults.Ok(subjects);
    }

    [HttpGet("{subjectId:int}")]
    [ProducesResponseType(typeof(SubjectDisplay), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<SubjectDisplay>, UnauthorizedHttpResult>> GetSubject(int subjectId)
    {
        var subject = await GetSubjectById(subjectId);
        if (subject is null)
            return TypedResults.Unauthorized();
        
        return TypedResults.Ok(subject.ToSubjectDisplay());
    }

    [HttpDelete("{subjectId:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<Results<NoContent, BadRequest>> DeleteSubject(int subjectId)
    {
        var subject = await GetSubjectById(subjectId);
        var student = await GetMyStudent(_db);
        if (subject is null || student is null || !student.HasAdminRights)
            return TypedResults.BadRequest();
        
        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();
        _logger.LogInformation("[Subject] [Delete] [{SubjectId}] {SubjectName}", subject.Id, subject.Name);
        return TypedResults.NoContent();
    }

    [HttpPut("{subjectId:int}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
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
        _logger.LogInformation("[Subject] [Change] [{SubjectId}] {SubjectName}", subject.Id, subject.Name);
        return TypedResults.NoContent();
    }
}