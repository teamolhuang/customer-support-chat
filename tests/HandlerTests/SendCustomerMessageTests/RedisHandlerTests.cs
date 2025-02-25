using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contexts.Redis.Abstracts;
using src.Contexts.Redis.Entities;
using src.Handlers;

namespace tests.HandlerTests.SendCustomerMessageTests;

[TestFixture]
[Description("針對 SendCustomerMessageCommandDbHandler 的一系列測試方法。")]
public class RedisHandlerTests
{
    [Test]
    [Description("驗證 Handler 應接受包含訊息內容的 SendCustomerMessageCommand 物件，並把這筆留言寫進 Redis。")]
    public async Task Handle_ShouldAcceptSendCustomerMessageCommand_AndInsertMessageIntoRedis()
    {
        // Arrange
        SendCustomerMessageCommand command = new()
        {
            Message = Guid.NewGuid().ToString(),
            CreatedTime = DateTime.Now
        };
        
        AutoMocker autoMocker = new();
        ICollection<ChatMessageRedis> captures = [];
        
        Mock<IRedisContext> mockedRedis = autoMocker.GetMock<IRedisContext>();
        mockedRedis.Setup(redis =>
            redis.AddToListAsync(IRedisContext.ChatMessageKey, Capture.In(captures)));
        mockedRedis.Setup(redis =>
            redis.ExpireAsync(IRedisContext.ChatMessageKey, TimeSpan.FromDays(1)));
        
        SendCustomerMessageCommandRedisHandler handler = autoMocker.CreateInstance<SendCustomerMessageCommandRedisHandler>();

        // Act
        await handler.Handle(command, default);

        // Assert
        Assert.That(captures, Has.Count.EqualTo(1));

        ChatMessageRedis capture = captures.Single();
        
        Assert.That(capture, Is.InstanceOf<ChatMessageRedis>());

        ChatMessageRedis chatMessageInserted = captures.Single();
        Assert.That(chatMessageInserted.Content, Is.EqualTo(command.Message));
        Assert.That(chatMessageInserted.CreatedTime, Is.EqualTo(command.CreatedTime));
        
        autoMocker.VerifyAll();
    }
}