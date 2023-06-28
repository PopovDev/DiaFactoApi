using System.Net.Mime;
using System.Security.Claims;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.Auth;
using DiaFactoApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DiaFactoApi.Controllers;

using AuthOperationResults = Results<Ok<LoginResponseModel>, UnauthorizedHttpResult>;

[ApiController]
[Route("auth")]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
public class AuthController : ControllerBase
{
    private readonly AppConfig _appConfig;
    private readonly ILogger<AuthController> _logger;
    private readonly DiaFactoDbContext _diaFactoDbContext;
    private readonly ExternalAuthKeyService _externalAuthKeyService;
    private readonly TokenService _tokenService;

    public AuthController(ILogger<AuthController> logger,
        DiaFactoDbContext diaFactoDbContext, ExternalAuthKeyService externalAuthKeyService, TokenService tokenService,
        IOptions<AppConfig> appConfig)
    {
        _logger = logger;
        _diaFactoDbContext = diaFactoDbContext;
        _externalAuthKeyService = externalAuthKeyService;
        _tokenService = tokenService;
        _appConfig = appConfig.Value;
    }

    [AllowAnonymous]
    [HttpPost("login/{type:regex(web|external)}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
    public async Task<AuthOperationResults> Login([FromBody] LoginRequestModel loginRequestModel, string type)
    {
        var student = await _diaFactoDbContext.Students.FindAsync(loginRequestModel.UserId);
        if (student is null || student.GroupId != loginRequestModel.GroupId)
            return TypedResults.Unauthorized();

        switch (type)
        {
            case "web":
                if (!_tokenService.TryAuthenticatePassword(student, loginRequestModel.Password))
                    return TypedResults.Unauthorized();
                return TypedResults.Ok(GenerateLoginResponse(student, LoginRequestMode.Web));
            case "external":
                if (!_tokenService.TryAuthenticateExternalKey(student, loginRequestModel.Password))
                    return TypedResults.Unauthorized();
                return TypedResults.Ok(GenerateLoginResponse(student, LoginRequestMode.ExternalKey));
            default:
                return TypedResults.Unauthorized();
        }
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public Ok Logout()
    {
        Response.Cookies.Delete(_appConfig.CookieName);
        _logger.LogInformation("User {UserId} logged out", User.FindFirstValue(ClaimTypes.NameIdentifier));
        return TypedResults.Ok();
    }

    [Authorize]
    [HttpPost("extendSession")]
    [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<AuthOperationResults> ExtendSession()
    {
        var studentInfo = await _tokenService.GetStudentInfo(User);
        if (!studentInfo.HasValue) return TypedResults.Unauthorized();
        var (student, loginRequestMode) = studentInfo.Value;
        return TypedResults.Ok(GenerateLoginResponse(student, loginRequestMode));
    }

    [Authorize]
    [HttpPost("changePassword")]
    [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<AuthOperationResults> ChangePassword([FromBody] ChangePasswordRequestModel changePassRequestModel)
    {
        var studentInfo = await _tokenService.GetStudentInfo(User);
        if (!studentInfo.HasValue) return TypedResults.Unauthorized();
        var (user, loginMode) = studentInfo.Value;

        if (!_tokenService.TryAuthenticatePassword(user, changePassRequestModel.OldPassword))
            return TypedResults.Unauthorized();

        await _tokenService.ChangePassword(user, changePassRequestModel.NewPassword);
        return TypedResults.Ok(GenerateLoginResponse(user, loginMode));
    }

    [Authorize]
    [HttpPost("generateExternalKey")]
    [ProducesResponseType(typeof(LoginGenerateExternalKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    
    public async Task<Results<Ok<LoginGenerateExternalKeyResponse>, UnauthorizedHttpResult>> GenerateExternalKey()
    {
        var studentInfo = await _tokenService.GetStudentInfo(User);
        if (!studentInfo.HasValue)
            return TypedResults.Unauthorized();
        var (user, loginMode) = studentInfo.Value;
        
        if (loginMode != LoginRequestMode.Web)
            return TypedResults.Unauthorized();
        
        var (key, expires) = _externalAuthKeyService.GenerateKey(user.Id);
        return TypedResults.Ok(new LoginGenerateExternalKeyResponse { Key = key, Expires = expires });
    }

    private LoginResponseModel GenerateLoginResponse(Student student, LoginRequestMode loginRequestMode)
    {
        var tokenString = _tokenService.GenerateToken(student, loginRequestMode);
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

        return new LoginResponseModel
        {
            Token = tokenString,
            ExpiresAt = expirationDate,
            UserId = student.Id,
            GroupId = student.GroupId,
            LoginMode = loginRequestMode
        };
    }
}