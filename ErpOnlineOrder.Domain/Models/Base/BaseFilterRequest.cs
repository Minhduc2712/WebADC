// Base – chỉ chứa params chung
public abstract class BasePaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortColumn { get; set; }
    public bool IsAscending { get; set; } = true;
}

// Product – thêm filter riêng
public class ProductFilterRequest : BasePaginationRequest
{
    public int? CategoryId { get; set; }
    public int? PublisherId { get; set; }
}

// Order – thêm filter riêng
public class OrderFilterRequest : BasePaginationRequest
{
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

// Customer – thêm filter riêng
public class CustomerFilterRequest : BasePaginationRequest
{
    public int? RegionId { get; set; }
    public int? CustomerCategoryId { get; set; }
}

// Invoice – thêm filter riêng
public class InvoiceFilterRequest : BasePaginationRequest
{
    public string? Status { get; set; }
}

// WarehouseExport – thêm filter riêng
public class WarehouseExportFilterRequest : BasePaginationRequest
{
    public string? Status { get; set; }
}

// Staff – thêm filter riêng
public class StaffFilterRequest : BasePaginationRequest
{
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

// Product cho Shop (khách hàng) – category theo tên, sort
public class ProductForShopFilterRequest : BasePaginationRequest
{
    public string? Category { get; set; }
    public string? Sort { get; set; }
}