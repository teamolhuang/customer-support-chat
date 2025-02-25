using System.Data;
using Moq;
using Moq.AutoMock;
using Moq.EntityFrameworkCore;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Handlers;

namespace tests.HandlerTests.RegisterTests;

[TestFixture]
[Description("針對註冊功能寫進 DB 的一系列測試方法。")]
public class RegisterCommandDbHandlerTests
{
    [Test]
    [Description("驗證 Handle 的處理應該將密碼雜湊後，在 DB 新建一筆資料。")]
    public async Task Handle_ShouldHashPassword_AndWriteNewAccountInDb()
    {
        // Arrange
        RegisterCommand request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        AutoMocker autoMocker = new();

        Mock<DatabaseContext> mockDb = new();

        ICollection<Account> captures = [];

        mockDb.Setup(ctx => ctx.Accounts)
            .ReturnsDbSet([]);
        
        mockDb.Setup(ctx => ctx.AddAsync(Capture.In(captures), It.IsAny<CancellationToken>()))
            .Verifiable(Times.Once);

        mockDb.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Verifiable(Times.Once);
        
        autoMocker.Use(mockDb);
        
        RegisterCommandDbHandler handler = autoMocker.CreateInstance<RegisterCommandDbHandler>();

        // Act
        RegisterCommandResult result = await handler.Handle(request, default);

        // Assert
        Assert.That(captures, Has.Count.EqualTo(1));

        Account entity = captures.Single();
        
        Assert.That(entity.Username, Is.EqualTo(request.Username));
        Assert.That(BCrypt.Net.BCrypt.Verify(request.Password, entity.HashedPassword), Is.True);
        
        Assert.That(result.Username, Is.EqualTo(entity.Username));
        
        autoMocker.Verify();
    }

    [Test]
    [Description("驗證 Handle 應該先到 DB 檢查帳號是否已存在，如果已存在，拋出 DuplicateNameException。")]
    public async Task Handle_ShouldCheckIfAccountDuplicated_AndThrowDuplicateNameException()
    {
        // Arrange
        AutoMocker autoMocker = new();

        RegisterCommand request = new()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Guid.NewGuid().ToString()
        };
        
        Account mockData = new()
        {
            Username = request.Username
        };

        Mock<DatabaseContext> mockedContext = new();
            
        mockedContext.Setup(ctx => ctx.Accounts)
            .ReturnsDbSet([mockData])
            .Verifiable(Times.Once);
        
        mockedContext.Setup(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Verifiable(Times.Never);
        
        autoMocker.Use(mockedContext);
        
        RegisterCommandDbHandler handler = autoMocker.CreateInstance<RegisterCommandDbHandler>();
        
        // Act & Assert
        Assert.ThrowsAsync<DuplicateNameException>(async () => await handler.Handle(request, default));

        autoMocker.Verify();
    }
}