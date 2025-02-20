using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;

namespace src.Contexts.Redis.Abstracts;

/// <summary>
/// 存取 Redis 用的類型。
/// </summary>
public interface IRedisContext
{
    /// <summary>
    /// 聊天訊息用的 key。
    /// </summary>
    public const string ChatMessageKey = "chat-message";

    /// <summary>
    /// 將物件放入指定的 List。
    /// </summary>
    Task AddToListAsync<T>(string chatMessageKey, T input);

    /// <summary>
    /// 指定某個 Key 的有效時長。
    /// </summary>
    Task ExpireAsync(string chatMessageKey, TimeSpan timeSpan);
}