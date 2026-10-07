using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using OkinawaBot.Application.Requests;
using OkinawaBot.Infrastructure.Line;

namespace OkinawaBot.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LineWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly LineSignatureValidator _signatureValidator;
    private readonly LineClient _lineClient;

    public LineWebhookController(
        IMediator mediator,
        LineSignatureValidator signatureValidator,
        LineClient lineClient)
    {
        _mediator = mediator;
        _signatureValidator = signatureValidator;
        _lineClient = lineClient;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        var signature = Request.Headers["x-line-signature"].ToString();

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (!_signatureValidator.ValidateSignature(body, signature))
        {
            return BadRequest("Invalid signature");
        }

        var request = JsonSerializer.Deserialize<LineWebhookRequest>(body);
        if (request?.Events == null)
        {
            return Ok();
        }

        foreach (var lineEvent in request.Events)
        {
            if (lineEvent.Type != "message" || lineEvent.Message.Type != "text")
            {
                continue;
            }

            var userId = lineEvent.Source.UserId;
            var message = lineEvent.Message.Text;
            var replyToken = lineEvent.ReplyToken;

            var response = await _mediator.Send(new ProcessMessageCommand(userId, message));

            if (!string.IsNullOrEmpty(response.Message))
            {
                await _lineClient.ReplyMessageAsync(replyToken, response.Message);
            }
        }

        return Ok();
    }
}