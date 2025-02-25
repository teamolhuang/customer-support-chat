using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Contexts.SharedContexts.Abstracts;
using src.Handlers;
using src.Utilities;

namespace tests.HandlerTests.SendCustomerMessageTests;

[TestFixture]
[Description("針對 SendCustomerMessageNotificationDbHandler 的一系列測試方法。")]
public class RedisHandlerTests
{
    [Test]
    [Description("驗證 Handler 應接受包含訊息內容的 SendCustomerMessageNotification 物件，並把這筆留言寫進 Redis。")]
    public async Task Handle_ShouldAcceptSendCustomerMessageCommand_AndInsertMessageIntoRedis()
    {
        // Arrange
        Random random = new();
        int mockAccountId = random.Next();
        int mockExpireHours = random.Next(1, 100);
        
        SendCustomerMessageNotification notification = new()
        {
            Message = Guid.NewGuid().ToString(),
            CreatedTime = DateTime.Now
        };
        
        AutoMocker autoMocker = new();
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
        
        ICollection<ChatMessageRedis> captures = [];
        
        Mock<IRedisContext> mockedRedis = autoMocker.GetMock<IRedisContext>();
        mockedRedis.Setup(redis =>
            redis.AddToListAsync(IRedisContext.GetChatMessageKey(mockAccountId), Capture.In(captures)));
        mockedRedis.Setup(redis =>
            redis.ExpireAsync(IRedisContext.GetChatMessageKey(mockAccountId), TimeSpan.FromHours(mockExpireHours)));
        
        autoMocker.GetMock<ISharedAuthorizedContext>()
            .SetupGet(ctx => ctx.AccountId)
            .Returns(mockAccountId)
            .Verifiable(Times.Once);
        
        SendCustomerMessageNotificationRedisHandler handler = autoMocker.CreateInstance<SendCustomerMessageNotificationRedisHandler>();

        // Act
        await handler.Handle(notification, default);

        // Assert
        Assert.That(captures, Has.Count.EqualTo(1));

        ChatMessageRedis capture = captures.Single();
        
        Assert.That(capture, Is.InstanceOf<ChatMessageRedis>());

        ChatMessageRedis chatMessageInserted = captures.Single();
        Assert.That(chatMessageInserted.Content, Is.EqualTo(notification.Message));
        Assert.That(chatMessageInserted.CreatedTime, Is.EqualTo(notification.CreatedTime));
        
        autoMocker.VerifyAll();
    }
}