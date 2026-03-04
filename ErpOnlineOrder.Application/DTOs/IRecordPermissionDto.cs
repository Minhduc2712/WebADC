namespace ErpOnlineOrder.Application.DTOs
{
    public interface IRecordPermissionDto
    {
        bool AllowUpdate { get; set; }
        bool AllowDelete { get; set; }
    }
}
