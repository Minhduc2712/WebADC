namespace ErpOnlineOrder.Application.Constants
{
    public static class PermissionCodes
    {
        public static readonly string[] CategoryOrder =
        {
            "PRODUCT", "CATEGORY", "REGION", "PROVINCE", "ORGANIZATION", "DISTRIBUTOR",
            "CUSTOMER", "ORDER", "INVOICE", "WAREHOUSE", "WAREHOUSE_EXPORT", "STAFF",
            "ROLE", "PERMISSION", "SETTINGS"
        };

        public static readonly HashSet<string> DanhMucChungCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            "REGION", "PROVINCE", "ORGANIZATION", "DISTRIBUTOR", "WAREHOUSE"
        };

        public static readonly Dictionary<string, string> CategoryDisplayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            { "PRODUCT", "Danh mục sản phẩm" },
            { "CATEGORY", "Thể loại" },
            { "REGION", "Vùng miền" },
            { "PROVINCE", "Tỉnh/Thành" },
            { "ORGANIZATION", "Tổ chức" },
            { "DISTRIBUTOR", "Nhà phân phối" },
            { "CUSTOMER", "Danh mục khách hàng" },
            { "ORDER", "Đơn hàng" },
            { "INVOICE", "Hóa đơn" },
            { "WAREHOUSE", "Kho hàng" },
            { "WAREHOUSE_EXPORT", "Xuất kho" },
            { "STAFF", "Danh sách cán bộ" },
            { "ROLE", "Vai trò" },
            { "PERMISSION", "Quyền hạn" },
            { "SETTINGS", "Cài đặt hệ thống" },
            { "REPORT", "Báo cáo" },
            { "OTHER", "Khác" },
            { "DANH_MUC_CHUNG", "Danh mục chung" }
        };

        public static readonly Dictionary<string, string> ActionDisplayNames = new(StringComparer.OrdinalIgnoreCase)
        {
            { "VIEW", "Xem" },
            { "CREATE", "Thêm mới" },
            { "UPDATE", "Cập nhật" },
            { "DELETE", "Xóa" },
            { "EXPORT", "Xuất file" },
            { "IMPORT", "Nhập file" },
            { "APPROVE", "Duyệt" },
            { "REJECT", "Từ chối" },
            { "ASSIGN", "Phân quyền" },
            { "PRINT_2023", "In ủy nhiệm chi năm 2023" },
            { "PRINT_2025", "In ủy nhiệm chi năm 2025" },
            { "PRODUCT_VIEW", "Xem sản phẩm khách hàng" },
            { "PRODUCT_ASSIGN", "Phân quyền sản phẩm" },
            { "EXPORT_VIEW", "Xem" },
            { "EXPORT_CREATE", "Thêm mới" },
            { "EXPORT_UPDATE", "Cập nhật" },
            { "EXPORT_DELETE", "Xóa" }
        };

        // Product Permissions
        public const string ProductView = "PRODUCT_VIEW";
        public const string ProductCreate = "PRODUCT_CREATE";
        public const string ProductUpdate = "PRODUCT_UPDATE";
        public const string ProductDelete = "PRODUCT_DELETE";
        public const string ProductExport = "PRODUCT_EXPORT";
        public const string ProductImport = "PRODUCT_IMPORT";

        // Category Permissions
        public const string CategoryView = "CATEGORY_VIEW";
        public const string CategoryCreate = "CATEGORY_CREATE";
        public const string CategoryUpdate = "CATEGORY_UPDATE";
        public const string CategoryDelete = "CATEGORY_DELETE";

        // Region Permissions
        public const string RegionView = "REGION_VIEW";
        public const string RegionCreate = "REGION_CREATE";
        public const string RegionUpdate = "REGION_UPDATE";
        public const string RegionDelete = "REGION_DELETE";

        // Province Permissions
        public const string ProvinceView = "PROVINCE_VIEW";
        public const string ProvinceCreate = "PROVINCE_CREATE";
        public const string ProvinceUpdate = "PROVINCE_UPDATE";
        public const string ProvinceDelete = "PROVINCE_DELETE";

        // Organization Permissions
        public const string OrganizationView = "ORGANIZATION_VIEW";
        public const string OrganizationCreate = "ORGANIZATION_CREATE";
        public const string OrganizationUpdate = "ORGANIZATION_UPDATE";
        public const string OrganizationDelete = "ORGANIZATION_DELETE";

        // Customer Permissions
        public const string CustomerView = "CUSTOMER_VIEW";
        public const string CustomerCreate = "CUSTOMER_CREATE";
        public const string CustomerUpdate = "CUSTOMER_UPDATE";
        public const string CustomerDelete = "CUSTOMER_DELETE";
        public const string CustomerAssign = "CUSTOMER_ASSIGN";
        public const string CustomerProductView = "CUSTOMER_PRODUCT_VIEW";
        public const string CustomerProductAssign = "CUSTOMER_PRODUCT_ASSIGN";

        // Order Permissions
        public const string OrderView = "ORDER_VIEW";
        public const string OrderCreate = "ORDER_CREATE";
        public const string OrderUpdate = "ORDER_UPDATE";
        public const string OrderDelete = "ORDER_DELETE";
        public const string OrderApprove = "ORDER_APPROVE";
        public const string OrderReject = "ORDER_REJECT";
        public const string OrderExport = "ORDER_EXPORT";

        // Invoice Permissions
        public const string InvoiceView = "INVOICE_VIEW";
        public const string InvoiceCreate = "INVOICE_CREATE";
        public const string InvoiceUpdate = "INVOICE_UPDATE";
        public const string InvoiceDelete = "INVOICE_DELETE";

        // Warehouse Permissions (Kho hàng - master data)
        public const string WarehouseView = "WAREHOUSE_VIEW";
        public const string WarehouseCreate = "WAREHOUSE_CREATE";
        public const string WarehouseUpdate = "WAREHOUSE_UPDATE";
        public const string WarehouseDelete = "WAREHOUSE_DELETE";

        // Warehouse Export Permissions (Phiếu xuất kho)
        public const string WarehouseExportView = "WAREHOUSE_EXPORT_VIEW";
        public const string WarehouseExportCreate = "WAREHOUSE_EXPORT_CREATE";
        public const string WarehouseExportUpdate = "WAREHOUSE_EXPORT_UPDATE";
        public const string WarehouseExportDelete = "WAREHOUSE_EXPORT_DELETE";

        // Staff Permissions
        public const string StaffView = "STAFF_VIEW";
        public const string StaffCreate = "STAFF_CREATE";
        public const string StaffUpdate = "STAFF_UPDATE";
        public const string StaffDelete = "STAFF_DELETE";
        public const string StaffAssignRole = "STAFF_ASSIGN";

        // Distributor Permissions
        public const string DistributorView = "DISTRIBUTOR_VIEW";
        public const string DistributorCreate = "DISTRIBUTOR_CREATE";
        public const string DistributorUpdate = "DISTRIBUTOR_UPDATE";
        public const string DistributorDelete = "DISTRIBUTOR_DELETE";

        // Role Management
        public const string RoleView = "ROLE_VIEW";
        public const string RoleCreate = "ROLE_CREATE";
        public const string RoleUpdate = "ROLE_UPDATE";
        public const string RoleDelete = "ROLE_DELETE";
        public const string RoleAssignPermission = "ROLE_ASSIGN";

        // Permission Management
        public const string PermissionView = "PERMISSION_VIEW";
        public const string PermissionCreate = "PERMISSION_CREATE";
        public const string PermissionUpdate = "PERMISSION_UPDATE";
        public const string PermissionDelete = "PERMISSION_DELETE";
        public const string PermissionAssignUser = "PERMISSION_ASSIGN";

        // Settings
        public const string SettingsView = "SETTINGS_VIEW";
        public const string SettingsUpdate = "SETTINGS_UPDATE";

        // Quyen dac thu (chi tiet, hien thi rieng dropdown)
        public const string InvoicePrint2023 = "INVOICE_PRINT_2023";
        public const string InvoicePrint2025 = "INVOICE_PRINT_2025";
    }
}
