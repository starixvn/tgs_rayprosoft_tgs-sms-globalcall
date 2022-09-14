using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Security.Application;
using Tgs.Sms.GlobalCall.Models;

namespace Tgs.Sms.GlobalCall
{
    public class GlobalCallClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GlobalCallClient> _logger;

        public GlobalCallClient(
            HttpClient httpClient,
            ILogger<GlobalCallClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<SendSMSResponse?> SendSMSAsync(SendSMSRequest request)
        {
            var queryStrings = HttpUtility.ParseQueryString(string.Empty);

            queryStrings.Add("username", request.Username);
            queryStrings.Add("password", request.Password);
            queryStrings.Add("destinatingAddress", request.DestinatingAddress.Replace("+", ""));

            var xmlEncodeString = Encoder.XmlEncode(request.Sms);
            var sms = Uri.EscapeDataString(xmlEncodeString);

            queryStrings.Add("SMS", sms);

            var httpResponseMessage = await _httpClient
                .GetAsync($"/cs/sms/openapi/Send?{queryStrings}")
                .ConfigureAwait(false);

            var responseMessage = await httpResponseMessage.Content
            .ReadAsStringAsync()
            .ConfigureAwait(false);

            _logger.LogInformation(responseMessage);

            var transactionId = responseMessage.Trim();

            _ = httpResponseMessage.EnsureSuccessStatusCode();

            if (transactionId == "-1")
            {
                _logger.LogError("Wrong Destination Number:{DestinatingAddress}", request.DestinatingAddress);
                return null;
            }
            else if (transactionId == "-2")
            {
                _logger.LogError("Account Information / username / IP / password incorrect: Username: {Username}, Password:{Password}", request.Username, request.Password);
                return null;
            }
            else if (transactionId == "-3")
            {
                _logger.LogError("Content ID incorrect");
                return null;
            }
            else if (transactionId == "-4")
            {
                _logger.LogError("Destination Number not all correct");
                return null;
            }
            else if (transactionId == "-100")
            {
                _logger.LogError("Internal Error, please contact support");
                return null;
            }

            return new SendSMSResponse
            {
                TransactionID = transactionId
            };
        }
    }
}
