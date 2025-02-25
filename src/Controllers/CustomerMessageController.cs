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
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendMessageAsync([FromBody] SendCustomerMessageRequest request)
    {
        SendCustomerMessageNotification notification = new()
        {
            Message = request.Message,
            CreatedTime = DateTime.Now
        };
        
        await Mediator.Publish(notification);

        return Ok();
    }

    /// <summary>
    /// 取得訊息。
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(GetCustomerMessagesCommandResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesAsync()
    {
        GetCustomerMessagesCommandResult result = await Mediator.Send(new GetCustomerMessagesCommand());

        return Ok(result);
    }
}