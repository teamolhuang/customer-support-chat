using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contexts.SharedContexts.Abstracts;
using src.Handlers;

namespace tests.HandlerTests.SendCustomerMessageTests
{
    [TestFixture]
    [Description("針對 SendCustomerMessageNotificationDbHandler 的一系列測試方法。")]
    public class DbHandlerTests
    {
        [Test]
        [Description("驗證 Handler 應接受包含訊息內容的 SendCustomerMessageNotification 物件，並把這筆留言連同使用者 ID 寫進 DB。")]
        public async Task Handle_ShouldAcceptSendCustomerMessageCommand_AndInsertMessageInDatabase(){
            
            // Arrange
            Random random = new();
            int mockAccountId = random.Next();
            
            SendCustomerMessageNotification notification = new() {
                Message = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now
            };

            Mock<DatabaseContext> mockContext = new();

            ICollection<ChatMessage> captures = [];

            mockContext.Setup(ctx => ctx.AddAsync(Capture.In(captures), default))
                       .Verifiable(Times.Once);

            mockContext.Setup(ctx => ctx.SaveChangesAsync(default))
                       .Verifiable(Times.Once);

            AutoMocker autoMocker = new();            
            autoMocker.Use(mockContext);

            autoMocker.GetMock<ISharedAuthorizedContext>()
                .SetupGet(ctx => ctx.AccountId)
                .Returns(mockAccountId)
                .Verifiable(Times.Once);
            
            SendCustomerMessageNotificationDbHandler instance = autoMocker.CreateInstance<SendCustomerMessageNotificationDbHandler>();

            // Act
            await instance.Handle(notification, default);

            // Assert
            ChatMessage writtenEntity = captures.Single();
            Assert.That(writtenEntity.AccountId, Is.EqualTo(mockAccountId));
            Assert.That(writtenEntity.Content, Is.EqualTo(notification.Message));
            Assert.That(writtenEntity.CreatedTime, Is.GreaterThanOrEqualTo(notification.CreatedTime));
            Assert.That(writtenEntity.IsFromUser, Is.True); // 因為是客戶傳訊息的 Command，所以一定是 true。

            autoMocker.Verify();
        }
    }
}