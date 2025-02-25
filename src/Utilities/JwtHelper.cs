using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using src.Contracts.Dtos;
using src.Utilities.Abstracts;

namespace src.Utilities;

/// <inheritdoc />
public class JwtHelper : IJwtHelper
{
    private readonly IOptions<AppSettings> _appSettings;
    
    /// <summary>
    /// 建立實例。
    /// </summary>
    public JwtHelper(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings;
    }

    /// <inheritdoc />
    public async Task<JwtTokenResult> CreateTokenAsync(int accountId)
    {
        // 1. 從 AppSettings 取得密鑰、有效時間長度。
        JwtSettings jwtSettings = _appSettings.Value.Jwt;
        
        // 2. 包裝 Jwt，用到前述的密鑰與時間長度，以及把帳號 ID 包裝在 claims 中。
        JsonWebTokenHandler handler = new();
        
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        SymmetricSecurityKey securityKey = new(keyBytes);
        
        DateTime issuedAt = DateTime.UtcNow;
        DateTime expires = issuedAt.AddHours(jwtSettings.ExpirationHour);
        
        string? token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Expires = expires,
            IssuedAt = issuedAt,
            Claims = new Dictionary<string, object>
            {
                { ClaimTypes.Sid, accountId }

            },
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        });

        // 3. 回傳結果。

        return await Task.FromResult(new JwtTokenResult
        {
            AccessToken = token ?? "",
            Expiration = expires
        });
    }

    /// <inheritdoc />
    public Task<bool> ValidateUserIdInClaimAsync(TokenValidatedContext context)
    {
        return Task.FromResult(context.Principal?.HasClaim(c => c.Type == ClaimTypes.Sid) ?? false);
    }
}