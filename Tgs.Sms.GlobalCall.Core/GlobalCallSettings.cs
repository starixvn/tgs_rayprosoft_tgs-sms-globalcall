namespace Tgs.Sms.GlobalCall
{
    public record GlobalCallSettings
    {
        public string Username { get; init; } = default!;

        public string Password { get; init; } = default!;
    };
}
