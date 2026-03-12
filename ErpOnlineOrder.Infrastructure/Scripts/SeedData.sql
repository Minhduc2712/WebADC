-- =============================================
-- Script: SeedData.sql
-- Description: Seed du lieu mac dinh cho ErpOnlineOrder
-- Synchronized with PermissionCodes.cs + Domain Models
-- =============================================

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. SEED ROLES
-- =============================================
PRINT N'[1/8] Seeding Roles...';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_ADMIN')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_ADMIN', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_STAFF')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_STAFF', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_CUSTOMER')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_CUSTOMER', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Roles OK';
GO

-- =============================================
-- 2. SEED PERMISSIONS (match PermissionCodes.cs)
-- =============================================
PRINT N'[2/8] Seeding Permissions...';

-- Helper: insert permission neu chua ton tai
-- Flat permissions (Parent_id = NULL, Is_special = 0)

-- Product
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_EXPORT')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_EXPORT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_IMPORT')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_IMPORT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Category
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Region
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Province
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Organization
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Customer
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_PRODUCT_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_PRODUCT_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_PRODUCT_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_PRODUCT_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Order
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_APPROVE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_APPROVE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_REJECT')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_REJECT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_EXPORT')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_EXPORT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Invoice
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Warehouse (kho hang - master data)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Warehouse Export (parent permission + children)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT' AND Parent_id IS NULL)
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
GO

-- Warehouse Export children (need Parent_id from above)
DECLARE @WeParentId INT;
SELECT @WeParentId = Id FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT' AND Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_VIEW', @WeParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_CREATE', @WeParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_UPDATE', @WeParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_DELETE', @WeParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
GO

-- Staff
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Distributor
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Role
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Permission management
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

-- Settings (parent + children)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS' AND Parent_id IS NULL)
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
GO

DECLARE @SettingsParentId INT;
SELECT @SettingsParentId = Id FROM Permissions WHERE Permission_code = 'SETTINGS' AND Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS_VIEW', @SettingsParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS_UPDATE', @SettingsParentId, 0, 0, GETDATE(), 0, GETDATE(), 0);
GO

-- Special permissions (Is_special = 1)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_PRINT_2023')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_PRINT_2023', NULL, 1, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_PRINT_2025')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_PRINT_2025', NULL, 1, 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Permissions OK';
GO

-- =============================================
-- 3. SEED ADMIN USER (Password: Admin@123)
-- =============================================
PRINT N'[3/8] Seeding Admin User...';

DECLARE @AdminUserId INT;
DECLARE @AdminRoleId INT;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('admin', 'admin@erp.com', '$2a$11$rBNPE0Ebc5sXb0e.yNHOEeBs4yXP1gqpv4deyJPmvmFxuQ2TJP/Pu', 1, 0, GETDATE(), 0, GETDATE(), 0);
    SET @AdminUserId = SCOPE_IDENTITY();
END
ELSE
    SELECT @AdminUserId = Id FROM Users WHERE Username = 'admin';

-- Staff cho admin
IF NOT EXISTS (SELECT 1 FROM Staffs WHERE User_id = @AdminUserId)
    INSERT INTO Staffs (Staff_code, Full_name, Phone_number, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ADMIN001', N'Quản trị viên', '0900000000', @AdminUserId, 0, GETDATE(), 0, GETDATE(), 0);

-- Gan role ROLE_ADMIN
SELECT @AdminRoleId = Id FROM Roles WHERE Role_name = 'ROLE_ADMIN';
IF @AdminRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE User_id = @AdminUserId AND Role_id = @AdminRoleId AND Is_deleted = 0)
    INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@AdminUserId, @AdminRoleId, 0, GETDATE(), 0, GETDATE(), 0);

-- Gan tat ca permissions cho ROLE_ADMIN
INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
SELECT @AdminRoleId, p.Id, 0, GETDATE(), 0, GETDATE(), 0
FROM Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.Id AND rp.Is_deleted = 0
);

PRINT N'  -> Admin User OK';
GO

-- =============================================
-- 4. SEED CUSTOMER USER (Password: Customer@123)
-- =============================================
PRINT N'[4/8] Seeding Customer User...';

DECLARE @CustomerUserId INT;
DECLARE @CustomerRoleId INT;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'khachhang')
BEGIN
    INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('khachhang', 'khachhang@example.com', '$2a$11$BxgS94uIDdgwyLWbcueLGe7aJHKigKxT4ts.XU9g4AciDI2AlpRmK', 1, 0, GETDATE(), 0, GETDATE(), 0);
    SET @CustomerUserId = SCOPE_IDENTITY();
END
ELSE
    SELECT @CustomerUserId = Id FROM Users WHERE Username = 'khachhang';

IF NOT EXISTS (SELECT 1 FROM Customers WHERE User_id = @CustomerUserId)
    INSERT INTO Customers (Customer_code, Full_name, Phone_number, Address, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('KH001', N'Khách hàng mẫu', '0900000001', N'Quận 1, TP.HCM', @CustomerUserId, 0, GETDATE(), 0, GETDATE(), 0);

SELECT @CustomerRoleId = Id FROM Roles WHERE Role_name = 'ROLE_CUSTOMER';
IF @CustomerRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE User_id = @CustomerUserId AND Role_id = @CustomerRoleId AND Is_deleted = 0)
    INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@CustomerUserId, @CustomerRoleId, 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Customer User OK';
GO

-- =============================================
-- 5. SEED MASTER DATA
-- =============================================
PRINT N'[5/8] Seeding Master Data...';

-- Regions
IF NOT EXISTS (SELECT 1 FROM Regions WHERE Region_code = 'MB')
    INSERT INTO Regions (Region_code, Region_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('MB', N'Miền Bắc', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Regions WHERE Region_code = 'MT')
    INSERT INTO Regions (Region_code, Region_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('MT', N'Miền Trung', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Regions WHERE Region_code = 'MN')
    INSERT INTO Regions (Region_code, Region_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('MN', N'Miền Nam', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Regions WHERE Region_code = 'TN')
    INSERT INTO Regions (Region_code, Region_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TN', N'Tây Nguyên', 0, GETDATE(), 0, GETDATE(), 0);

-- Provinces
DECLARE @RegionMB INT, @RegionMT INT, @RegionMN INT, @RegionTN INT;
SELECT @RegionMB = Id FROM Regions WHERE Region_code = 'MB';
SELECT @RegionMT = Id FROM Regions WHERE Region_code = 'MT';
SELECT @RegionMN = Id FROM Regions WHERE Region_code = 'MN';
SELECT @RegionTN = Id FROM Regions WHERE Region_code = 'TN';

IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN', N'Hà Nội', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HCM')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM', N'TP. Hồ Chí Minh', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN', N'Đà Nẵng', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HP')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP', N'Hải Phòng', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'BN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BN', N'Bắc Ninh', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);

-- Categories
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Category_code = 'SACH')
    INSERT INTO Categories (Category_code, Category_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SACH', N'Sách', 0, GETDATE(), 0, GETDATE(), 0);

-- Authors
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG001')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG001', N'Nguyễn Nhật Ánh', N'Nguyễn Nhật Ánh', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG002')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG002', N'Tô Hoài', N'Tô Hoài', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);

-- Publishers
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB001')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB001', N'NXB Kim Đồng', N'Hà Nội', '0241234567', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB002')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB002', N'NXB Trẻ', N'TP. Hồ Chí Minh', '0281234567', 0, GETDATE(), 0, GETDATE(), 0);

-- CoverTypes
IF NOT EXISTS (SELECT 1 FROM CoverTypes WHERE Cover_type_code = 'BC')
    INSERT INTO CoverTypes (Cover_type_code, Cover_type_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BC', N'Bìa cứng', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM CoverTypes WHERE Cover_type_code = 'BM')
    INSERT INTO CoverTypes (Cover_type_code, Cover_type_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BM', N'Bìa mềm', 0, GETDATE(), 0, GETDATE(), 0);

-- Distributors
IF NOT EXISTS (SELECT 1 FROM Distributors WHERE Distributor_code = 'NPP001')
    INSERT INTO Distributors (Distributor_code, Distributor_name, Distributor_address, Distributor_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NPP001', N'Nhà phân phối Fahasa', N'TP. Hồ Chí Minh', '0281111111', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Distributors WHERE Distributor_code = 'NPP002')
    INSERT INTO Distributors (Distributor_code, Distributor_name, Distributor_address, Distributor_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NPP002', N'Nhà phân phối Tiki', N'TP. Hồ Chí Minh', '0282222222', 0, GETDATE(), 0, GETDATE(), 0);

-- Warehouses
DECLARE @ProvinceHN INT, @ProvinceHCM INT;
SELECT @ProvinceHN = Id FROM Provinces WHERE Province_code = 'HN';
SELECT @ProvinceHCM = Id FROM Provinces WHERE Province_code = 'HCM';

IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Warehouse_code = 'KHO01')
    INSERT INTO Warehouses (Warehouse_code, Warehouse_name, Warehouse_address, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('KHO01', N'Kho Hà Nội', N'Số 123, Đường Láng, Quận Đống Đa, Hà Nội', @ProvinceHN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Warehouse_code = 'KHO02')
    INSERT INTO Warehouses (Warehouse_code, Warehouse_name, Warehouse_address, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('KHO02', N'Kho TP.HCM', N'Số 456, Đường Nguyễn Văn Linh, Quận 7, TP.HCM', @ProvinceHCM, 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Master Data OK';
GO

-- =============================================
-- 6. SEED SYSTEM SETTINGS (SMTP)
-- =============================================
PRINT N'[6/8] Seeding SystemSettings...';

IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_HOST')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_HOST', 'smtp.gmail.com', N'SMTP server host', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_PORT')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_PORT', '587', N'SMTP port (587 TLS, 465 SSL)', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_USE_SSL')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_USE_SSL', 'false', N'Use SSL (true/false)', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_FROM_NAME')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_FROM_NAME', 'ERP Online Order', N'Tên người gửi', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_FROM_EMAIL')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_FROM_EMAIL', '', N'Email người gửi', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'SMTP_PASSWORD')
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SMTP_PASSWORD', '', N'Mật khẩu ứng dụng email', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> SystemSettings OK';
GO

-- =============================================
-- 7. SEED SAMPLE PRODUCTS
-- =============================================
PRINT N'[7/8] Seeding Sample Products...';

DECLARE @PubId1 INT, @PubId2 INT, @CoverId1 INT, @CoverId2 INT;
DECLARE @DistId1 INT, @CatId1 INT, @AuthId1 INT, @AuthId2 INT;

SELECT @PubId1 = Id FROM Publishers WHERE Publisher_code = 'NXB001';
SELECT @PubId2 = Id FROM Publishers WHERE Publisher_code = 'NXB002';
SELECT @CoverId1 = Id FROM CoverTypes WHERE Cover_type_code = 'BC';
SELECT @CoverId2 = Id FROM CoverTypes WHERE Cover_type_code = 'BM';
SELECT @DistId1 = Id FROM Distributors WHERE Distributor_code = 'NPP001';
SELECT @CatId1 = Id FROM Categories WHERE Category_code = 'SACH';
SELECT @AuthId1 = Id FROM Authors WHERE Author_code = 'TG001';
SELECT @AuthId2 = Id FROM Authors WHERE Author_code = 'TG002';

-- Product 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP001')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP001', N'Tôi thấy hoa vàng trên cỏ xanh', 85000, N'Truyện dài của Nguyễn Nhật Ánh', 10, @CoverId2, @PubId1, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);

    DECLARE @Pid1 INT = SCOPE_IDENTITY();

    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid1, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid1, @AuthId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid1, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 2
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP002')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP002', N'Dế Mèn phiêu lưu ký', 55000, N'Tác phẩm nổi tiếng của Tô Hoài', 10, @CoverId1, @PubId1, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);

    DECLARE @Pid2 INT = SCOPE_IDENTITY();

    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid2, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid2, @AuthId2, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid2, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 3
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP003')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP003', N'Mắt biếc', 75000, N'Truyện tình cảm của Nguyễn Nhật Ánh', 10, @CoverId2, @PubId2, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);

    DECLARE @Pid3 INT = SCOPE_IDENTITY();

    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid3, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid3, @AuthId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid3, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

PRINT N'  -> Sample Products OK';
GO

-- =============================================
-- 8. VERIFICATION
-- =============================================
PRINT N'[8/8] Verification...';
PRINT N'';

SELECT 'Roles' AS [Table], COUNT(*) AS [Count] FROM Roles WHERE Is_deleted = 0
UNION ALL SELECT 'Permissions', COUNT(*) FROM Permissions WHERE Is_deleted = 0
UNION ALL SELECT 'Users', COUNT(*) FROM Users WHERE Is_deleted = 0
UNION ALL SELECT 'Staffs', COUNT(*) FROM Staffs WHERE Is_deleted = 0
UNION ALL SELECT 'Customers', COUNT(*) FROM Customers WHERE Is_deleted = 0
UNION ALL SELECT 'Regions', COUNT(*) FROM Regions WHERE Is_deleted = 0
UNION ALL SELECT 'Provinces', COUNT(*) FROM Provinces WHERE Is_deleted = 0
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories WHERE Is_deleted = 0
UNION ALL SELECT 'Authors', COUNT(*) FROM Authors WHERE Is_deleted = 0
UNION ALL SELECT 'Publishers', COUNT(*) FROM Publishers WHERE Is_deleted = 0
UNION ALL SELECT 'CoverTypes', COUNT(*) FROM CoverTypes WHERE Is_deleted = 0
UNION ALL SELECT 'Distributors', COUNT(*) FROM Distributors WHERE Is_deleted = 0
UNION ALL SELECT 'Warehouses', COUNT(*) FROM Warehouses WHERE Is_deleted = 0
UNION ALL SELECT 'Products', COUNT(*) FROM Products WHERE Is_deleted = 0
UNION ALL SELECT 'SystemSettings', COUNT(*) FROM SystemSettings WHERE Is_deleted = 0;

PRINT N'';
PRINT N'=== Seed data hoan tat ===';
GO
