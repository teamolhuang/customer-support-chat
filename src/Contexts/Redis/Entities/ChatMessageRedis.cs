namespace src.Contexts.Redis.Entities;

/// <summary>
/// 快取在 Redis 的聊天訊息的物件格式。
/// </summary>
public class ChatMessageRedis
{
    /// <summary>
    /// 訊息內容
    /// </summary>
    public string Content { get; set; } = null!;
    
    /// <summary>
    /// 訊息建立時間
    /// </summary>
    public DateTime CreatedTime { get; set; }
}