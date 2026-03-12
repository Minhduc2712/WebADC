namespace ErpOnlineOrder.Domain.Constants
{
    public static class DeliveryStatuses
    {
        public const string Pending = "Pending";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";

        public static readonly string[] All = { Pending, Shipped, Delivered };

        public static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { Pending, new[] { Shipped } },
            { Shipped, new[] { Delivered } },
        };

        public static bool CanTransition(string from, string to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        public static string ToDisplayText(string? status) => status switch
        {
            Pending => "Chờ giao",
            Shipped => "Đã giao",
            Delivered => "Đã nhận",
            _ => status ?? "N/A"
        };

        public static string ToCssClass(string? status) => status switch
        {
            Pending => "bg-secondary",
            Shipped => "bg-info",
            Delivered => "bg-success",
            _ => "bg-secondary"
        };
    }
}
