using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.AutoMock;
using src.Commands;
using src.Contracts.Requests.Login;
using src.Controllers;

namespace tests.ControllerTests.AuthenticationTests;

[TestFixture]
[Description("針對驗證用控制器中，登入時的一系列測試方法。")]
public class AuthenticationLoginTests
{
    [Test]
    [Description("驗證 LoginAsync 會把要求轉成 command 後向後呼叫 Mediator 進行處理，並回傳包含 Command 處理器的回傳結果的 200。")]
    public async Task LoginAsync_ShouldConvertLoginRequestToLoginCommandAndPassToMediator_AndReturnCommandResultAsOk()
    {
        // Arrange
        DateTime startTime = DateTime.Now;

        LoginRequest request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        AutoMocker autoMocker = new();

        ICollection<LoginCommand> captures = [];
        LoginCommandResult mockedCommandResult = new()
        {
            AccessToken = Guid.NewGuid().ToString(),
            Expiration = startTime.AddHours(new Random().Next(1, 100))
        };
        autoMocker.GetMock<IMediator>()
            .Setup(m => m.Send(Capture.In(captures), default))
            .ReturnsAsync(mockedCommandResult);
        
        AuthenticationController authenticationController = autoMocker.CreateInstance<AuthenticationController>();

        // Act

        IActionResult result = await authenticationController.LoginAsync(request);

        // Assert
        Assert.That(captures, Has.Count.EqualTo(1));
        
        LoginCommand capture = captures.Single();
        
        Assert.That(capture.Username, Is.EqualTo(request.Username));
        Assert.That(capture.Password, Is.EqualTo(request.Password));
        
        Assert.That(result, Is.TypeOf<OkObjectResult>());
        OkObjectResult okResult = (OkObjectResult)result;
        
        Assert.That(okResult.Value, Is.TypeOf<LoginCommandResult>());
        LoginCommandResult loginCommandResult = (LoginCommandResult)okResult.Value;

        Assert.That(loginCommandResult.AccessToken, Is.EqualTo(mockedCommandResult.AccessToken));
        Assert.That(loginCommandResult.Expiration, Is.EqualTo(mockedCommandResult.Expiration));

        autoMocker.Verify();
    }
}