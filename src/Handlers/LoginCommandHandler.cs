using MediatR;
using Microsoft.EntityFrameworkCore;
using src.Commands;
using src.Contexts.Database.Entities;
using src.Contracts.Dtos;
using src.Utilities.Abstracts;

namespace src.Handlers;

/// <summary>
/// 使用者登入時的 command 處理。
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly DatabaseContext _dbContext;
    private readonly IJwtHelper _jwtHelper;

    /// <summary>
    /// 使用者登入時的 command 處理。
    /// </summary>
    public LoginCommandHandler(DatabaseContext dbContext,
        IJwtHelper jwtHelper)
    {
        _dbContext = dbContext;
        _jwtHelper = jwtHelper;
    }

    /// <inheritdoc />
    public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. 到 DB 找相同的帳號，驗證密碼
        Account data = await _dbContext.Accounts
            .FirstOrDefaultAsync(acc => acc.Username == request.Username, cancellationToken: cancellationToken)
            ?? ThrowWrongCredential();
        
        bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(request.Password, data.HashedPassword);

        if (!isCorrectPassword)
            ThrowWrongCredential();
        
        // 2. 建立 JWT Token
        JwtTokenResult token = await _jwtHelper.CreateTokenAsync(data.Id);

        // 3. 回傳
        return new LoginCommandResult
        {
            AccessToken = token.AccessToken,
            Expiration = token.Expiration
        };
    }

    private static Account ThrowWrongCredential()
    {
        throw new NullReferenceException("使用者帳號或密碼錯誤，請重新確認！");
    }
}