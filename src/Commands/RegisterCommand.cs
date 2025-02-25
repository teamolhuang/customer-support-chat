using MediatR;

namespace src.Commands;

/// <summary>
/// 使用者註冊的 Command。
/// </summary>
public class RegisterCommand : IRequest<RegisterCommandResult>
{
    /// <summary>
    /// 帳號。
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// 密碼。
    /// </summary>
    public string Password { get; set; } = null!;
}