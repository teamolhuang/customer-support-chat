using System.Text.Json;
using src.Contexts.Redis.Abstracts;
using StackExchange.Redis;

namespace src.Contexts.Redis;

/// <inheritdoc />
public class RedisContext : IRedisContext
{
    private ConnectionMultiplexer ConnectionMultiplexer { get; init; }
    
    /// <summary>
    /// 取得實例。
    /// </summary>
    public RedisContext(IConfiguration configuration)
    {
        string connectionString = configuration.GetSection("ConnectionStrings")
            .GetValue<string?>("Redis") ?? throw new Exception("Redis connection string not found");

        ConnectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
    }
    
    private IDatabase Database => ConnectionMultiplexer.GetDatabase();

    /// <inheritdoc />
    public async Task AddToListAsync<T>(string key, T input)
    {
        string serialization = JsonSerializer.Serialize(input);
        await Database.ListRightPushAsync(key, serialization);
    }

    /// <inheritdoc />
    public async Task ExpireAsync(string key, TimeSpan timeSpan)
    {
        await Database.KeyExpireAsync(key, timeSpan);
    }
}