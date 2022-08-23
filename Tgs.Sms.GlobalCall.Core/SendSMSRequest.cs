namespace Tgs.Sms.GlobalCall
{
    public class SendSMSRequest
    {
        public string Username { get; set; } = default!;

        public string Password { get; set; } = default!;

        public string Sms { get; set; } = default!;

        public string DestinatingAddress { get; set; } = default!;
    }
}
