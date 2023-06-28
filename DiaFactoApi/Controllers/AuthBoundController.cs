using System.Security.Claims;
using DiaFactoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiaFactoApi.Controllers;

public abstract class AuthBoundController : ControllerBase
{
    private int MyGroupId => int.Parse(User.FindFirstValue(ClaimTypes.GroupSid) ?? "-1");
    private int MyUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1");

    protected async Task<Group?> GetMyGroup(DiaFactoDbContext db)
    {
        if (MyGroupId <= 0)
            return null;
        var group = await db.Groups
            .Include(g => g.Subjects)
            .ThenInclude(s => s.SubjectTimes)
            .FirstOrDefaultAsync(g => g.Id == MyGroupId);
        
        return group;
    }
    
    protected async Task<Student?> GetMyStudent(DiaFactoDbContext db)
    {
        if (MyUserId <= 0)
            return null;
        var student = await db.Students.FindAsync(MyUserId);
        return student;
    }
}