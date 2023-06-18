using System.Security.Claims;
using DiaFactoApi.Models.Api.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/student/my")]
public class StudentController: ControllerBase
{
    private readonly ILogger<StudentController> _logger;
    private readonly DiaFactoDbContext _db;

    public StudentController(ILogger<StudentController> logger, DiaFactoDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    
    [HttpPut("info")]
    public async Task<Results<NoContent, BadRequest>> ChangeStudentInfo(ChangeMyStudentInfoRequest request)
    {
        var myStudentId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";
        var myStudent = await _db.Students.FindAsync(int.Parse(myStudentId));
        if (myStudent is null)
            return TypedResults.BadRequest();
        
        if (request.Info is null && request.ShortName is null)
            return TypedResults.BadRequest();
        if (request.Info is not null)
        {
            myStudent.Info = request.Info;
            _logger.LogInformation("Student {StudentId} changed info to {Info}", myStudent.Id, request.Info);
        }

        if (request.ShortName is not null)
        {
            myStudent.ShortName = request.ShortName;
            _logger.LogInformation("Student {StudentId} changed short name to {ShortName}", myStudent.Id, request.ShortName);
        }
        
        await _db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
    
    [HttpGet("")]
    public async Task<Results<Ok<MyStudentResponse>, BadRequest>> GetMyStudent()
    {
        var myStudentId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1";
        var myStudent = await _db.Students.FindAsync(int.Parse(myStudentId));
        if (myStudent is null)
            return TypedResults.BadRequest();
        
        return TypedResults.Ok(MyStudentResponse.FromStudent(myStudent));
    }
    
}