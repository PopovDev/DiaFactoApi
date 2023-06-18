using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DiaFactoApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppConfig _appConfig;
    private readonly ILogger<AuthController> _logger;
    private readonly DiaFactoDbContext _diaFactoDbContext;
    private readonly ExternalAuthKeyHolder _externalAuthKeyHolder;

    public AuthController(IOptions<AppConfig> jwtSettings, ILogger<AuthController> logger,
        DiaFactoDbContext diaFactoDbContext, ExternalAuthKeyHolder externalAuthKeyHolder)
    {
        _logger = logger;
        _diaFactoDbContext = diaFactoDbContext;
        _externalAuthKeyHolder = externalAuthKeyHolder;
        _appConfig = jwtSettings.Value;
    }

    private async Task<(Student, LoginRequestMode)?> GetStudentInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        var student = await _diaFactoDbContext.Students.FindAsync(int.Parse(userId));
        if (student is null) return null;
        var passwordHash = User.FindFirstValue(ClaimTypes.Hash);
        if (string.IsNullOrEmpty(passwordHash)) return null;
        var hashOfPassword = SHA256.HashData(Encoding.UTF8.GetBytes(student.Password));
        if (Convert.ToBase64String(hashOfPassword) != passwordHash) return null;
        var loginRequestMode = User.FindFirstValue("loginMode");
        if (string.IsNullOrEmpty(loginRequestMode)) return null;
        var loginRequestModeEnum = Enum.Parse<LoginRequestMode>(loginRequestMode);
        return (student, loginRequestModeEnum);
    }

    private string GenerateToken(Student student, LoginRequestMode loginRequestMode)
    {
        var hashOfPassword = SHA256.HashData(Encoding.UTF8.GetBytes(student.Password));

        var claims = new List<Claim>
        {
            new(ClaimTypes.GroupSid, student.GroupId.ToString(), ClaimValueTypes.Integer32),
            new(ClaimTypes.NameIdentifier, student.Id.ToString(), ClaimValueTypes.Integer32),
            new(ClaimTypes.Role, "user"),
            new(ClaimTypes.Name, student.Name),
            new(ClaimTypes.GivenName, student.ShortName),
            new(ClaimTypes.Hash, Convert.ToBase64String(hashOfPassword)),
            new("loginMode", loginRequestMode.ToString(), ClaimValueTypes.String)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appConfig.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _appConfig.Issuer,
            _appConfig.Audience,
            claims,
            expires: DateTime.UtcNow.Add(_appConfig.TokenLifetime),
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<Results<Ok<LoginResponseModel>, UnauthorizedHttpResult, BadRequest>> Login(
        [FromBody] LoginRequestModel loginRequestModel)
    {
        var user = await _diaFactoDbContext.Students.FindAsync(loginRequestModel.UserId);
        if (user is null || user.GroupId != loginRequestModel.GroupId)
            return TypedResults.BadRequest();

        switch (loginRequestModel.LoginMode)
        {
            case LoginRequestMode.Web:
                if (!BCrypt.Net.BCrypt.Verify(loginRequestModel.Password, user.Password))
                    return TypedResults.Unauthorized();
                break;
            case LoginRequestMode.ExternalKey:
                if (!_externalAuthKeyHolder.TryTakeUser(loginRequestModel.Password, out var userId) ||
                    userId != user.Id)
                    return TypedResults.Unauthorized();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loginRequestModel));
        }

        var tokenString = GenerateToken(user, loginRequestModel.LoginMode);
        var expirationDate = DateTimeOffset.UtcNow.Add(_appConfig.TokenLifetime);
        switch (loginRequestModel.LoginMode)
        {
            case LoginRequestMode.Web:
                var cookieOptions = new CookieOptions { Expires = expirationDate };
                Response.Cookies.Append(_appConfig.CookieName, tokenString, cookieOptions);
                tokenString = "set";
                break;
            case LoginRequestMode.ExternalKey:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loginRequestModel));
        }

        return TypedResults.Ok(new LoginResponseModel
        {
            Token = tokenString,
            ExpiresAt = expirationDate,
            UserId = user.Id,
            GroupId = user.GroupId,
            LoginMode = loginRequestModel.LoginMode
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public Ok Logout()
    {
        Response.Cookies.Delete(_appConfig.CookieName);
        _logger.LogInformation("User {UserId} logged out", User.FindFirstValue(ClaimTypes.NameIdentifier));
        return TypedResults.Ok();
    }

    [Authorize]
    [HttpPost("extendSession")]
    public async Task<Results<Ok<LoginResponseModel>, BadRequest>> ExtendSession()
    {
        var studentInfo = await GetStudentInfo();
        if (!studentInfo.HasValue) return TypedResults.BadRequest();
        var (student, loginRequestMode) = studentInfo.Value;

        var tokenString = GenerateToken(student, loginRequestMode);
        var expirationDate = DateTimeOffset.UtcNow.Add(_appConfig.TokenLifetime);
        switch (loginRequestMode)
        {
            case LoginRequestMode.Web:
                var cookieOptions = new CookieOptions { Expires = expirationDate };
                Response.Cookies.Append(_appConfig.CookieName, tokenString, cookieOptions);
                tokenString = "set";
                break;
            case LoginRequestMode.ExternalKey:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loginRequestMode));
        }

        return TypedResults.Ok(new LoginResponseModel
        {
            Token = tokenString,
            ExpiresAt = expirationDate,
            UserId = student.Id,
            GroupId = student.GroupId,
            LoginMode = loginRequestMode
        });
    }

    [Authorize]
    [HttpPost("changePassword")]
    public async Task<Results<Ok<LoginResponseModel>, BadRequest, UnauthorizedHttpResult>> ChangePassword(
        [FromBody] ChangePasswordRequestModel changePasswordRequestModel)
    {
        var studentInfo = await GetStudentInfo();
        if (!studentInfo.HasValue) return TypedResults.BadRequest();
        var (user, loginMode) = studentInfo.Value;

        if (!BCrypt.Net.BCrypt.Verify(changePasswordRequestModel.OldPassword, user.Password))
        {
            _logger.LogInformation("User [{UserId}] is not authorized to change password", user.Id);
            return TypedResults.Unauthorized();
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordRequestModel.NewPassword);
        await _diaFactoDbContext.SaveChangesAsync();
        var tokenString = GenerateToken(user, loginMode);
        var expirationDate = DateTimeOffset.UtcNow.Add(_appConfig.TokenLifetime);
        switch (loginMode)
        {
            case LoginRequestMode.Web:
                var cookieOptions = new CookieOptions { Expires = expirationDate };
                Response.Cookies.Append(_appConfig.CookieName, tokenString, cookieOptions);
                tokenString = "set";
                break;
            case LoginRequestMode.ExternalKey:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(changePasswordRequestModel));
        }

        return TypedResults.Ok(new LoginResponseModel
        {
            Token = tokenString,
            ExpiresAt = expirationDate,
            UserId = user.Id,
            GroupId = user.GroupId,
            LoginMode = loginMode
        });
    }

    [Authorize]
    [HttpPost("generateExternalKey")]
    public async Task<Results<Ok<LoginGenerateExternalKeyResponse>, BadRequest>> GenerateExternalKey()
    {
        var studentInfo = await GetStudentInfo();
        if (!studentInfo.HasValue) return TypedResults.BadRequest();
        var (user, loginMode) = studentInfo.Value;
        if (loginMode != LoginRequestMode.Web) return TypedResults.BadRequest();
        var (key, expires) = _externalAuthKeyHolder.GenerateKey(user.Id);
        return TypedResults.Ok(new LoginGenerateExternalKeyResponse { Key = key, Expires = expires });
    }
}