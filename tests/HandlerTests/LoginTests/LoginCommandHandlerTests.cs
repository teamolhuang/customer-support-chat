using Moq;
using Moq.AutoMock;
using Moq.EntityFrameworkCore;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contracts.Dtos;
using src.Handlers;
using src.Utilities.Abstracts;

namespace tests.HandlerTests.LoginTests;

[TestFixture]
[Description("驗證登入時商業邏輯 Handler 的一系列測試方法。")]
public class LoginCommandHandlerTests
{
    [Test]
    [Description("驗證登入時，應從 Db 找出對應的帳號並核對密碼後，回傳 JWT Access Token 與過期時間。")]
    public async Task Handle_ShouldQueryAccountAndValidatePassword_AndReturnJwtAndExpiration()
    {
        // Arrange
        
        LoginCommand request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        AutoMocker autoMocker = new();

        Random random = new();
        Account mockData = new()
        {
            Id = random.Next(),
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        Mock<DatabaseContext> mockDb = new();
        mockDb.Setup(ctx => ctx.Accounts)
            .ReturnsDbSet([mockData])
            .Verifiable(Times.Once);
        
        autoMocker.Use(mockDb);
        
        TimeSpan mockExpireHours = TimeSpan.FromHours(random.Next(1, 100)); // 避免骰到太大的數字導致 TimeSpan overflow
        
        JwtTokenResult mockToken = new()
        {
            AccessToken = Guid.NewGuid().ToString(),
            Expiration = DateTime.Now.Add(mockExpireHours)
        };

        autoMocker.GetMock<IJwtHelper>()
            .Setup(jwt => jwt.CreateTokenAsync(mockData.Id))
            .ReturnsAsync(mockToken)
            .Verifiable(Times.Once);
        
        LoginCommandHandler handler = autoMocker.CreateInstance<LoginCommandHandler>();

        // Act
        LoginCommandResult result = await handler.Handle(request, default);

        // Assert
        Assert.That(result.AccessToken, Is.EqualTo(mockToken.AccessToken));
        Assert.That(result.Expiration, Is.EqualTo(mockToken.Expiration));

        autoMocker.Verify();
    }

    [Test]
    [Description("驗證登入時，如果沒有找到相同使用者名稱的帳號，應拋出 NullReferenceException。")]
    public async Task Handle_ShouldQueryAccount_AndThrowNullReferenceExceptionIfNotFound()
    {
        // Arrange
        LoginCommand request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        AutoMocker autoMocker = new();

        Mock<DatabaseContext> mockDb = new();
        
        mockDb.Setup(ctx => ctx.Accounts)
            .ReturnsDbSet([])
            .Verifiable(Times.Once);
        
        autoMocker.Use(mockDb);
        
        autoMocker.GetMock<IJwtHelper>()
            .VerifyNoOtherCalls();
        
        LoginCommandHandler handler = autoMocker.CreateInstance<LoginCommandHandler>();

        // Act & Assert
        NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(() => handler.Handle(request, default));
        
        Assert.That(exception?.Message, Is.EqualTo("使用者帳號或密碼錯誤，請重新確認！"));
        autoMocker.Verify();
    }

    [Test]
    [Description("驗證登入時，查到帳號後，應該驗證雜湊密碼，並在驗證失敗時拋出 NullReferenceException。")]
    public async Task
        Handle_ShouldQueryAccountAndVerifyPasswordHash_AndThrowNullReferenceExceptionIfPasswordInvalid()
    {
        // Arrange
        LoginCommand request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        AutoMocker autoMocker = new();

        Mock<DatabaseContext> mockDb = new();

        Account mockData = new()
        {
            Id = new Random().Next(),
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password + "#")
        };
        
        mockDb.Setup(ctx => ctx.Accounts)
            .ReturnsDbSet([mockData])
            .Verifiable(Times.Once);
        
        autoMocker.Use(mockDb);
        
        autoMocker.GetMock<IJwtHelper>()
            .VerifyNoOtherCalls();
        
        LoginCommandHandler handler = autoMocker.CreateInstance<LoginCommandHandler>();

        // Act & Assert
        NullReferenceException? exception = Assert.ThrowsAsync<NullReferenceException>(() => handler.Handle(request, default));
        
        Assert.That(exception?.Message, Is.EqualTo("使用者帳號或密碼錯誤，請重新確認！"));
        autoMocker.Verify();
    }
}