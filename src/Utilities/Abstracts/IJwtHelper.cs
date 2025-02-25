using Microsoft.AspNetCore.Authentication.JwtBearer;
using src.Contexts.Database.Entities;
using src.Contracts.Dtos;

namespace src.Utilities.Abstracts;

/// <summary>
/// JWT Token 生成相關的處理工具類型。
/// </summary>
public interface IJwtHelper
{
    /// <summary>
    /// 建立並回傳 JWT AccessToken 與其有效時間。
    /// </summary>
    /// <param name="accountId">帳號 ID，<see cref="Account.Id"/></param>
    Task<JwtTokenResult> CreateTokenAsync(int accountId);

    /// <summary>
    /// 驗證使用者 ID 存在於 JWT claims 裡面。
    /// </summary>
    Task<bool> ValidateUserIdInClaimAsync(TokenValidatedContext context);

    /// <summary>
    /// 將使用者 ID 寫在 SharedAuthorizedContext 中，以便各層類型取用。
    /// </summary>
    Task WriteClaimsInSharedContextAsync(TokenValidatedContext context);
}