using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace OkinawaBot.Infrastructure.Line;

public class LineSignatureValidator
{
    private readonly LineBotOptions _options;

    public LineSignatureValidator(IOptions<LineBotOptions> options)
    {
        _options = options.Value;
    }

    public bool ValidateSignature(string body, string signature)
    {
        if (string.IsNullOrEmpty(_options.ChannelSecret) || string.IsNullOrEmpty(signature))
        {
            return false;
        }

        var secretBytes = Encoding.UTF8.GetBytes(_options.ChannelSecret);
        using var hmac = new HMACSHA256(secretBytes);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hash = hmac.ComputeHash(bodyBytes);
        var calculatedSignature = Convert.ToBase64String(hash);

        return signature == calculatedSignature;
    }
}
