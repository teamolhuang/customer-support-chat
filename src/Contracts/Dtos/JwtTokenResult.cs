namespace src.Contracts.Dtos;

/// <summary>
/// 產生 JWT Access Token 時用於內部溝通的 DTO。 
/// </summary>
public class JwtTokenResult
{
    /// <summary>
    /// JWT Token
    /// </summary>
    public string AccessToken { get; set; } = null!;
    
    /// <summary>
    /// 過期時間
    /// </summary>
    public DateTime Expiration { get; set; }
}