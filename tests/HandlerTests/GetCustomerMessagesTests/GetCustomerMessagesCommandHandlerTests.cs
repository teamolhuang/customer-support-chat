using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Moq.EntityFrameworkCore;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Contexts.SharedContexts.Abstracts;
using src.Handlers;
using src.Utilities;

namespace tests.HandlerTests.GetCustomerMessagesTests;

[TestFixture]
[Description("針對處理 GetCustomerMessagesNotification 的處理器的測試。")]
public class GetCustomerMessagesCommandHandlerTests
{
    [Test]
    [Description("驗證此處理應該從 Redis 取得快取訊息，重設這張表的有效時間，然後在依時間新至舊排序之後，回傳這些快取訊息。")]
    public async Task Handle_ShouldQueryFromRedisAndResetExpiration_AndReturnSortedRedisCachedMessages()
    {
        // Arrange
        Random random = new();
        int mockAccountId = random.Next();
        int mockExpireHours = random.Next(1, 100);
        
        // 為了驗證排序，這裡的定義時，必須是依時間正序
        IEnumerable<ChatMessageRedis> mockMessages =
        [
            new()
            {
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddMinutes(-100),
                IsFromUser = true
            },
            new()
            {
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddMinutes(-50),
                IsFromUser = false
            }
        ];
        AutoMocker autoMocker = new();

        autoMocker.GetMock<ISharedAuthorizedContext>()
            .Setup(ctx => ctx.AccountId)
            .Returns(mockAccountId)
            .Verifiable(Times.Once);

        mockMessages = mockMessages.ToArray();

        autoMocker.GetMock<IOptions<AppSettings>>()
            .SetupGet(o => o.Value)
            .Returns(new AppSettings
            {
                Redis = new RedisSettings
                {
                    ChatMessageExpireHours = mockExpireHours
                }
            })
            .Verifiable(Times.Once);
        
        Mock<IRedisContext> mockRedis = autoMocker.GetMock<IRedisContext>();

        string redisKey = IRedisContext.GetChatMessageKey(mockAccountId);

        mockRedis.Setup(r => r.ContainsKeyAsync(redisKey))
            .ReturnsAsync(true)
            .Verifiable(Times.Once);
        
        mockRedis.Setup(r => r.QueryListAsync<ChatMessageRedis>(redisKey))
            .ReturnsAsync(mockMessages)
            .Verifiable(Times.Once);
        
        mockRedis.Setup(r => r.ExpireAsync(IRedisContext.GetChatMessageKey(mockAccountId), TimeSpan.FromHours(mockExpireHours)))
            .Verifiable(Times.Once);
        
        autoMocker.Use(new Mock<DatabaseContext>());
        
        GetCustomerMessagesCommandHandler handler = autoMocker.CreateInstance<GetCustomerMessagesCommandHandler>();
        
        // Act
        GetCustomerMessagesCommandResult result = await handler.Handle(new GetCustomerMessagesCommand(), default);
        
        // Assert
        Assert.That(result.Messages.Count(), Is.EqualTo(mockMessages.Count()));

        IEnumerable<ChatMessageRedis> orderedMockedMessages = mockMessages
            .OrderByDescending(x => x.CreatedTime)
            .ToArray();
        
        // 驗證排序。
        for (int i = 0; i < mockMessages.Count(); i++)
        {
            ChatMessageRedis mockMessage = orderedMockedMessages.ElementAt(i);
            GetCustomerMessagesCommandResultMessage targetResult = result.Messages.ElementAt(i);
            
            Assert.That(mockMessage.Content, Is.EqualTo(targetResult.Content));
            Assert.That(mockMessage.CreatedTime, Is.EqualTo(targetResult.CreatedTime));
            Assert.That(mockMessage.IsFromUser, Is.EqualTo(targetResult.IsFromUser));
        }
        
        autoMocker.Verify();
    }

    [Test]
    [Description("驗證此處理應該在 Redis 沒有 key 時，從 DB 取得資料並重建一次快取後，再返回繼續原本的處理。")]
    public async Task Handle_ShouldBuildRedisCacheFromDbIfKeyNotExists_AndReturnResultsAsUsual()
    {
        // Arrange
        Random random = new();
        int mockAccountId = random.Next();

        ChatMessage[] mockChatMessageData =
        [
            new()
            {
                Id = Guid.NewGuid(),
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddHours(-random.Next(1, 100)),
                AccountId = mockAccountId,
                IsFromUser = true,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddHours(-random.Next(1, 100)),
                AccountId = mockAccountId,
                IsFromUser = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddHours(-random.Next(1, 100)),
                AccountId = mockAccountId - 1,
                IsFromUser = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now.AddHours(-random.Next(1, 100)),
                AccountId = mockAccountId - 1,
                IsFromUser = false
            }
        ];
        
        AutoMocker autoMocker = new();

        autoMocker.GetMock<ISharedAuthorizedContext>()
            .Setup(ctx => ctx.AccountId)
            .Returns(mockAccountId)
            .Verifiable(Times.AtLeastOnce);
        
        autoMocker.GetMock<IOptions<AppSettings>>()
            .Setup(s => s.Value)
            .Returns(new AppSettings
            {
                Redis = new RedisSettings
                {
                    ChatMessageExpireHours = random.Next(1, 100)
                }
            })
            .Verifiable(Times.Once);
        
        Mock<IRedisContext> mockRedis = autoMocker.GetMock<IRedisContext>();

        string key = IRedisContext.GetChatMessageKey(mockAccountId);

        mockRedis.Setup(r => r.ContainsKeyAsync(key))
            .ReturnsAsync(false)
            .Verifiable(Times.Once);
        
        mockRedis.Setup(r => r.QueryListAsync<ChatMessageRedis>(key))
            .ReturnsAsync([])
            .Verifiable(Times.Never);
        
        Mock<DatabaseContext> mockDb = new();

        mockDb.Setup(db => db.ChatMessages)
            .ReturnsDbSet(mockChatMessageData)
            .Verifiable(Times.Once);
        
        autoMocker.Use(mockDb);

        ICollection<IEnumerable<ChatMessageRedis>> captures = [];

        mockRedis.Setup(r => r.BatchAddToListAsync(key, Capture.In(captures)))
            .Verifiable(Times.Once);
        
        GetCustomerMessagesCommandHandler handler = autoMocker.CreateInstance<GetCustomerMessagesCommandHandler>();
        
        // Act

        GetCustomerMessagesCommandResult results = await handler.Handle(new GetCustomerMessagesCommand(), default);
        
        // Assert
        
        // 驗證寫入 Redis 的資料。
        Assert.That(captures, Has.Count.EqualTo(1));
        IEnumerable<ChatMessageRedis> writtenToRedis = captures.Single();

        writtenToRedis = writtenToRedis.ToArray();
        
        Assert.That(writtenToRedis.Count(), Is.EqualTo(mockChatMessageData.Count(cm => cm.AccountId == mockAccountId)));
        
        foreach (ChatMessageRedis entity in writtenToRedis)
        {
            Assert.That(mockChatMessageData.Any(d => d.Content == entity.Content
                                                     && d.CreatedTime == entity.CreatedTime
                                                     && d.IsFromUser == entity.IsFromUser), Is.True);
        }
        
        // 驗證回傳。
        // 基於單元測試只測試單項目標的原則，這裡不驗證排序，而是測試該回傳訊息內容確實都存在即可。
        Assert.That(results.Messages.Count(), Is.EqualTo(mockChatMessageData.Count(cm => cm.AccountId == mockAccountId)));
        
        foreach (GetCustomerMessagesCommandResultMessage resultMsg in results.Messages)
        {
            Assert.That(mockChatMessageData.Any(d => d.Content == resultMsg.Content
                                                     && d.CreatedTime == resultMsg.CreatedTime
                                                     && d.IsFromUser == resultMsg.IsFromUser), Is.True);
        }

        autoMocker.Verify();
    }
}