using Microsoft.AspNetCore.Mvc;
using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Models;

namespace OkinawaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LineWebhookController : ControllerBase
{
    private readonly IMediator _mediator;

    public LineWebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(
        [FromBody] LineWebhookRequest request)
    {
        foreach (var lineEvent in request.Events)
        {
            if (lineEvent.Type != "message")
            {
                continue;
            }

            if (lineEvent.Message.Type != "text")
            {
                continue;
            }

            var userId = lineEvent.Source.UserId;
            var message = lineEvent.Message.Text;

            var response =
                await _mediator.Send(
                    new ProcessMessageCommand(userId, message));

            // 暫時先不呼叫 LINE Reply API
            return Ok(response);
        }

        return Ok();
    }
}