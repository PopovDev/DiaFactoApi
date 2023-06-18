using System.Security.Claims;
using DiaFactoApi.Models.Api.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Controllers;

[Authorize]
[ApiController]
[Route("/group")]
public class GroupController : ControllerBase
{
    private readonly ILogger<GroupController> _logger;
    private readonly DiaFactoDbContext _db;

    public GroupController(ILogger<GroupController> logger, DiaFactoDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("anon")]
    public async Task<Ok<AnonGroupsResponse>> GetAnonGroups()
    {
        var groups = await _db.Groups
            .Select(g => AnonGroupsResponse.AnonGroup.FromGroup(g))
            .ToArrayAsync();
        
        return TypedResults.Ok(new AnonGroupsResponse { Groups = groups, });
    }

    [HttpGet("my")]
    public async Task<Results<Ok<MyGroupResponse>, BadRequest>> GetMyGroup()
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups.FindAsync(int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.BadRequest();
        
        return TypedResults.Ok(MyGroupResponse.FromGroup(myGroup));
    }
    
    [HttpPut("my/info")]
    public async Task<Results<NoContent, BadRequest>> ChangeGroupInfo(ChangeGroupInfoRequest request)
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups.FindAsync(int.Parse(myGroupId));
        
        if (myGroup is null)
            return TypedResults.BadRequest();
        
        myGroup.Info = request.Info;
        _logger.LogInformation("Group {GroupId} changed info to {Info}", myGroup.Id, request.Info);
        await _db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}