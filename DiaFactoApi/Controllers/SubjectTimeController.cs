using System.Net.Mime;
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
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class SubjectTimeController : AuthBoundController
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
        var myGroup = await GetMyGroup(_db);
        if (myGroup is null)
            return TypedResults.BadRequest();
        
        var subjectTimes = myGroup.Subjects
            .SelectMany(s => s.SubjectTimes)
            .Select(st => st.ToSubjectTimeDisplay())
            .ToArray();

        return TypedResults.Ok(subjectTimes);
    }
    
    [HttpPost("create")]
    public async Task<Results<Created<SubjectTimeDisplay>, BadRequest>> CreateSubjectTime(CreateSubjectTimeRequest request)
    {
        var myGroup = await GetMyGroup(_db);
        var mySubject = myGroup?.Subjects.FirstOrDefault(s => s.Id == request.SubjectId);

        if (mySubject is null)
            return TypedResults.BadRequest();
        
        var subjectTime = request.ToSubjectTime();
        await _db.SubjectTimes.AddAsync(subjectTime);
        await _db.SaveChangesAsync();
        _logger.LogInformation("[SubjectTime] [Create] {SubjectTimeId}", subjectTime.Id);
        var display = subjectTime.ToSubjectTimeDisplay();
        return TypedResults.Created($@"subjectTime/{subjectTime.Id}", display);
    }
}