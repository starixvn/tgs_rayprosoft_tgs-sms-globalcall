using System;
using System.Threading.Tasks;

namespace Tgs.Sms.GlobalCall
{
    public interface ITgsRepository
    {
        public ValueTask<BrandInfo?> GetBrandInfoByIdAsync(Guid brandId);
    }
}
