using System.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using src.Commands;
using src.Contexts.Database.Entities;

namespace src.Handlers;

/// <summary>
/// 處理使用者註冊後，把資料寫到 DB 的 handler。
/// </summary>
public class RegisterCommandDbHandler : IRequestHandler<RegisterCommand, RegisterCommandResult>
{
    private readonly DatabaseContext _dbContext;

    /// <summary>
    /// 處理使用者註冊後，把資料寫到 DB 的 handler。
    /// </summary>
    public RegisterCommandDbHandler(DatabaseContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<RegisterCommandResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. 檢查相同 Username 的帳號是否已存在，如果已存在，拋出 DuplicateNameException
        bool isExist = await _dbContext.Accounts
            .AnyAsync(a => a.Username == request.Username, cancellationToken: cancellationToken);

        if (isExist)
            throw new DuplicateNameException("非常抱歉，您使用的帳號名稱已被其他人註冊，請更換其他名稱。");
        
        // 2. 將 request 轉換成 Account
        // 密碼需要經過雜湊

        Account account = new()
        {
            Username = request.Username,
            HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        // 3. 把 Account 寫入 DB 並存檔
        await _dbContext.AddAsync(account, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        // 4. 回傳 CommandResult
        RegisterCommandResult result = new()
        {
            Id = account.Id,
            Username = account.Username
        };

        return result;
    }
}