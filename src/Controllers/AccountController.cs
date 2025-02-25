using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Commands;
using src.Contracts.Requests.Register;

namespace src.Controllers;

/// <summary>
/// 使用者帳號的操作。
/// </summary>
[ApiController]
[Route("api/account")]
public class AccountController : Controller
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 使用者帳號的操作。
    /// </summary>
    public AccountController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 註冊帳號。
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        RegisterCommand command = new()
        {
            Username = request.Username,
            Password = request.Password
        };
        
        RegisterCommandResult result = await _mediator.Send(command);

        return Created($"api/account/{result.Id}", result);
    }
    
}