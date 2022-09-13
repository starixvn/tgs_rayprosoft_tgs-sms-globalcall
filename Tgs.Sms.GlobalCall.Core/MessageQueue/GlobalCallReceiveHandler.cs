using Microsoft.Extensions.Logging;
using Tgs.MessageQueue;
using Tgs.Sms.Protos;

namespace Tgs.Sms.GlobalCall.MessageQueue
{
    public class GlobalCallReceiveHandler : IMessageHandler
    {
        private readonly ITgsRepository _tgsRepository;
        private readonly ILogger<GlobalCallReceiveHandler> _logger;
        private readonly GlobalCallClient _globalCallClient;

        public GlobalCallReceiveHandler(
            ITgsRepository tgsRepository,
            ILogger<GlobalCallReceiveHandler> logger,
            GlobalCallClient globalCallClient)
        {
            _tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _globalCallClient = globalCallClient ?? throw new ArgumentNullException(nameof(globalCallClient));
        }

        public async ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var shortMsg = ShortMessage.Parser.ParseFrom(data.Span);
            var brandId = shortMsg.BrandId.ToGuid();

            _logger.LogInformation(
                "Receive sms: [{nameof(shortMsg.BrandId)}] {brandId} " +
                "[{nameof(shortMsg.Destination)}] {shortMsg.Destination} " +
                "[{nameof(shortMsg.Content)}] {shortMsg.Content}",
                nameof(shortMsg.BrandId),
                brandId,
                nameof(shortMsg.Destination),
                shortMsg.Destination,
                nameof(shortMsg.Content),
                shortMsg.Content);

            var brandInfo = await _tgsRepository
                .GetBrandInfoByIdAsync(brandId)
                .ConfigureAwait(false);

            if (brandInfo == null)
            {
                _logger.LogInformation($"Can't find brand info ({brandId})");

                return;
            }

            if (!brandInfo.SmsEnabled || brandInfo.SmsVendorSettings == null)
            {
                _logger.LogWarning("Brand setting warn");

                return;
            }

            var request = new SendSMSRequest
            {
                Username = brandInfo.SmsVendorSettings.Username,
                Password = brandInfo.SmsVendorSettings.Password,
                DestinatingAddress = shortMsg.Destination,
                Sms = shortMsg.Content
            };

            var response = await _globalCallClient.SendSMSAsync(request).ConfigureAwait(false);

            if (response is null)
                throw new Exception("Sms send fail");

            _logger.LogInformation("Sms sent.");
        }
    }
}
