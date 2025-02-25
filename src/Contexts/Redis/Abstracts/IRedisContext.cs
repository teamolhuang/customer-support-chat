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
    /// 將集合中的所有物件放進指定的 List。
    /// </summary>
    Task BatchAddToListAsync<T>(string key, IEnumerable<T> inputs);

    /// <summary>
    /// 指定某個 Key 的有效時長。
    /// </summary>
    Task ExpireAsync(string key, TimeSpan timeSpan);

    /// <summary>
    /// 查詢字串類型的資料，變形成指定的型態後回傳。
    /// </summary>
    Task<IEnumerable<T>> QueryListAsync<T>(string key);

    /// <summary>
    /// 檢查目前 Redis 是否存在某個 key。
    /// </summary>
    Task<bool> ContainsKeyAsync(string redisKey);
}