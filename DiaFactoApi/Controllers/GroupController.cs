using System.Net.Mime;
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
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
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
    [ProducesResponseType(typeof(AnonGroupsResponse), StatusCodes.Status200OK)]
    public async Task<Ok<AnonGroupsResponse>> GetAnonGroups()
    {
        var groups = await _db.Groups
            .Select(g => AnonGroupsResponse.AnonGroup.FromGroup(g))
            .ToArrayAsync();
        
        return TypedResults.Ok(new AnonGroupsResponse { Groups = groups, });
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(MyGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<Ok<MyGroupResponse>, UnauthorizedHttpResult>> GetMyGroup()
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups.FindAsync(int.Parse(myGroupId));
        if (myGroup is null)
            return TypedResults.Unauthorized();
        
        return TypedResults.Ok(MyGroupResponse.FromGroup(myGroup));
    }
    
    [HttpPut("my/info")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<Results<NoContent, UnauthorizedHttpResult>> ChangeGroupInfo(ChangeGroupInfoRequest request)
    {
        var myGroupId = User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1";
        var myGroup = await _db.Groups.FindAsync(int.Parse(myGroupId));
        
        if (myGroup is null)
            return TypedResults.Unauthorized();
        
        myGroup.Info = request.Info;
        _logger.LogInformation("Group {GroupId} changed info to {Info}", myGroup.Id, request.Info);
        await _db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}