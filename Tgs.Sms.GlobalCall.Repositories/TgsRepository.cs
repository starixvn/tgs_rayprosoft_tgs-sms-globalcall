using System;
using System.Threading.Tasks;
using Tgs.Sms.GlobalCall.Core;
using Tgs.Sms.GlobalCall.Repositories.Entities;

namespace Tgs.Sms.GlobalCall.Repositories
{
    internal class TgsRepository : ITgsRepository
    {
        private readonly TgsContext _tgsContext;
        private readonly GlobalCallSerializer _globalCallSerializer;

        public TgsRepository(TgsContext tgsContext, GlobalCallSerializer globalCallSerializer)
        {
            _tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
            _globalCallSerializer = globalCallSerializer ?? throw new ArgumentNullException(nameof(globalCallSerializer));
        }

        public async ValueTask<BrandInfo?> GetBrandInfoByIdAsync(Guid brandId)
        {
            var brand = await _tgsContext.Brands
                .FindAsync(brandId)
                .ConfigureAwait(false);

            if (brand == null)
                return null;

            return new BrandInfo(
                brand.Id,
                brand.Code,
                brand.EnableSms,
                brand.SmsVendorCode,
                string.IsNullOrWhiteSpace(brand.SmsVendorSettings)
                    ? null
                    : _globalCallSerializer.Deserialize<GlobalCallSettings>(brand.SmsVendorSettings));
        }
    }
}
