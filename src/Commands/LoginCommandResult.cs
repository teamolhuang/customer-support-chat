namespace src.Commands;

/// <summary>
/// 使用者登入後的處理結果
/// </summary>
public class LoginCommandResult
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