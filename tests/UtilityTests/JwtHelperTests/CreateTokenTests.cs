using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.AutoMock;
using src.Contracts.Dtos;
using src.Utilities;

namespace tests.UtilityTests.JwtHelperTests;

[TestFixture]
[Description("驗證 JwtHelper 發行 JWT 時的一系列測試方法。")]
public class CreateTokenTests
{
    [Test]
    [Description("驗證 CreateTokenAsync 應該從參數取得 JWT 有效小時數後，建立包含 Account ID 的 JWT，並連同其有效日期一起回傳。")]
    public async Task
        CreateTokenAsync_ShouldUseExpirationFromAppSettings_AndReturnExpirationAndAccessTokenContainingAccountId()
    {
        // Arrange
        DateTime startTime = DateTime.UtcNow;
        Random random = new();
        int accountId = random.Next();
        
        string mockSecret = Guid.NewGuid().ToString();
        int mockExpiration = random.Next(1, 100);
        
        AutoMocker autoMocker = new();

        Mock<AppSettings> mockAppSettings = autoMocker.GetMock<AppSettings>();

        mockAppSettings.SetupGet(app => app.Jwt)
            .Returns(new JwtSettings
            {
                SecretKey = mockSecret,
                ExpirationHour = mockExpiration
            })
            .Verifiable(Times.Once);
        
        autoMocker.GetMock<IOptions<AppSettings>>()
            .SetupGet(c => c.Value)
            .Returns(mockAppSettings.Object)
            .Verifiable(Times.Once);
        
        JwtHelper jwtHelper = autoMocker.CreateInstance<JwtHelper>();

        // Act

        JwtTokenResult result = await jwtHelper.CreateTokenAsync(accountId);

        // Assert
        
        Assert.That(result.AccessToken, Is.Not.Null);

        JsonWebTokenHandler jwtSecurityTokenHandler = new();
        TokenValidationResult? jwtResult = await jwtSecurityTokenHandler.ValidateTokenAsync(result.AccessToken, new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mockSecret)),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateAudience = false,
            ValidateIssuer = false
        });
        
        Assert.That(jwtResult.Claims.FirstOrDefault(c => c.Key == ClaimTypes.Sid).Value, Is.EqualTo(accountId));
        Assert.That(result.Expiration, Is.GreaterThanOrEqualTo(startTime.AddHours(mockExpiration)));
        
        autoMocker.Verify();
    }
}