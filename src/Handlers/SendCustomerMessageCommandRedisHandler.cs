using MediatR;
using src.Commands;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;

namespace src.Handlers;

/// <summary>
/// 一般使用者傳送訊息後，把訊息快取到 Redis
/// </summary>
public class SendCustomerMessageCommandRedisHandler : IRequestHandler<SendCustomerMessageCommand>
{
    private IRedisContext RedisContext { get; }

    /// <summary>
    /// 一般使用者傳送訊息後，把訊息快取到 Redis
    /// </summary>
    public SendCustomerMessageCommandRedisHandler(IRedisContext redisContext)
    {
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
        
        await RedisContext.AddToListAsync(IRedisContext.ChatMessageKey, newMessage);
        await RedisContext.ExpireAsync(IRedisContext.ChatMessageKey, TimeSpan.FromDays(1));
    }
}