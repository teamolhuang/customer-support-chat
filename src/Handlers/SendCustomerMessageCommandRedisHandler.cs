using MediatR;
using src.Commands;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Contexts.SharedContexts.Abstracts;

namespace src.Handlers;

/// <summary>
/// 一般使用者傳送訊息後，把訊息快取到 Redis
/// </summary>
public class SendCustomerMessageCommandRedisHandler : IRequestHandler<SendCustomerMessageCommand>
{
    private readonly ISharedAuthorizedContext _sharedAuthorizedContext;
    private IRedisContext RedisContext { get; }

    /// <summary>
    /// 一般使用者傳送訊息後，把訊息快取到 Redis
    /// </summary>
    public SendCustomerMessageCommandRedisHandler(IRedisContext redisContext,
        ISharedAuthorizedContext sharedAuthorizedContext)
    {
        _sharedAuthorizedContext = sharedAuthorizedContext;
        RedisContext = redisContext;
    }

    /// <inheritdoc />
    public async Task Handle(SendCustomerMessageCommand request, CancellationToken cancellationToken)
    {
        ChatMessageRedis newMessage = new()
        {
            Content = request.Message,
            CreatedTime = request.CreatedTime
        };

        int accountId = _sharedAuthorizedContext.AccountId;
        string chatMessageSetKey = IRedisContext.GetChatMessageKey(accountId);
        await RedisContext.AddToListAsync(chatMessageSetKey, newMessage);
        await RedisContext.ExpireAsync(chatMessageSetKey, TimeSpan.FromDays(1));
    }
}