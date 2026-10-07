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
    private readonly ILineClient _lineClient;
    private readonly ILogger<LineWebhookController> _logger;

    public LineWebhookController(
        IMediator mediator,
        LineSignatureValidator signatureValidator,
        ILineClient lineClient,
        ILogger<LineWebhookController> logger)
    {
        _mediator = mediator;
        _signatureValidator = signatureValidator;
        _lineClient = lineClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        var signature = Request.Headers["x-line-signature"].ToString();

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();

        if (!_signatureValidator.ValidateSignature(body, signature))
        {
            _logger.LogWarning("Invalid LINE webhook signature detected. Header: {Signature}", signature);
            return Unauthorized("Invalid signature.");
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
                _logger.LogInformation("Received non-text message or non-message event. Type: {Type}", lineEvent.Type);
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