using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using src.Commands;
using src.Contracts.Requests.CustomerMessage;

namespace src.Controllers;

/// <summary>
/// 前台使用者對訊息的操作。
/// </summary>
[ApiController]
[Route("api/customer-message")]
public class CustomerMessageController : ControllerBase
{
    private IMediator Mediator { get; }

    /// <summary>
    /// 前台使用者對訊息的操作。
    /// </summary>
    public CustomerMessageController(IMediator mediator)
    {
        Mediator = mediator;
    }

    /// <summary>
    /// 前台使用者發送訊息。
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // TODO: 授權模組
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessageAsync([FromBody] SendCustomerMessageRequest request)
    {
        SendCustomerMessageCommand command = new()
        {
            Message = request.Message,
            CreatedTime = DateTime.Now
        };
        
        await Mediator.Send(command);

        return Ok();
    }
}