namespace ErpOnlineOrder.Domain.Constants
{
    public static class ExportStatuses
    {
        public const string Draft = "Draft";
        public const string Confirmed = "Confirmed";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Split = "Split";
        public const string Merged = "Merged";

        public static readonly string[] All = { Draft, Confirmed, Completed, Cancelled, Split, Merged };

        /// <summary>Trạng thái cho phép chuyển sang trạng thái mới (thủ công).</summary>
        public static readonly Dictionary<string, string[]> AllowedTransitions = new()
        {
            { Draft, new[] { Confirmed, Cancelled } },
            { Confirmed, new[] { Completed, Cancelled } },
        };

        public static bool CanTransition(string from, string to)
        {
            return AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
        }

        public static string ToDisplayText(string? status) => status switch
        {
            Draft => "Nháp",
            Confirmed => "Đã xác nhận",
            Completed => "Hoàn thành",
            Cancelled => "Đã hủy",
            Split => "Đã tách",
            Merged => "Đã gộp",
            _ => status ?? "N/A"
        };

        public static string ToCssClass(string? status) => status switch
        {
            Draft => "bg-secondary",
            Confirmed => "bg-primary",
            Completed => "bg-success",
            Cancelled => "bg-danger",
            Split => "bg-info",
            Merged => "bg-secondary",
            _ => "bg-secondary"
        };
    }
}
