using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Contexts.SharedContexts.Abstracts;
using src.Utilities;

namespace src.Handlers;

/// <summary>
/// 取得使用者訊息
/// </summary>
public class GetCustomerMessagesCommandHandler : IRequestHandler<GetCustomerMessagesCommand, GetCustomerMessagesCommandResult>
{
    private readonly IRedisContext _redisContext;
    private readonly ISharedAuthorizedContext _sharedAuthorizedContext;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly DatabaseContext _databaseContext;

    /// <summary>
    /// 取得使用者訊息
    /// </summary>
    public GetCustomerMessagesCommandHandler(IRedisContext redisContext,
        ISharedAuthorizedContext sharedAuthorizedContext,
        IOptions<AppSettings> appSettings,
        DatabaseContext databaseContext)
    {
        _redisContext = redisContext;
        _sharedAuthorizedContext = sharedAuthorizedContext;
        _appSettings = appSettings;
        _databaseContext = databaseContext;
    }

    /// <inheritdoc />
    public async Task<GetCustomerMessagesCommandResult> Handle(GetCustomerMessagesCommand request, CancellationToken cancellationToken)
    {
        // 1. 如果 Redis 裡還不存在目前使用者的 key，則到 DB 取得資料後建立快取，並直接利用建立的資料回傳。
        //    如果有 key，才從 redis 取得快取並回傳。
        string redisKey = IRedisContext.GetChatMessageKey(_sharedAuthorizedContext.AccountId);
        bool isInRedis = await _redisContext.ContainsKeyAsync(redisKey);
        
        // 2. 依據上一步的結果，從 DB 取得訊息並建立快取，或是依據使用者 ID，從 Redis 查詢其所有聊天訊息
        IEnumerable<ChatMessageRedis> allData = 
            isInRedis
            ? await _redisContext.QueryListAsync<ChatMessageRedis>(redisKey)
            : await CreateCacheFromDbAsync(redisKey);

        // 3. 從參數設定取得有效小時數，重設聊天訊息表的有效期限
        await _redisContext.ExpireAsync(redisKey, TimeSpan.FromHours(_appSettings.Value.Redis.ChatMessageExpireHours));
        
        // 4. 依新至舊排序，轉換成所需格式後回傳
        return new GetCustomerMessagesCommandResult
        {
            Messages = allData
                .OrderByDescending(cm => cm.CreatedTime)
                .Select(msg => new GetCustomerMessagesCommandResultMessage
                {
                    Content = msg.Content,
                    CreatedTime = msg.CreatedTime,
                    IsFromUser = msg.IsFromUser
                })
        };
    }

    private async Task<IEnumerable<ChatMessageRedis>> CreateCacheFromDbAsync(string redisKey)
    {
        // 1. 先到 DB 查詢目前帳號 ID 的聊天訊息。
        ChatMessage[] messagesFromDb = await _databaseContext.ChatMessages
            .Where(cm => cm.AccountId == _sharedAuthorizedContext.AccountId)
            .ToArrayAsync();
        
        // 2. 把這些訊息轉換成 Redis 的格式並寫入 Redis。
        ChatMessageRedis[] transformed = messagesFromDb
            .Select(m => new ChatMessageRedis
            {
                Content = m.Content,
                CreatedTime = m.CreatedTime,
                IsFromUser = m.IsFromUser
            })
            .ToArray();

        await _redisContext.BatchAddToListAsync(redisKey, transformed);

        // 3. 回傳上一步轉換的資料。
        return transformed;
    }
}