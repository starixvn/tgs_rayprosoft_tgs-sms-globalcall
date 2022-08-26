using System;

namespace Tgs.Sms.GlobalCall
{
    public record BrandInfo(
        Guid Id,
        string Name,
        bool SmsEnabled,
        string? SmsVendorCode,
        GlobalCallSettings? SmsVendorSettings);
}
