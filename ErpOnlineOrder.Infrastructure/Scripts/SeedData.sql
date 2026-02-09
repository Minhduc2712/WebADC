-- =============================================
-- Script: SeedData.sql
-- Description: Seed du lieu mac dinh cho ErpOnlineOrder
-- Version: 2.0 - Complete seed data
-- =============================================

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. SEED ROLES
-- =============================================
PRINT N'Seeding Roles...';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_ADMIN')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_ADMIN', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_STAFF')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_STAFF', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_CUSTOMER')
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_CUSTOMER', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'Da seed Roles';
GO

-- =============================================
-- 2. SEED PERMISSIONS
-- =============================================
PRINT N'Seeding Permissions...';

-- Product permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_EXPORT')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_EXPORT', 0, GETDATE(), 0, GETDATE(), 0);

-- Order permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_APPROVE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_APPROVE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_REJECT')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_REJECT', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORDER_EXPORT')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORDER_EXPORT', 0, GETDATE(), 0, GETDATE(), 0);

-- Customer permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Invoice permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'INVOICE_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('INVOICE_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Warehouse permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Staff permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Category permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CATEGORY_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CATEGORY_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Region permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REGION_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REGION_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Province permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PROVINCE_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PROVINCE_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

-- Role permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_ASSIGN')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ROLE_ASSIGN', 0, GETDATE(), 0, GETDATE(), 0);

-- Fix: Cập nhật ROLE_ASSIGN_PERMISSION cũ thành ROLE_ASSIGN nếu tồn tại
UPDATE Permissions SET Permission_code = 'ROLE_ASSIGN' WHERE Permission_code = 'ROLE_ASSIGN_PERMISSION' AND NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ROLE_ASSIGN');

-- Permission management permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PERMISSION_ASSIGN')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PERMISSION_ASSIGN', 0, GETDATE(), 0, GETDATE(), 0);

-- Staff assign permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_ASSIGN')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('STAFF_ASSIGN', 0, GETDATE(), 0, GETDATE(), 0);

-- Report permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REPORT_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REPORT_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'REPORT_EXPORT')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('REPORT_EXPORT', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'Da seed Permissions';
GO

-- =============================================
-- 3. SEED ADMIN USER
-- Password: Admin@123 (BCrypt hash)
-- =============================================
PRINT N'Seeding Admin User...';

DECLARE @AdminUserId INT;
DECLARE @AdminRoleId INT;

-- Tao user admin
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('admin', 'admin@erp.com', '$2a$11$rBNPE0Ebc5sXb0e.yNHOEeBs4yXP1gqpv4deyJPmvmFxuQ2TJP/Pu', 1, 0, GETDATE(), 0, GETDATE(), 0);
    
    SET @AdminUserId = SCOPE_IDENTITY();
    PRINT N'Da tao user admin voi Id: ' + CAST(@AdminUserId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @AdminUserId = Id FROM Users WHERE Username = 'admin';
    PRINT N'User admin da ton tai voi Id: ' + CAST(@AdminUserId AS NVARCHAR(10));
END

-- Tao staff cho admin
IF NOT EXISTS (SELECT 1 FROM Staffs WHERE User_id = @AdminUserId)
BEGIN
    INSERT INTO Staffs (Staff_code, Full_name, Phone_number, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ADMIN001', N'Quản trị viên', '0900000000', @AdminUserId, 0, GETDATE(), 0, GETDATE(), 0);
    PRINT N'Da tao Staff cho admin';
END

-- Gan role ROLE_ADMIN cho user admin
SELECT @AdminRoleId = Id FROM Roles WHERE Role_name = 'ROLE_ADMIN';

IF @AdminRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE User_id = @AdminUserId AND Role_id = @AdminRoleId AND Is_deleted = 0)
BEGIN
    INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@AdminUserId, @AdminRoleId, 0, GETDATE(), 0, GETDATE(), 0);
    PRINT N'Da gan role ROLE_ADMIN cho admin';
END

-- Gan tat ca permissions cho ROLE_ADMIN
INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
SELECT @AdminRoleId, p.Id, 0, GETDATE(), 0, GETDATE(), 0
FROM Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp 
    WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.Id AND rp.Is_deleted = 0
);

PRINT N'Da gan tat ca permissions cho ROLE_ADMIN';
GO

-- =============================================
-- 4. SEED MASTER DATA
-- =============================================
PRINT N'Seeding Master Data...';

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

-- Provinces
DECLARE @RegionMB INT, @RegionMT INT, @RegionMN INT;
SELECT @RegionMB = Id FROM Regions WHERE Region_code = 'MB';
SELECT @RegionMT = Id FROM Regions WHERE Region_code = 'MT';
SELECT @RegionMN = Id FROM Regions WHERE Region_code = 'MN';

IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HN', N'Hà Nội', @RegionMB, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HCM')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HCM', N'TP. Hồ Chí Minh', @RegionMN, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN', N'Đà Nẵng', @RegionMT, 0, GETDATE(), 0, GETDATE(), 0);

-- Categories (chi sach)
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

PRINT N'Da seed Master Data';
GO

-- =============================================
-- 5. SEED SAMPLE PRODUCTS
-- =============================================
PRINT N'Seeding Sample Products...';

DECLARE @PublisherId1 INT, @PublisherId2 INT;
DECLARE @CoverTypeId1 INT, @CoverTypeId2 INT;
DECLARE @DistributorId1 INT;
DECLARE @CategoryId1 INT;
DECLARE @AuthorId1 INT, @AuthorId2 INT;

SELECT @PublisherId1 = Id FROM Publishers WHERE Publisher_code = 'NXB001';
SELECT @PublisherId2 = Id FROM Publishers WHERE Publisher_code = 'NXB002';
SELECT @CoverTypeId1 = Id FROM CoverTypes WHERE Cover_type_code = 'BC';
SELECT @CoverTypeId2 = Id FROM CoverTypes WHERE Cover_type_code = 'BM';
SELECT @DistributorId1 = Id FROM Distributors WHERE Distributor_code = 'NPP001';
SELECT @CategoryId1 = Id FROM Categories WHERE Category_code = 'SACH';
SELECT @AuthorId1 = Id FROM Authors WHERE Author_code = 'TG001';
SELECT @AuthorId2 = Id FROM Authors WHERE Author_code = 'TG002';

-- Products
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP001')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP001', N'Tôi thấy hoa vàng trên cỏ xanh', '85000', N'Truyện dài của Nguyễn Nhật Ánh', 10, @CoverTypeId2, @PublisherId1, @DistributorId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    DECLARE @ProductId1 INT = SCOPE_IDENTITY();
    
    -- Product Category
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId1, @CategoryId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    -- Product Author
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId1, @AuthorId1, 0, GETDATE(), 0, GETDATE(), 0);
    -- Product Image
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId1, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP002')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP002', N'Dế Mèn phiêu lưu ký', '55000', N'Tác phẩm nổi tiếng của Tô Hoài', 10, @CoverTypeId1, @PublisherId1, @DistributorId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    DECLARE @ProductId2 INT = SCOPE_IDENTITY();
    
    -- Product Category
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId2, @CategoryId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    -- Product Author
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId2, @AuthorId2, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId2, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP003')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP003', N'Mắt biếc', '75000', N'Truyện tình cảm của Nguyễn Nhật Ánh', 10, @CoverTypeId2, @PublisherId2, @DistributorId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    DECLARE @ProductId3 INT = SCOPE_IDENTITY();
    
    -- Product Category
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId3, @CategoryId1, 0, GETDATE(), 0, GETDATE(), 0);
    
    -- Product Author
    INSERT INTO ProductAuthors (Product_id, Author_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId3, @AuthorId1, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@ProductId3, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

PRINT N'Da seed Sample Products (sach)';
GO

-- =============================================
-- 6. PERMISSIONS BO SUNG (Organisation, Distributor, Product Import, Customer Assign)
-- =============================================
PRINT N'Seeding them Permissions...';

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'PRODUCT_IMPORT')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PRODUCT_IMPORT', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'CUSTOMER_ASSIGN')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUSTOMER_ASSIGN', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_VIEW', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_CREATE', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'ORGANIZATION_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('ORGANIZATION_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_VIEW', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_CREATE', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'DISTRIBUTOR_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DISTRIBUTOR_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'Da seed them Permissions';
GO

-- Gan lai tat ca permissions cho ROLE_ADMIN (bao gom permission moi)
DECLARE @AdminRoleId2 INT;
SELECT @AdminRoleId2 = Id FROM Roles WHERE Role_name = 'ROLE_ADMIN';
INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
SELECT @AdminRoleId2, p.Id, 0, GETDATE(), 0, GETDATE(), 0
FROM Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @AdminRoleId2 AND rp.PermissionId = p.Id AND rp.Is_deleted = 0
);
GO

-- =============================================
-- 7. USER KHACH HANG MAU
-- Password: Customer@123 (BCrypt)
-- =============================================
PRINT N'Seeding User Khach hang mau...';

DECLARE @CustomerUserId INT;
DECLARE @CustomerRoleId INT;
DECLARE @CustomerId INT;

IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'khachhang')
BEGIN
    INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('khachhang', 'khachhang@example.com', '$2a$11$K5xJ8VqL5nq1qkPH3BQ3/.5Ik8.0s5z5F5z5F5z5F5z5F5z5F5z5F', 1, 0, GETDATE(), 0, GETDATE(), 0);
    SET @CustomerUserId = SCOPE_IDENTITY();
    PRINT N'Da tao user khachhang voi Id: ' + CAST(@CustomerUserId AS NVARCHAR(10));
END
ELSE
    SELECT @CustomerUserId = Id FROM Users WHERE Username = 'khachhang';

IF NOT EXISTS (SELECT 1 FROM Customers WHERE User_id = @CustomerUserId)
BEGIN
    INSERT INTO Customers (Customer_code, Full_name, Phone_number, Address, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('CUST-SEED01', N'Khách hàng mẫu', '0900000001', N'Địa chỉ mẫu, Quận 1', @CustomerUserId, 0, GETDATE(), 0, GETDATE(), 0);
    SET @CustomerId = SCOPE_IDENTITY();
    PRINT N'Da tao Customer voi Id: ' + CAST(@CustomerId AS NVARCHAR(10));
END
ELSE
    SELECT @CustomerId = Id FROM Customers WHERE User_id = @CustomerUserId;

SELECT @CustomerRoleId = Id FROM Roles WHERE Role_name = 'ROLE_CUSTOMER';
IF @CustomerRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE User_id = @CustomerUserId AND Role_id = @CustomerRoleId AND Is_deleted = 0)
BEGIN
    INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@CustomerUserId, @CustomerRoleId, 0, GETDATE(), 0, GETDATE(), 0);
    PRINT N'Da gan role ROLE_CUSTOMER cho khachhang';
END
GO

-- =============================================
-- 8. THEM KHU VUC VA TINH THANH
-- =============================================
PRINT N'Seeding them Region + Provinces...';

IF NOT EXISTS (SELECT 1 FROM Regions WHERE Region_code = 'TN')
    INSERT INTO Regions (Region_code, Region_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TN', N'Tây Nguyên', 0, GETDATE(), 0, GETDATE(), 0);

DECLARE @RegionMB2 INT, @RegionMT2 INT, @RegionMN2 INT, @RegionTN INT;
SELECT @RegionMB2 = Id FROM Regions WHERE Region_code = 'MB';
SELECT @RegionMT2 = Id FROM Regions WHERE Region_code = 'MT';
SELECT @RegionMN2 = Id FROM Regions WHERE Region_code = 'MN';
SELECT @RegionTN = Id FROM Regions WHERE Region_code = 'TN';

IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'HP')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HP', N'Hải Phòng', @RegionMB2, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'BN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BN', N'Bắc Ninh', @RegionMB2, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'QN')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('QN', N'Quảng Ninh', @RegionMB2, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'BD')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('BD', N'Bình Dương', @RegionMN2, 0, GETDATE(), 0, GETDATE(), 0);
IF NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DN2')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DN2', N'Đồng Nai', @RegionMN2, 0, GETDATE(), 0, GETDATE(), 0);
IF @RegionTN IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Provinces WHERE Province_code = 'DL')
    INSERT INTO Provinces (Province_code, Province_name, Region_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DL', N'Đắk Lắk', @RegionTN, 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'Da seed them Provinces';
GO

-- =============================================
-- 9. THEM TAC GIA, NHA XUAT BAN (phuc vu sach)
-- =============================================
PRINT N'Seeding them Authors, Publishers...';

IF NOT EXISTS (SELECT 1 FROM Authors WHERE Author_code = 'TG003')
    INSERT INTO Authors (Author_code, Author_name, Pen_name, Nationality, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('TG003', N'Paulo Coelho', N'Paulo Coelho', N'Brazil', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Publishers WHERE Publisher_code = 'NXB003')
    INSERT INTO Publishers (Publisher_code, Publisher_name, Publisher_address, Publisher_phone, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('NXB003', N'NXB Văn học', N'Hà Nội', '0243333333', 0, GETDATE(), 0, GETDATE(), 0);

PRINT N'Da seed them Categories/Authors/Publishers';
GO

-- =============================================
-- 10. THEM SAN PHAM SACH MAU (SP006) + ProductImages
-- =============================================
PRINT N'Seeding them Sample Products (sach)...';

DECLARE @PubId INT, @CoverId INT, @DistId INT, @CatSach INT;

SELECT @PubId = Id FROM Publishers WHERE Publisher_code = 'NXB002';
SELECT @CoverId = Id FROM CoverTypes WHERE Cover_type_code = 'BM';
SELECT @DistId = Id FROM Distributors WHERE Distributor_code = 'NPP001';
SELECT @CatSach = Id FROM Categories WHERE Category_code = 'SACH';

-- SP006 - Sach them
IF NOT EXISTS (SELECT 1 FROM Products WHERE Product_code = 'SP006')
BEGIN
    INSERT INTO Products (Product_code, Product_name, Product_price, Product_description, Tax_rate, Cover_type_id, Publisher_id, Distributor_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SP006', N'Cây cam ngọt của tôi', '72000', N'Truyện thiếu nhi của José Mauro de Vasconcelos', 10, @CoverId, @PubId, @DistId, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @P6 INT = SCOPE_IDENTITY();
    INSERT INTO ProductCategories (Product_id, Category_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES (@P6, @CatSach, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO ProductImages (Product_id, Image_url, Is_main, Sort_order, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@P6, '/shop/images/homepage-one/category-img/dresses.webp', 1, 0, 0, GETDATE(), 0, GETDATE(), 0);
END

PRINT N'Da seed them Sample Products (sach)';
GO

-- =============================================
-- 11. DON HANG MAU (cho khach hang)
-- =============================================
PRINT N'Seeding Don hang mau...';

DECLARE @CustId INT;
DECLARE @ProdId1 INT, @ProdId2 INT;
DECLARE @OrderId1 INT;

SELECT @CustId = Id FROM Customers c INNER JOIN Users u ON c.User_id = u.Id WHERE u.Username = 'khachhang';
SELECT @ProdId1 = Id FROM Products WHERE Product_code = 'SP001';
SELECT @ProdId2 = Id FROM Products WHERE Product_code = 'SP003';

IF @CustId IS NOT NULL AND @ProdId1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Orders WHERE Order_code = 'DH-SEED01')
BEGIN
    INSERT INTO Orders (Order_code, Order_date, Total_amount, Total_price, Order_status, Shipping_address, note, Customer_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DH-SEED01', DATEADD(day, -3, GETDATE()), 2, 160000, N'Pending', N'123 Nguyen Hue, Q1, TP.HCM', N'Don mau seed', @CustId, 0, GETDATE(), 0, GETDATE(), 0);
    SET @OrderId1 = SCOPE_IDENTITY();

    INSERT INTO OrderDetails (Order_id, Product_id, Quantity, Unit_price, Total_price, Tax_rate, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@OrderId1, @ProdId1, 1, 85000, 85000, 10, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO OrderDetails (Order_id, Product_id, Quantity, Unit_price, Total_price, Tax_rate, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@OrderId1, @ProdId2, 1, 75000, 75000, 10, 0, GETDATE(), 0, GETDATE(), 0);

    PRINT N'Da tao don hang mau DH-SEED01';
END

IF @CustId IS NOT NULL AND @ProdId1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Orders WHERE Order_code = 'DH-SEED02')
BEGIN
    INSERT INTO Orders (Order_code, Order_date, Total_amount, Total_price, Order_status, Shipping_address, note, Customer_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('DH-SEED02', DATEADD(day, -1, GETDATE()), 1, 55000, N'Confirmed', N'456 Le Loi, Q3, TP.HCM', N'', @CustId, 0, GETDATE(), 0, GETDATE(), 0);
    DECLARE @OrderId2 INT = SCOPE_IDENTITY();
    INSERT INTO OrderDetails (Order_id, Product_id, Quantity, Unit_price, Total_price, Tax_rate, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@OrderId2, (SELECT Id FROM Products WHERE Product_code = 'SP002'), 1, 55000, 55000, 10, 0, GETDATE(), 0, GETDATE(), 0);
    PRINT N'Da tao don hang mau DH-SEED02';
END
GO

-- =============================================
-- 12. HOA DON MAU (theo don hang DH-SEED01)
-- =============================================
PRINT N'Seeding Hoa don mau...';

DECLARE @OrderIdInv INT, @StaffIdInv INT, @CustIdInv INT, @InvoiceIdSeed INT;

SELECT @OrderIdInv = Id FROM Orders WHERE Order_code = 'DH-SEED01';
SELECT @StaffIdInv = s.Id FROM Staffs s INNER JOIN Users u ON s.User_id = u.Id WHERE u.Username = 'admin';
SELECT @CustIdInv = c.Id FROM Customers c INNER JOIN Users u ON c.User_id = u.Id WHERE u.Username = 'khachhang';

IF @OrderIdInv IS NOT NULL AND @StaffIdInv IS NOT NULL AND @CustIdInv IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Invoices WHERE Invoice_code = 'HD-SEED01')
BEGIN
    -- Tong 160000, thue 10% ~ 14545 (lam tron), Tax_amount co the = 0 hoac tinh theo don
    INSERT INTO Invoices (Invoice_code, Invoice_date, Customer_id, Staff_id, Order_id, Warehouse_export_id, Total_amount, Tax_amount, Status, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('HD-SEED01', DATEADD(day, -2, GETDATE()), @CustIdInv, @StaffIdInv, @OrderIdInv, NULL, 160000, 16000, N'Confirmed', 0, GETDATE(), 0, GETDATE(), 0);
    SET @InvoiceIdSeed = SCOPE_IDENTITY();

    INSERT INTO InvoiceDetails (Invoice_id, Product_id, Quantity, Unit_price, Total_price, Tax_rate, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@InvoiceIdSeed, (SELECT Id FROM Products WHERE Product_code = 'SP001'), 1, 85000, 85000, 10, 0, GETDATE(), 0, GETDATE(), 0);
    INSERT INTO InvoiceDetails (Invoice_id, Product_id, Quantity, Unit_price, Total_price, Tax_rate, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES (@InvoiceIdSeed, (SELECT Id FROM Products WHERE Product_code = 'SP003'), 1, 75000, 75000, 10, 0, GETDATE(), 0, GETDATE(), 0);

    PRINT N'Da tao hoa don HD-SEED01';
END
GO

-- =============================================
-- 13. PHIEU XUAT KHO MAU (theo hoa don HD-SEED01)
-- =============================================
PRINT N'Seeding Phieu xuat kho mau...';

DECLARE @InvIdPX INT, @WhIdPX INT, @StaffIdPX INT, @CustIdPX INT, @OrderIdPX INT, @ExportIdSeed INT;

SELECT @InvIdPX = Id FROM Invoices WHERE Invoice_code = 'HD-SEED01';
SELECT @WhIdPX = Id FROM Warehouses WHERE Warehouse_code = 'KHO02';
SELECT @StaffIdPX = s.Id FROM Staffs s INNER JOIN Users u ON s.User_id = u.Id WHERE u.Username = 'admin';
SELECT @CustIdPX = c.Id FROM Customers c INNER JOIN Users u ON c.User_id = u.Id WHERE u.Username = 'khachhang';
SELECT @OrderIdPX = Id FROM Orders WHERE Order_code = 'DH-SEED01';

IF @InvIdPX IS NOT NULL AND @WhIdPX IS NOT NULL AND @StaffIdPX IS NOT NULL AND @CustIdPX IS NOT NULL AND NOT EXISTS (SELECT 1 FROM WarehouseExports WHERE Warehouse_export_code = 'PX-SEED01')
BEGIN
    INSERT INTO WarehouseExports (Warehouse_export_code, Warehouse_id, Order_id, Invoice_id, Staff_id, Customer_id, Export_date, Carrier_name, Tracking_number, Delivery_status, Status, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('PX-SEED01', @WhIdPX, @OrderIdPX, @InvIdPX, @StaffIdPX, @CustIdPX, DATEADD(day, -2, GETDATE()), N'Giao hang nhanh', N'GHN001', N'Shipping', N'Confirmed', 0, GETDATE(), 0, GETDATE(), 0);
    SET @ExportIdSeed = SCOPE_IDENTITY();

    INSERT INTO WarehouseExportDetails (Warehouse_export_id, Warehouse_id, Product_id, Quantity_shipped, Unit_price, Total_price, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT @ExportIdSeed, @WhIdPX, Id, 1, 85000, 85000, 0, GETDATE(), 0, GETDATE(), 0 FROM Products WHERE Product_code = 'SP001';
    INSERT INTO WarehouseExportDetails (Warehouse_export_id, Warehouse_id, Product_id, Quantity_shipped, Unit_price, Total_price, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT @ExportIdSeed, @WhIdPX, Id, 1, 75000, 75000, 0, GETDATE(), 0, GETDATE(), 0 FROM Products WHERE Product_code = 'SP003';

    PRINT N'Da tao phieu xuat kho PX-SEED01';
END
GO

PRINT N'';
PRINT N'========================================';
PRINT N'HOAN THANH SEED DATA!';
PRINT N'';
PRINT N'Dang nhap Admin:';
PRINT N'  Username: admin';
PRINT N'  Password: Admin@123';
PRINT N'';
PRINT N'Dang nhap Khach hang:';
PRINT N'  Username: khachhang';
PRINT N'  Password: Customer@123 (can hash BCrypt that trong DB)';
PRINT N'========================================';
GO
