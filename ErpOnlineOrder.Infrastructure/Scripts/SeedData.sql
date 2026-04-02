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

-- Package
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PACKAGE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PACKAGE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PACKAGE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PACKAGE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PACKAGE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PACKAGE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PACKAGE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PACKAGE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

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

-- Ward
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WARD_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WARD_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WARD_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WARD_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WARD_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WARD_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WARD_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WARD_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

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
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_PACKAGE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_PACKAGE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_PACKAGE_ASSIGN')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_PACKAGE_ASSIGN', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

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

-- Stock (Ton kho)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STOCK_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STOCK_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STOCK_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STOCK_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STOCK_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STOCK_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STOCK_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STOCK_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

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

-- Staff Region Rule (Phan cong theo vung)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_REGION_RULE_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_REGION_RULE_VIEW', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_REGION_RULE_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_REGION_RULE_CREATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_REGION_RULE_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_REGION_RULE_UPDATE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_REGION_RULE_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_REGION_RULE_DELETE', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

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

-- More provinces - Miền Bắc
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'QN2')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('QN2', N'Quảng Ninh', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HD')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HD', N'Hải Dương', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'VP')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('VP', N'Vĩnh Phúc', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'TNG')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TNG', N'Thái Nguyên', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'ND')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ND', N'Nam Định', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);

-- More provinces - Miền Trung
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'NA')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NA', N'Nghệ An', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'TTH')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TTH', N'Thừa Thiên Huế', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'KH')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('KH', N'Khánh Hòa', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'BDINH')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BDINH', N'Bình Định', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);

-- More provinces - Tây Nguyên
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'LD')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('LD', N'Lâm Đồng', @RegionTN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DKL')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DKL', N'Đắk Lắk', @RegionTN, 0, GETDATE(), 0, GETDATE(), 0);

-- More provinces - Miền Nam
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'CT')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CT', N'Cần Thơ', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DNAI')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DNAI', N'Đồng Nai', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'BD')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BD', N'Bình Dương', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'LA')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('LA', N'Long An', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'AG')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('AG', N'An Giang', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);

-- Wards
DECLARE @WProv_HN INT, @WProv_HCM INT, @WProv_DN INT, @WProv_HP INT;
SELECT @WProv_HN  = Id FROM Provinces WHERE Province_code = 'HN';
SELECT @WProv_HCM = Id FROM Provinces WHERE Province_code = 'HCM';
SELECT @WProv_DN  = Id FROM Provinces WHERE Province_code = 'DN';
SELECT @WProv_HP  = Id FROM Provinces WHERE Province_code = 'HP';

-- Hà Nội - Quận Hoàn Kiếm
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-001', N'Phường Hàng Bạc', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-002', N'Phường Hàng Gai', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-003', N'Phường Hàng Đào', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-004')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-004', N'Phường Tràng Tiền', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-005')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-005', N'Phường Lý Thái Tổ', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

-- Hà Nội - Quận Cầu Giấy
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-CG-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-CG-001', N'Phường Cầu Giấy', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-CG-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-CG-002', N'Phường Dịch Vọng', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-CG-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-CG-003', N'Phường Nghĩa Đô', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-CG-004')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-CG-004', N'Phường Quan Hoa', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-001', N'Phường Cát Linh', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-002', N'Phường Hàng Bột', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-003', N'Phường Quốc Tử Giám', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q1-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q1-001', N'Phường Bến Nghé', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q1-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q1-002', N'Phường Bến Thành', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q1-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q1-003', N'Phường Tân Định', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q1-004')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q1-004', N'Phường Phạm Ngũ Lão', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q1-005')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q1-005', N'Phường Cầu Ông Lãnh', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q3-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q3-001', N'Phường 1', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q3-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q3-002', N'Phường 2', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q3-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q3-003', N'Phường Võ Thị Sáu', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q7-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q7-001', N'Phường Tân Phú', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q7-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q7-002', N'Phường Tân Quy', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HCM-Q7-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM-Q7-003', N'Phường Phú Mỹ', @WProv_HCM, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'DN-HC-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN-HC-001', N'Phường Hải Châu I', @WProv_DN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'DN-HC-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN-HC-002', N'Phường Nam Dương', @WProv_DN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'DN-HC-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN-HC-003', N'Phường Thạch Thang', @WProv_DN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'DN-ST-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN-ST-001', N'Phường Mân Thái', @WProv_DN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'DN-ST-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN-ST-002', N'Phường An Hải Bắc', @WProv_DN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-006')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-006', N'Phường Hoàn Kiếm', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HK-007')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HK-007', N'Phường Cửa Nam', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-BD-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-BD-001', N'Phường Ba Đình', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-BD-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-BD-002', N'Phường Ngọc Hà', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-BD-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-BD-003', N'Phường Giảng Võ', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HBT-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HBT-001', N'Phường Hai Bà Trưng', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HBT-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HBT-002', N'Phường Vĩnh Tuy', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-HBT-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-HBT-003', N'Phường Bạch Mai', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-004')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-004', N'Phường Đống Đa', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-005')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-005', N'Phường Kim Liên', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HN-DD-006')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN-DD-006', N'Phường Văn Miếu - Quốc Tử Giám', @WProv_HN, 0, GETDATE(), 0, GETDATE(), 0);

-- Hải Phòng
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-001')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-001', N'Phường Thủy Nguyên', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-002')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-002', N'Phường Thiên Hương', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-003')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-003', N'Phường Hòa Bình', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-004')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-004', N'Phường Nam Triệu', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-005')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-005', N'Phường Bạch Đằng', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-006')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-006', N'Phường Lưu Kiếm', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-007')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-007', N'Phường Lê Ích Mộc', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-008')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-008', N'Phường Hồng Bàng', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-009')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-009', N'Phường Hồng An', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-010')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-010', N'Phường Ngô Quyền', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Wards WHERE Ward_code = 'HP-011')
    INSERT INTO Wards (Ward_code, Ward_name, Province_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP-011', N'Phường Gia Viên', @WProv_HP, 0, GETDATE(), 0, GETDATE(), 0);

-- Categories
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Category_code = 'SACH')
    INSERT INTO Categories (Category_code, Category_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SACH', N'Sách', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Category_code = 'SGK-TH')
    INSERT INTO Categories (Category_code, Category_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SGK-TH', N'Sách giáo khoa tiểu học', 0, GETDATE(), 0, GETDATE(), 0);

-- Authors
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG001')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG001', N'Nguyễn Nhật Ánh', N'Nguyễn Nhật Ánh', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG002')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG002', N'Tô Hoài', N'Tô Hoài', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG003')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG003', N'Paulo Coelho', N'Paulo Coelho', N'Brazil', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG004')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG004', N'Dale Carnegie', N'Dale Carnegie', N'Mỹ', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG005')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG005', N'Vũ Trọng Phụng', N'Vũ Trọng Phụng', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG006')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG006', N'J.K. Rowling', N'J.K. Rowling', N'Anh', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG007')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG007', N'Antoine de Saint-Exupéry', N'Antoine de Saint-Exupéry', N'Pháp', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG008')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG008', N'Bộ Giáo dục và Đào tạo', N'Bộ GD&ĐT', N'Việt Nam', 0, GETDATE(), 0, GETDATE(), 0);

-- Publishers
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB001')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB001', N'NXB Kim Đồng', N'Hà Nội', '0241234567', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB002')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB002', N'NXB Trẻ', N'TP. Hồ Chí Minh', '0281234567', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB003')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB003', N'NXB Văn học', N'Hà Nội', '0243456789', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB004')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB004', N'NXB Hội Nhà Văn', N'Hà Nội', '0244567890', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB005')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB005', N'NXB Giáo dục Việt Nam', N'Hà Nội', '0245678901', 0, GETDATE(), 0, GETDATE(), 0);

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
IF NOT EXISTS (SELECT 1 FROM Distributors WHERE Distributor_code = 'NPP003')
    INSERT INTO Distributors (Distributor_code, Distributor_name, Distributor_address, Distributor_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NPP003', N'Nhà phân phối Đinh Tị', N'Hà Nội', '0243333333', 0, GETDATE(), 0, GETDATE(), 0);

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
-- 6b. SEED ORGANIZATIONS (Đơn vị / Trường học)
-- =============================================
PRINT N'[6b] Seeding Organizations...';

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV001')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV001', N'Trường Tiểu học Thăng Long', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV002')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV002', N'Trường Tiểu học Trưng Vương', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV003')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV003', N'Trường Tiểu học Nam Từ Liêm', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV004')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV004', N'Trường TH, THCS & THPT Thực Nghiệm', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV005')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV005', N'Trường Tiểu học Lý Thường Kiệt', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV006')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV006', N'Trường Tiểu học Trung Văn', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV007')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV007', N'Trường Tiểu Học Ba Đình', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM OrganizationInformations WHERE Organization_code = 'DV008')
    INSERT INTO OrganizationInformations (Organization_code, Organization_name, Address, Tax_number, Recipient_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DV008', N'Trường Tiểu học Nam Thành Công', N'Hà Nội', '', '', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Organizations OK';
GO

-- =============================================
-- 7. SEED SAMPLE PRODUCTS
-- =============================================
PRINT N'[7/8] Seeding Sample Products...';

DECLARE @PubId1 INT, @PubId2 INT, @PubId3 INT, @PubId4 INT, @CoverId1 INT, @CoverId2 INT;
DECLARE @DistId1 INT, @DistId2 INT, @CatId1 INT, @AuthId1 INT, @AuthId2 INT;
DECLARE @AuthId3 INT, @AuthId4 INT, @AuthId5 INT, @AuthId6 INT, @AuthId7 INT;

SELECT @PubId1 = Id FROM Publishers WHERE Publisher_code = 'NXB001';
SELECT @PubId2 = Id FROM Publishers WHERE Publisher_code = 'NXB002';
SELECT @PubId3 = Id FROM Publishers WHERE Publisher_code = 'NXB003';
SELECT @PubId4 = Id FROM Publishers WHERE Publisher_code = 'NXB004';
SELECT @CoverId1 = Id FROM CoverTypes WHERE Cover_type_code = 'BC';
SELECT @CoverId2 = Id FROM CoverTypes WHERE Cover_type_code = 'BM';
SELECT @DistId1 = Id FROM Distributors WHERE Distributor_code = 'NPP001';
SELECT @DistId2 = Id FROM Distributors WHERE Distributor_code = 'NPP003';
SELECT @CatId1 = Id FROM Categories WHERE Category_code = 'SACH';
SELECT @AuthId1 = Id FROM Authors WHERE Author_code = 'TG001';
SELECT @AuthId2 = Id FROM Authors WHERE Author_code = 'TG002';
SELECT @AuthId3 = Id FROM Authors WHERE Author_code = 'TG003';
SELECT @AuthId4 = Id FROM Authors WHERE Author_code = 'TG004';
SELECT @AuthId5 = Id FROM Authors WHERE Author_code = 'TG005';
SELECT @AuthId6 = Id FROM Authors WHERE Author_code = 'TG006';
SELECT @AuthId7 = Id FROM Authors WHERE Author_code = 'TG007';

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

-- Product 4
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP004')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP004', N'Nhà giả kim', 120000, N'Tiểu thuyết triết học nổi tiếng của Paulo Coelho', 10, @CoverId2, @PubId3, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid4 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid4, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid4, @AuthId3, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid4, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 5
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP005')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP005', N'Đắc nhân tâm', 95000, N'Sách kỹ năng sống kinh điển của Dale Carnegie', 10, @CoverId2, @PubId3, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid5 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid5, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid5, @AuthId4, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid5, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 6
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP006')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP006', N'Số đỏ', 65000, N'Tiểu thuyết trào phúng nổi tiếng của Vũ Trọng Phụng', 10, @CoverId1, @PubId4, @DistId2, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid6 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid6, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid6, @AuthId5, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid6, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 7
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP007')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP007', N'Harry Potter và Hòn đá Phù thủy', 150000, N'Tập 1 bộ truyện Harry Potter của J.K. Rowling', 10, @CoverId1, @PubId2, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid7 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid7, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid7, @AuthId6, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid7, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 8
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP008')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP008', N'Hoàng Tử Bé', 70000, N'Tác phẩm kinh điển của Antoine de Saint-Exupéry', 10, @CoverId2, @PubId1, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid8 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid8, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid8, @AuthId7, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid8, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- Product 9
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP009')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP009', N'Cho tôi xin một vé đi tuổi thơ', 85000, N'Truyện dài của Nguyễn Nhật Ánh', 10, @CoverId2, @PubId2, @DistId1, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @Pid9 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid9, @CatId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid9, @AuthId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@Pid9, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

PRINT N'  -> Sample Products OK';
GO

-- =============================================
-- 7b. SEED ELEMENTARY SCHOOL TEXTBOOKS (Sách giáo khoa tiểu học)
-- =============================================
PRINT N'[7b] Seeding Elementary School Textbooks...';

DECLARE @NXB_GD INT, @NPP_FA INT, @CAT_SGK INT, @AUTH_BGD INT, @CV_BM INT;
SELECT @NXB_GD  = Id FROM Publishers  WHERE Publisher_code  = 'NXB005';
SELECT @NPP_FA  = Id FROM Distributors WHERE Distributor_code = 'NPP001';
SELECT @CAT_SGK = Id FROM Categories  WHERE Category_code   = 'SGK-TH';
SELECT @AUTH_BGD= Id FROM Authors     WHERE Author_code     = 'TG008';
SELECT @CV_BM   = Id FROM CoverTypes  WHERE Cover_type_code = 'BM';

-- SP034: Toán 1 - Kết nối tri thức với cuộc sống
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP034')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP034', N'Toán 1 - Kết nối tri thức với cuộc sống', 28000, N'Sách giáo khoa Toán lớp 1 bộ Kết nối tri thức với cuộc sống, dành cho học sinh tiểu học.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P34 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P34, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P34, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P34, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP035: Tiếng Việt 1 - Tập 1 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP035')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP035', N'Tiếng Việt 1 Tập 1 - Kết nối tri thức với cuộc sống', 22000, N'Sách giáo khoa Tiếng Việt lớp 1 tập 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P35 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P35, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P35, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P35, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP036: Tiếng Việt 1 - Tập 2 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP036')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP036', N'Tiếng Việt 1 Tập 2 - Kết nối tri thức với cuộc sống', 22000, N'Sách giáo khoa Tiếng Việt lớp 1 tập 2 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P36 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P36, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P36, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P36, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP037: Tự nhiên và Xã hội 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP037')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP037', N'Tự nhiên và Xã hội 1 - Kết nối tri thức với cuộc sống', 22000, N'Sách giáo khoa Tự nhiên và Xã hội lớp 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P37 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P37, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P37, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P37, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP038: Đạo đức 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP038')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP038', N'Đạo đức 1 - Kết nối tri thức với cuộc sống', 18000, N'Sách giáo khoa Đạo đức lớp 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P38 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P38, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P38, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P38, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP039: Toán 2 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP039')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP039', N'Toán 2 - Kết nối tri thức với cuộc sống', 29000, N'Sách giáo khoa Toán lớp 2 bộ Kết nối tri thức với cuộc sống.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P39 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P39, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P39, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P39, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP040: Tiếng Việt 2 Tập 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP040')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP040', N'Tiếng Việt 2 Tập 1 - Kết nối tri thức với cuộc sống', 24000, N'Sách giáo khoa Tiếng Việt lớp 2 tập 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P40 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P40, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P40, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P40, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP041: Tự nhiên và Xã hội 2
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP041')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP041', N'Tự nhiên và Xã hội 2 - Kết nối tri thức với cuộc sống', 23000, N'Sách giáo khoa Tự nhiên và Xã hội lớp 2 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P41 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P41, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P41, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P41, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP042: Toán 3 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP042')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP042', N'Toán 3 - Kết nối tri thức với cuộc sống', 30000, N'Sách giáo khoa Toán lớp 3 bộ Kết nối tri thức với cuộc sống.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P42 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P42, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P42, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P42, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP043: Tiếng Việt 3 Tập 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP043')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP043', N'Tiếng Việt 3 Tập 1 - Kết nối tri thức với cuộc sống', 25000, N'Sách giáo khoa Tiếng Việt lớp 3 tập 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P43 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P43, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P43, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P43, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP044: Tự nhiên và Xã hội 3
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP044')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP044', N'Tự nhiên và Xã hội 3 - Kết nối tri thức với cuộc sống', 24000, N'Sách giáo khoa Tự nhiên và Xã hội lớp 3 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P44 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P44, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P44, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P44, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP045: Toán 4 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP045')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP045', N'Toán 4 - Kết nối tri thức với cuộc sống', 32000, N'Sách giáo khoa Toán lớp 4 bộ Kết nối tri thức với cuộc sống.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P45 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P45, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P45, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P45, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP046: Tiếng Việt 4 Tập 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP046')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP046', N'Tiếng Việt 4 Tập 1 - Kết nối tri thức với cuộc sống', 27000, N'Sách giáo khoa Tiếng Việt lớp 4 tập 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P46 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P46, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P46, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P46, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP047: Khoa học 4
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP047')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP047', N'Khoa học 4 - Kết nối tri thức với cuộc sống', 26000, N'Sách giáo khoa Khoa học lớp 4 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P47 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P47, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P47, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P47, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP048: Lịch sử và Địa lý 4
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP048')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP048', N'Lịch sử và Địa lý 4 - Kết nối tri thức với cuộc sống', 28000, N'Sách giáo khoa Lịch sử và Địa lý lớp 4 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P48 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P48, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P48, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P48, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP049: Toán 5 - Kết nối tri thức
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP049')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP049', N'Toán 5 - Kết nối tri thức với cuộc sống', 33000, N'Sách giáo khoa Toán lớp 5 bộ Kết nối tri thức với cuộc sống.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P49 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P49, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P49, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P49, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP050: Tiếng Việt 5 Tập 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP050')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP050', N'Tiếng Việt 5 Tập 1 - Kết nối tri thức với cuộc sống', 28000, N'Sách giáo khoa Tiếng Việt lớp 5 tập 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P50 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P50, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P50, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P50, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP051: Khoa học 5
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP051')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP051', N'Khoa học 5 - Kết nối tri thức với cuộc sống', 27000, N'Sách giáo khoa Khoa học lớp 5 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P51 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P51, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P51, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P51, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP052: Lịch sử và Địa lý 5
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP052')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP052', N'Lịch sử và Địa lý 5 - Kết nối tri thức với cuộc sống', 29000, N'Sách giáo khoa Lịch sử và Địa lý lớp 5 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P52 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P52, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P52, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P52, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP053: Mĩ thuật 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP053')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP053', N'Mĩ thuật 1 - Kết nối tri thức với cuộc sống', 19000, N'Sách giáo khoa Mĩ thuật lớp 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P53 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P53, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P53, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P53, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

-- SP054: Âm nhạc 1
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP054')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP054', N'Âm nhạc 1 - Kết nối tri thức với cuộc sống', 18000, N'Sách giáo khoa Âm nhạc lớp 1 bộ Kết nối tri thức.', 5, @CV_BM, @NXB_GD, @NPP_FA, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P54 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P54, @CAT_SGK, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductAuthors    (Product_id, Author_id,   Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P54, @AUTH_BGD, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages     (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P54, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

PRINT N'  -> Elementary School Textbooks OK';
GO

-- =============================================
-- 7c. SEED STOCK (Tồn kho)
-- =============================================
PRINT N'[7c] Seeding Stocks...';

DECLARE @KHO01 INT, @KHO02 INT;
SELECT @KHO01 = Id FROM Warehouses WHERE Warehouse_code = 'KHO01';
SELECT @KHO02 = Id FROM Warehouses WHERE Warehouse_code = 'KHO02';

-- Hàm tiện ích: chèn stock nếu chưa có
-- (dùng EXISTS theo cặp Warehouse_id + Product_id vì không có unique constraint tên)

-- === Sách văn học (SP001 – SP009) · KHO01 & KHO02 ===
DECLARE @SP001 INT, @SP002 INT, @SP003 INT, @SP004 INT, @SP005 INT;
DECLARE @SP006 INT, @SP007 INT, @SP008 INT, @SP009 INT;
SELECT @SP001 = Id FROM Products WHERE Product_code = 'SP001';
SELECT @SP002 = Id FROM Products WHERE Product_code = 'SP002';
SELECT @SP003 = Id FROM Products WHERE Product_code = 'SP003';
SELECT @SP004 = Id FROM Products WHERE Product_code = 'SP004';
SELECT @SP005 = Id FROM Products WHERE Product_code = 'SP005';
SELECT @SP006 = Id FROM Products WHERE Product_code = 'SP006';
SELECT @SP007 = Id FROM Products WHERE Product_code = 'SP007';
SELECT @SP008 = Id FROM Products WHERE Product_code = 'SP008';
SELECT @SP009 = Id FROM Products WHERE Product_code = 'SP009';

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP001 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (120, @KHO01, @SP001, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP001 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (80, @KHO02, @SP001, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP002 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (95, @KHO01, @SP002, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP002 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (60, @KHO02, @SP002, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP003 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (75, @KHO01, @SP003, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP003 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (50, @KHO02, @SP003, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP004 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (110, @KHO01, @SP004, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP004 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (70, @KHO02, @SP004, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP005 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (85, @KHO01, @SP005, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP005 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (55, @KHO02, @SP005, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP006 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (60, @KHO01, @SP006, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP006 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (40, @KHO02, @SP006, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP007 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (45, @KHO01, @SP007, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP007 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (30, @KHO02, @SP007, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP008 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (100, @KHO01, @SP008, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP008 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (65, @KHO02, @SP008, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP009 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (55, @KHO01, @SP009, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO02 AND Product_id = @SP009 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (35, @KHO02, @SP009, 0, GETDATE(), 0, GETDATE(), 0);

-- === Sách giáo khoa tiểu học (SP034 – SP054) · chủ yếu ở KHO01 (Hà Nội) ===
DECLARE @SP034 INT, @SP035 INT, @SP036 INT, @SP037 INT, @SP038 INT, @SP039 INT, @SP040 INT;
DECLARE @SP041 INT, @SP042 INT, @SP043 INT, @SP044 INT, @SP045 INT, @SP046 INT, @SP047 INT;
DECLARE @SP048 INT, @SP049 INT, @SP050 INT, @SP051 INT, @SP052 INT, @SP053 INT, @SP054 INT;

SELECT @SP034 = Id FROM Products WHERE Product_code = 'SP034';
SELECT @SP035 = Id FROM Products WHERE Product_code = 'SP035';
SELECT @SP036 = Id FROM Products WHERE Product_code = 'SP036';
SELECT @SP037 = Id FROM Products WHERE Product_code = 'SP037';
SELECT @SP038 = Id FROM Products WHERE Product_code = 'SP038';
SELECT @SP039 = Id FROM Products WHERE Product_code = 'SP039';
SELECT @SP040 = Id FROM Products WHERE Product_code = 'SP040';
SELECT @SP041 = Id FROM Products WHERE Product_code = 'SP041';
SELECT @SP042 = Id FROM Products WHERE Product_code = 'SP042';
SELECT @SP043 = Id FROM Products WHERE Product_code = 'SP043';
SELECT @SP044 = Id FROM Products WHERE Product_code = 'SP044';
SELECT @SP045 = Id FROM Products WHERE Product_code = 'SP045';
SELECT @SP046 = Id FROM Products WHERE Product_code = 'SP046';
SELECT @SP047 = Id FROM Products WHERE Product_code = 'SP047';
SELECT @SP048 = Id FROM Products WHERE Product_code = 'SP048';
SELECT @SP049 = Id FROM Products WHERE Product_code = 'SP049';
SELECT @SP050 = Id FROM Products WHERE Product_code = 'SP050';
SELECT @SP051 = Id FROM Products WHERE Product_code = 'SP051';
SELECT @SP052 = Id FROM Products WHERE Product_code = 'SP052';
SELECT @SP053 = Id FROM Products WHERE Product_code = 'SP053';
SELECT @SP054 = Id FROM Products WHERE Product_code = 'SP054';

IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP034 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (500, @KHO01, @SP034, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP035 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (500, @KHO01, @SP035, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP036 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (450, @KHO01, @SP036, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP037 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (450, @KHO01, @SP037, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP038 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (400, @KHO01, @SP038, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP039 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (400, @KHO01, @SP039, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP040 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (380, @KHO01, @SP040, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP041 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (380, @KHO01, @SP041, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP042 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (360, @KHO01, @SP042, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP043 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (360, @KHO01, @SP043, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP044 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (340, @KHO01, @SP044, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP045 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (340, @KHO01, @SP045, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP046 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (320, @KHO01, @SP046, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP047 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (320, @KHO01, @SP047, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP048 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (300, @KHO01, @SP048, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP049 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (300, @KHO01, @SP049, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP050 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (280, @KHO01, @SP050, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP051 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (280, @KHO01, @SP051, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP052 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (260, @KHO01, @SP052, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP053 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (260, @KHO01, @SP053, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Stocks WHERE Warehouse_id = @KHO01 AND Product_id = @SP054 AND Is_deleted = 0)
    INSERT INTO Stocks (Quantity, Warehouse_id, Product_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (240, @KHO01, @SP054, 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'  -> Stocks OK';
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
UNION ALL SELECT 'Wards', COUNT(*) FROM Wards WHERE Is_deleted = 0
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories WHERE Is_deleted = 0
UNION ALL SELECT 'Authors', COUNT(*) FROM Authors WHERE Is_deleted = 0
UNION ALL SELECT 'Publishers', COUNT(*) FROM Publishers WHERE Is_deleted = 0
UNION ALL SELECT 'CoverTypes', COUNT(*) FROM CoverTypes WHERE Is_deleted = 0
UNION ALL SELECT 'Distributors', COUNT(*) FROM Distributors WHERE Is_deleted = 0
UNION ALL SELECT 'Warehouses', COUNT(*) FROM Warehouses WHERE Is_deleted = 0
UNION ALL SELECT 'Products', COUNT(*) FROM Products WHERE Is_deleted = 0
UNION ALL SELECT 'Orders', COUNT(*) FROM Orders WHERE Is_deleted = 0
UNION ALL SELECT 'OrderDetails', COUNT(*) FROM OrderDetails WHERE Is_deleted = 0
UNION ALL SELECT 'Invoices', COUNT(*) FROM Invoices WHERE Is_deleted = 0
UNION ALL SELECT 'WarehouseExports', COUNT(*) FROM WarehouseExports WHERE Is_deleted = 0
UNION ALL SELECT 'WarehouseExportDetails', COUNT(*) FROM WarehouseExportDetails WHERE Is_deleted = 0
UNION ALL SELECT 'SystemSettings', COUNT(*) FROM SystemSettings WHERE Is_deleted = 0;

PRINT N'';
PRINT N'=== Seed data hoan tat ===';
GO
