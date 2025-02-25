using MediatR;

namespace src.Commands;

/// <summary>
/// 使用者登入的 Command
/// </summary>
public class LoginCommand : IRequest<LoginCommandResult>
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; set; } = null!;
}