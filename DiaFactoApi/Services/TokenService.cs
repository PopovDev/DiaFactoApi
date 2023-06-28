using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DiaFactoApi.Models;
using DiaFactoApi.Models.Api.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DiaFactoApi.Services;

public class TokenService
{
    private readonly AppConfig _appConfig;
    private readonly ILogger<TokenService> _logger;
    private readonly DiaFactoDbContext _diaFactoDbContext;
    private readonly ExternalAuthKeyService _externalAuthKeyService;

    public TokenService(IOptions<AppConfig> appConfig, ILogger<TokenService> logger,
        DiaFactoDbContext diaFactoDbContext, ExternalAuthKeyService externalAuthKeyService)
    {
        _logger = logger;
        _diaFactoDbContext = diaFactoDbContext;
        _externalAuthKeyService = externalAuthKeyService;
        _appConfig = appConfig.Value;
    }

    public async Task<(Student, LoginRequestMode)?> GetStudentInfo(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        var student = await _diaFactoDbContext.Students.FindAsync(int.Parse(userId));
        if (student is null) return null;
        var passwordHash = user.FindFirstValue(ClaimTypes.Hash);
        if (string.IsNullOrEmpty(passwordHash)) return null;
        var hashOfPassword = SHA256.HashData(Encoding.UTF8.GetBytes(student.Password));
        if (Convert.ToBase64String(hashOfPassword) != passwordHash) return null;
        var loginRequestMode = user.FindFirstValue("loginMode");
        if (string.IsNullOrEmpty(loginRequestMode)) return null;
        var loginRequestModeEnum = Enum.Parse<LoginRequestMode>(loginRequestMode);
        return (student, loginRequestModeEnum);
    }

    private static string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public string GenerateToken(Student student, LoginRequestMode loginRequestMode)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.GroupSid, student.GroupId.ToString(), ClaimValueTypes.Integer32),
            new(ClaimTypes.NameIdentifier, student.Id.ToString(), ClaimValueTypes.Integer32),
            new(ClaimTypes.Role, student.HasAdminRights ? "admin" : "student"),
            new(ClaimTypes.Name, student.Name),
            new(ClaimTypes.GivenName, student.ShortName),
            new(ClaimTypes.Hash, HashPassword(student.Password)),
            new("loginMode", loginRequestMode.ToString(), ClaimValueTypes.String),
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


    public bool TryAuthenticatePassword(Student student, string password)
    {
        _logger.LogInformation("Trying to authenticate user {UserId} with password", student.Id);
        return BCrypt.Net.BCrypt.Verify(password, student.Password);
    }

    public bool TryAuthenticateExternalKey(Student student, string externalKey)
    {
        _logger.LogInformation("Trying to authenticate user {UserId} with external key", student.Id);
        return _externalAuthKeyService.TryTakeUser(externalKey, student.Id);
    }
    
    public async Task ChangePassword(Student student, string newPassword)
    {
        _logger.LogInformation("Changing password for user {UserId}", student.Id);
        student.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _diaFactoDbContext.Students.Update(student);
        await _diaFactoDbContext.SaveChangesAsync();
    }
}