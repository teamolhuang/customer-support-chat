using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Commands;
using src.Contracts.Requests.Login;

namespace src.Controllers;

/// <summary>
/// 登入驗證用的相關功能控制器。
/// </summary>
[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 登入驗證用的相關功能控制器。
    /// </summary>
    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 登入。
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginCommandResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
    {
        // 1. 將 request 轉換成 command
        LoginCommand loginCommand = new()
        {
            Username = request.Username,
            Password = request.Password
        };

        // 2. 把 command 傳入 Mediator
        LoginCommandResult result = await _mediator.Send(loginCommand);
        
        // 3. 把 Mediator 的回傳結果放在 Ok() 裡回傳。
        return Ok(result);
    }
}