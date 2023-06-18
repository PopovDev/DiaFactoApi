using System.Security.Claims;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.SubjectTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/subjectTime")]
public class SubjectTimeController : ControllerBase
{
    private readonly ILogger<SubjectTimeController> _logger;
    private readonly DiaFactoDbContext _db;

    public SubjectTimeController(ILogger<SubjectTimeController> logger, DiaFactoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet]
    public async Task<Results<Ok<SubjectTimeDisplay[]>,BadRequest>> GetSubjectTimes()
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups
            .Include(g => g.Subjects)
            .ThenInclude(s => s.SubjectTimes)
            .FirstOrDefaultAsync(g => g.Id == int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.BadRequest();
        
        var subjectTimes = myGroup.Subjects
            .SelectMany(s => s.SubjectTimes)
            .Select(SubjectTimeDisplay.FromSubjectTime)
            .ToArray();

        return TypedResults.Ok(subjectTimes);
    }
    
    [HttpPost("create")]
    public async Task<Results<Created<SubjectTimeDisplay>, BadRequest>> CreateSubjectTime(CreateSubjectTimeRequest request)
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups
            .Include(g => g.Subjects)
            .ThenInclude(s => s.SubjectTimes)
            .FirstOrDefaultAsync(g => g.Id == int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.BadRequest();
        
        var mySubject = myGroup.Subjects.FirstOrDefault(s => s.Id == request.SubjectId);
        if (mySubject is null)
            return TypedResults.BadRequest();
        
        var subjectTime = new SubjectTime
        {
            Id = 0,
            SubjectId = request.SubjectId,
            DayNumber = request.DayNumber,
            TimeStart = request.TimeStart,
            TimeEnd = request.TimeEnd,
            WeekType = request.WeekType
        };
        await _db.SubjectTimes.AddAsync(subjectTime);
        await _db.SaveChangesAsync();
        var display = SubjectTimeDisplay.FromSubjectTime(subjectTime);
        return TypedResults.Created($@"subjectTime/{subjectTime.Id}", display);
    }
}