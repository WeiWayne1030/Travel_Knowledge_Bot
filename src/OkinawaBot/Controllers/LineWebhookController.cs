using Microsoft.AspNetCore.Mvc;
using OkinawaBot.Application.Handlers;
using OkinawaBot.Models;

namespace OkinawaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LineWebhookController : ControllerBase
{
    private readonly MessageHandler _messageHandler;

    public LineWebhookController(
        MessageHandler messageHandler)
    {
        _messageHandler = messageHandler;
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
                await _messageHandler.HandleAsync(
                    userId,
                    message);

            // 暫時先不呼叫 LINE Reply API
            return Ok(response);
        }

        return Ok();
    }
}