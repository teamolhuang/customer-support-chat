using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Controllers;

namespace tests.ControllerTests.CustomerMessageTests;

[TestFixture]
[Description("CustomerMessageController 中，針對取得訊息方法的一系列測試。")]
public class CustomerMessageGetMessagesTests
{
    [Test]
    [Description("驗證取得訊息時，應傳入一個空的 GetCustomerMessagesCommand 至 Mediator，並回傳包含其結果的 200。")]
    public async Task GetMessagesAsync_ShouldPassGetCustomerMessagesCommandToMediator_AndReturnCommandResultAsOk()
    {
        // Arrange
        AutoMocker autoMocker = new();

        CustomerMessageController controller = autoMocker.CreateInstance<CustomerMessageController>();

        GetCustomerMessagesCommandResult mockedCommandResult = new()
        {
            Messages = [new GetCustomerMessagesCommandResultMessage
            {
                Content = Guid.NewGuid().ToString(),
                CreatedTime = DateTime.Now,
                IsFromUser = true
            }]
        };

        autoMocker.GetMock<IMediator>()
            .Setup(m => m.Send(It.IsAny<GetCustomerMessagesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockedCommandResult);
        
        // Act
        IActionResult result = await controller.GetMessagesAsync();

        // Assert
        
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        
        OkObjectResult okResult = (OkObjectResult)result;
        
        Assert.That(okResult.Value, Is.TypeOf<GetCustomerMessagesCommandResult>());

        GetCustomerMessagesCommandResult commandResult = (GetCustomerMessagesCommandResult)okResult.Value;
        
        Assert.That(commandResult.Messages, Is.EqualTo(mockedCommandResult.Messages));

        autoMocker.Verify();
    }
}