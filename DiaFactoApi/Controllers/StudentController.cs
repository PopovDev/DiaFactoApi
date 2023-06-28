using System.Net.Mime;
using System.Security.Claims;
using DiaFactoApi.Models.Api.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/student")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class StudentController : AuthBoundController
{
    private readonly ILogger<StudentController> _logger;
    private readonly DiaFactoDbContext _db;

    public StudentController(ILogger<StudentController> logger, DiaFactoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(StudentDisplay[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<StudentDisplay[]>, UnauthorizedHttpResult>> GetStudents()
    {
        var myGroup = await GetMyGroup(_db);
        if (myGroup is null)
            return TypedResults.Unauthorized();

        var students = myGroup.Students.Select(x => x.ToStudentDisplay()).ToArray();
        return TypedResults.Ok(students);
    }

    [HttpPut("my/info")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<NoContent, UnauthorizedHttpResult>> ChangeStudentInfo(
        ChangeMyStudentInfoRequest request)
    {
        var myStudent = await GetMyStudent(_db);
        if (myStudent is null)
            return TypedResults.Unauthorized();
        
        if (request.Info is not null)
        {
            myStudent.Info = request.Info;
            _logger.LogInformation("Student {StudentId} changed info to {Info}", myStudent.Id, request.Info);
        }

        if (request.ShortName is not null)
        {
            myStudent.ShortName = request.ShortName;
            _logger.LogInformation("Student {StudentId} changed short name to {ShortName}", myStudent.Id,
                request.ShortName);
        }

        await _db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(StudentDisplay), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<StudentDisplay>, UnauthorizedHttpResult>> GetMyStudent()
    {
        var myStudent = await GetMyStudent(_db);
        if (myStudent is null)
            return TypedResults.Unauthorized();

        return TypedResults.Ok(myStudent.ToStudentDisplay());
    }
}