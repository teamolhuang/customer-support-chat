using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Controllers;
using RegisterRequest = src.Contracts.Requests.Register.RegisterRequest;

namespace tests.ControllerTests.Account;

[TestFixture]
[Description("針對 AccountController 中註冊端點的一系列測試方法。")]
public class AccountControllerRegisterTests
{
    [Test]
    [Description("驗證 RegisterAsync 方法應該接受指定的傳入物件並傳進 Mediator，再回傳包含相關資訊的 201。")]
    public async Task RegisterAsync_ShouldAcceptRegisterRequestAndSendToMediator_ThenReturnCreatedResult()
    {
        // Arrange
        RegisterRequest request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };

        RegisterCommandResult mockResult = new()
        {
            Id = new Random().Next(),
            Username = request.Username
        };
        
        AutoMocker autoMocker = new();

        ICollection<RegisterCommand> caputres = new List<RegisterCommand>();

        autoMocker.GetMock<IMediator>()
            .Setup(m => m.Send(Capture.In(caputres), default))
            .ReturnsAsync(mockResult)
            .Verifiable(Times.Once);
        
        AccountController controller = autoMocker.CreateInstance<AccountController>();
        
        // Act
        IActionResult result = await controller.RegisterAsync(request);

        // Assert
        Assert.That(result, Is.TypeOf<CreatedResult>());
        
        CreatedResult createdResult = (CreatedResult)result;
        
        Assert.That(createdResult.Location, Is.EqualTo($"api/account/{mockResult.Id}"));
        Assert.That(createdResult.Value, Is.TypeOf<RegisterCommandResult>());
        
        RegisterCommandResult commandResult =  (RegisterCommandResult)createdResult.Value;
        
        Assert.That(commandResult.Id, Is.EqualTo(mockResult.Id));
        Assert.That(commandResult.Username, Is.EqualTo(request.Username));
        
        autoMocker.VerifyAll();
    }
    
}