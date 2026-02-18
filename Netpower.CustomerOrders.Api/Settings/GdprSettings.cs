namespace Netpower.CustomerOrders.Api.Settings
{
    public sealed class GdprSettings
    {
        public int DataRetentionDays { get; set; } = 2555; // ~7 years
        public bool EnableAnonymization { get; set; } = true;
    }
}