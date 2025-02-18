using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Handlers;

namespace tests.HandlerTests
{
    [TestFixture]
    [Description("針對 SendCustomerMessageCommandDbHandler 的一系列測試方法。")]
    public class SendMessageTests
    {
        [Test]
        [Description("驗證 Handler 應接受包含訊息內容的 SendCustomerMessageCommand 物件，並把這筆留言寫進 DB。")]
        public async Task SendMessageAsync_ShouldAcceptSendCustomerMessageCommand_AndInsertMessageInDatabase(){
            
            // Arrange
            SendCustomerMessageCommand command = new() {
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

            SendCustomerMessageCommandDbHandler instance = autoMocker.CreateInstance<SendCustomerMessageCommandDbHandler>();

            // Act
            await instance.Handle(command, default);

            // Assert
            ChatMessage writtenEntity = captures.Single();
            Assert.That(writtenEntity.Content, Is.EqualTo(command.Message));
            Assert.That(writtenEntity.CreatedTime, Is.GreaterThanOrEqualTo(command.CreatedTime));

            autoMocker.Verify();
        }

    }
}