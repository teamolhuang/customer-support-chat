namespace src.Utilities;

/// <summary>
/// 系統參數設定的抽象化類別。
/// </summary>
/// <remarks>裡面的參數必須是 virtual，讓單元測試能順利 mock。</remarks>
public class AppSettings
{
    /// <summary>
    /// JWT 相關的設定。
    /// </summary>
    public virtual JwtSettings Jwt { get; set; } = new();

    /// <summary>
    /// Redis 相關的設定。
    /// </summary>
    public virtual RedisSettings Redis { get; set; } = new();
}

/// <summary>
/// Redis 相關的參數設定。
/// </summary>
public class RedisSettings
{
    /// <summary>
    /// 使用者聊天訊息表的快取有效時間長度（小時數）。
    /// </summary>
    public int ChatMessageExpireHours { get; set; }
}

/// <summary>
/// JWT 相關的參數設定。
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 密鑰 
    /// </summary>
    public string SecretKey { get; set; } = null!;
    
    /// <summary>
    /// 過期小時數
    /// </summary>
    public int ExpirationHour { get; set; }
}