using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contracts.Requests.CustomerMessage;
using src.Controllers;

namespace tests.ControllerTests.CustomerMessageTests;

[TestFixture]
[Description("針對 CustomerMessageController 中傳送訊息方法的一系列測試。")]
public class CustomerMessageSendMessageTests
{
    [Test]
    [Description("驗證傳送訊息時，應該將要求物件轉換成 Command 後傳給 Mediator，並回傳 200。")]
    public async Task SendMessageAsync_ShouldConvertRequestToCommandAndSendToMediator_AndReturnOkResult()
    {
        // Arrange
        DateTime startTime = DateTime.Now;
        
        AutoMocker autoMocker = new();

        SendCustomerMessageRequest request = new()
        {
            Message = Guid.NewGuid().ToString()
        };

        ICollection<SendCustomerMessageCommand> captures = [];
        
        autoMocker.GetMock<IMediator>()
            .Setup(m => m.Send(Capture.In(captures), It.IsAny<CancellationToken>()))
            .Verifiable(Times.Once);
        
        CustomerMessageController controller = autoMocker.CreateInstance<CustomerMessageController>();

        // Act
        IActionResult result = await controller.SendMessageAsync(request);

        // Assert
        Assert.That(result, Is.TypeOf<OkResult>());
        autoMocker.Verify();
        
        Assert.That(captures, Has.Count.EqualTo(1));

        SendCustomerMessageCommand capture = captures.Single();
        
        Assert.That(capture.Message, Is.EqualTo(request.Message));
        Assert.That(capture.CreatedTime, Is.GreaterThanOrEqualTo(startTime));
    }
}