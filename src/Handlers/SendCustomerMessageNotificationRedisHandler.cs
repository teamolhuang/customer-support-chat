using MediatR;
using Microsoft.Extensions.Options;
using src.Commands;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Contexts.SharedContexts.Abstracts;
using src.Utilities;

namespace src.Handlers;

/// <summary>
/// 一般使用者傳送訊息後，把訊息快取到 Redis
/// </summary>
public class SendCustomerMessageNotificationRedisHandler : INotificationHandler<SendCustomerMessageNotification>
{
    private readonly ISharedAuthorizedContext _sharedAuthorizedContext;
    private readonly IOptions<AppSettings> _appSettings;
    private IRedisContext RedisContext { get; }

    /// <summary>
    /// 一般使用者傳送訊息後，把訊息快取到 Redis
    /// </summary>
    public SendCustomerMessageNotificationRedisHandler(IRedisContext redisContext,
        ISharedAuthorizedContext sharedAuthorizedContext,
        IOptions<AppSettings> appSettings)
    {
        _sharedAuthorizedContext = sharedAuthorizedContext;
        _appSettings = appSettings;
        RedisContext = redisContext;
    }

    /// <inheritdoc />
    public async Task Handle(SendCustomerMessageNotification request, CancellationToken cancellationToken)
    {
        ChatMessageRedis newMessage = new()
        {
            Content = request.Message,
            CreatedTime = request.CreatedTime
        };

        int accountId = _sharedAuthorizedContext.AccountId;
        string chatMessageSetKey = IRedisContext.GetChatMessageKey(accountId);
        await RedisContext.AddToListAsync(chatMessageSetKey, newMessage);
        await RedisContext.ExpireAsync(chatMessageSetKey, TimeSpan.FromHours(_appSettings.Value.Redis.ChatMessageExpireHours));
    }
}