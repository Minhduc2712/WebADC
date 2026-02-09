namespace ErpOnlineOrder.Application.Constants
{
    public static class ModuleCodes
    {
        public const string Product = "PRODUCT";
        public const string Category = "CATEGORY";
        public const string Region = "REGION";
        public const string Province = "PROVINCE";
        public const string Organization = "ORGANIZATION";
        public const string Customer = "CUSTOMER";
        public const string Order = "ORDER";
        public const string Invoice = "INVOICE";
        public const string Warehouse = "WAREHOUSE";
        public const string Staff = "STAFF";
        public const string Report = "REPORT";
        public const string Distributor = "DISTRIBUTOR";
        public const string Role = "ROLE";
        public const string Permission = "PERMISSION";
    }
    public static class ActionCodes
    {
        public const string View = "VIEW";
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Delete = "DELETE";
        public const string Export = "EXPORT";
        public const string Import = "IMPORT";
        public const string Approve = "APPROVE";
        public const string Reject = "REJECT";
        public const string Assign = "ASSIGN";
    }
    public static class PermissionCodes
    {
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

        // Warehouse Permissions
        public const string WarehouseView = "WAREHOUSE_VIEW";
        public const string WarehouseCreate = "WAREHOUSE_CREATE";
        public const string WarehouseUpdate = "WAREHOUSE_UPDATE";
        public const string WarehouseDelete = "WAREHOUSE_DELETE";

        // Staff Permissions
        public const string StaffView = "STAFF_VIEW";
        public const string StaffCreate = "STAFF_CREATE";
        public const string StaffUpdate = "STAFF_UPDATE";
        public const string StaffDelete = "STAFF_DELETE";
        public const string StaffAssignRole = "STAFF_ASSIGN";

        // Report Permissions
        public const string ReportView = "REPORT_VIEW";
        public const string ReportExport = "REPORT_EXPORT";

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
    }
}
