using src.Contexts.Database.Entities;

namespace src.Contexts.Redis.Abstracts;

/// <summary>
/// 存取 Redis 用的類型。
/// </summary>
public interface IRedisContext
{
    /// <summary>
    /// 聊天訊息用的 key。
    /// </summary>
    private const string ChatMessageKey = "chat-message";

    /// <summary>
    /// 傳入一個使用者的 <see cref="Account.Id"/>>，取得對應這個帳號的聊天室快取表。
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public static string GetChatMessageKey(int accountId)
    {
        return $"{ChatMessageKey}:{accountId}";
    }
    
    /// <summary>
    /// 將物件放入指定的 List。
    /// </summary>
    Task AddToListAsync<T>(string key, T input);

    /// <summary>
    /// 指定某個 Key 的有效時長。
    /// </summary>
    Task ExpireAsync(string key, TimeSpan timeSpan);
}