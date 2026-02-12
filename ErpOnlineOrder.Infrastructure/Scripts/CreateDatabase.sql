-- =============================================
-- Script: CreateDatabase.sql
-- Description: Tao database va tat ca cac bang cho ErpOnlineOrder
-- Version: 6.0 - Synchronized with DbContext
-- =============================================

USE master;
GO

-- Tao database neu chua ton tai
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ErpOnlineOrderDb')
BEGIN
    CREATE DATABASE ErpOnlineOrderDb;
    PRINT N'Database ErpOnlineOrderDb da duoc tao thanh cong!';
END
ELSE
BEGIN
    PRINT N'Database ErpOnlineOrderDb da ton tai!';
END
GO

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. TAO BANG USERS VA AUTHENTICATION
-- =============================================

-- Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(MAX) NOT NULL,
        Is_active BIT NOT NULL DEFAULT 1,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Users';
END
GO

-- Roles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Role_name NVARCHAR(50) NOT NULL UNIQUE,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Roles';
END
GO

-- UserRoles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        User_id INT NOT NULL,
        Role_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (User_id) REFERENCES Users(Id),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (Role_id) REFERENCES Roles(Id)
    );
    PRINT N'Da tao bang UserRoles';
END
GO

-- Permissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE Permissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Permission_code NVARCHAR(100) NOT NULL UNIQUE,
        Module_id INT NULL,
        Action_id INT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Permissions';
END
GO

-- RolePermissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id),
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
    );
    PRINT N'Da tao bang RolePermissions';
END
GO

-- UserPermissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPermissions')
BEGIN
    CREATE TABLE UserPermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        PermissionId INT NOT NULL,
        GrantedBy INT NULL,
        GrantedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        ExpiresAt DATETIME2 NULL,
        Note NVARCHAR(500) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserPermissions_UserId_PermissionId UNIQUE (UserId, PermissionId)
    );
    PRINT N'Da tao bang UserPermissions';
END
GO

-- =============================================
-- 2. TAO BANG NHAN VIEN VA KHACH HANG
-- =============================================

-- Staffs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Staffs')
BEGIN
    CREATE TABLE Staffs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Staff_code NVARCHAR(50) NOT NULL UNIQUE,
        Full_name NVARCHAR(100) NULL,
        Phone_number NVARCHAR(20) NULL,
        User_id INT NULL UNIQUE,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Staffs_Users FOREIGN KEY (User_id) REFERENCES Users(Id)
    );
    PRINT N'Da tao bang Staffs';
END
GO

-- Customers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_code NVARCHAR(50) NOT NULL UNIQUE,
        Full_name NVARCHAR(100) NULL,
        Phone_number NVARCHAR(20) NULL,
        Address NVARCHAR(200) NULL,
        User_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Customers_Users FOREIGN KEY (User_id) REFERENCES Users(Id)
    );
    PRINT N'Da tao bang Customers';
END
GO

-- =============================================
-- 3. TAO BANG DIA LY
-- =============================================

-- Regions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Regions')
BEGIN
    CREATE TABLE Regions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Region_code NVARCHAR(20) NOT NULL UNIQUE,
        Region_name NVARCHAR(100) NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Regions';
END
GO

-- Provinces table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Provinces')
BEGIN
    CREATE TABLE Provinces (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Province_code NVARCHAR(20) NOT NULL UNIQUE,
        Province_name NVARCHAR(100) NOT NULL,
        Region_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Provinces_Regions FOREIGN KEY (Region_id) REFERENCES Regions(Id)
    );
    PRINT N'Da tao bang Provinces';
END
GO

-- =============================================
-- 4. TAO BANG SAN PHAM
-- =============================================

-- Categories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Category_code NVARCHAR(50) NOT NULL UNIQUE,
        Category_name NVARCHAR(100) NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Categories';
END
GO

-- Authors table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Authors')
BEGIN
    CREATE TABLE Authors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Author_code NVARCHAR(50) NOT NULL,
        Author_name NVARCHAR(100) NOT NULL,
        Pen_name NVARCHAR(100) NULL,
        Biography NVARCHAR(MAX) NULL,
        Email_author NVARCHAR(100) NULL,
        Phone_number NVARCHAR(20) NULL,
        Nationality NVARCHAR(100) NULL,
        birth_date NVARCHAR(50) NULL,
        death_date NVARCHAR(50) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Authors';
END
GO

-- Publishers table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Publishers')
BEGIN
    CREATE TABLE Publishers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Publisher_code NVARCHAR(50) NOT NULL,
        Publisher_name NVARCHAR(100) NOT NULL,
        Publisher_address NVARCHAR(500) NULL,
        Publisher_email NVARCHAR(100) NULL,
        Publisher_phone NVARCHAR(20) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Publishers';
END
GO

-- CoverTypes table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CoverTypes')
BEGIN
    CREATE TABLE CoverTypes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Cover_type_code NVARCHAR(50) NOT NULL,
        Cover_type_name NVARCHAR(50) NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang CoverTypes';
END
GO

-- Distributors table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Distributors')
BEGIN
    CREATE TABLE Distributors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Distributor_code NVARCHAR(50) NOT NULL UNIQUE,
        Distributor_name NVARCHAR(200) NOT NULL,
        Distributor_address NVARCHAR(500) NULL,
        Distributor_email NVARCHAR(100) NULL,
        Distributor_phone NVARCHAR(20) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0
    );
    PRINT N'Da tao bang Distributors';
END
GO

-- Products table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Product_code NVARCHAR(50) NOT NULL UNIQUE,
        Product_name NVARCHAR(200) NOT NULL,
        Product_price NVARCHAR(50) NULL,
        Product_link NVARCHAR(500) NULL,
        Product_description NVARCHAR(MAX) NULL,
        Tax_rate DECIMAL(5,2) NULL DEFAULT 10,
        Cover_type_id INT NULL,
        Publisher_id INT NULL,
        Distributor_id INT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Products_CoverTypes FOREIGN KEY (Cover_type_id) REFERENCES CoverTypes(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Products_Publishers FOREIGN KEY (Publisher_id) REFERENCES Publishers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Products_Distributors FOREIGN KEY (Distributor_id) REFERENCES Distributors(Id) ON DELETE SET NULL
    );
    PRINT N'Da tao bang Products';
END
GO

-- ProductCategories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCategories')
BEGIN
    CREATE TABLE ProductCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Product_id INT NOT NULL,
        Category_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_ProductCategories_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductCategories_Categories FOREIGN KEY (Category_id) REFERENCES Categories(Id) ON DELETE CASCADE
    );
    PRINT N'Da tao bang ProductCategories';
END
GO

-- ProductAuthors table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductAuthors')
BEGIN
    CREATE TABLE ProductAuthors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Product_id INT NOT NULL,
        Author_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_ProductAuthors_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProductAuthors_Authors FOREIGN KEY (Author_id) REFERENCES Authors(Id) ON DELETE CASCADE
    );
    PRINT N'Da tao bang ProductAuthors';
END
GO

-- ProductImages table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductImages')
BEGIN
    CREATE TABLE ProductImages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Product_id INT NOT NULL,
        Image_url NVARCHAR(500) NOT NULL,
        Is_main BIT NOT NULL DEFAULT 0,
        Sort_order INT NOT NULL DEFAULT 0,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_ProductImages_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE CASCADE
    );
    PRINT N'Da tao bang ProductImages';
END
GO

-- =============================================
-- 5. TAO BANG KHO HANG
-- =============================================

-- Warehouses table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warehouses')
BEGIN
    CREATE TABLE Warehouses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Warehouse_code NVARCHAR(50) NOT NULL UNIQUE,
        Warehouse_name NVARCHAR(200) NOT NULL,
        Warehouse_address NVARCHAR(500) NOT NULL,
        Province_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Warehouses_Provinces FOREIGN KEY (Province_id) REFERENCES Provinces(Id)
    );
    PRINT N'Da tao bang Warehouses';
END
GO

-- Stocks table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stocks')
BEGIN
    CREATE TABLE Stocks (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Warehouse_id INT NOT NULL,
        Product_id INT NOT NULL,
        Quantity INT NOT NULL DEFAULT 0,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Stocks_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id),
        CONSTRAINT FK_Stocks_Products FOREIGN KEY (Product_id) REFERENCES Products(Id)
    );
    PRINT N'Da tao bang Stocks';
END
GO

-- =============================================
-- 6. TAO BANG DON HANG
-- =============================================

-- Orders table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Order_code NVARCHAR(50) NOT NULL UNIQUE,
        Order_date DATETIME2 NOT NULL DEFAULT GETDATE(),
        Customer_id INT NOT NULL,
        Total_amount INT NULL DEFAULT 0,
        Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Order_status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        Shipping_address NVARCHAR(500) NULL,
        note NVARCHAR(1000) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Orders_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id)
    );
    PRINT N'Da tao bang Orders';
END
GO

-- OrderDetails table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderDetails')
BEGIN
    CREATE TABLE OrderDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Order_id INT NOT NULL,
        Product_id INT NOT NULL,
        Tax_rate DECIMAL(5,2) NULL DEFAULT 0,
        Quantity INT NOT NULL DEFAULT 0,
        Unit_price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id)
    );
    PRINT N'Da tao bang OrderDetails';
END
GO

-- =============================================
-- 7. TAO BANG HOA DON
-- =============================================

-- Invoices table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE Invoices (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Invoice_code NVARCHAR(50) NOT NULL UNIQUE,
        Invoice_date DATETIME2 NOT NULL,
        Customer_id INT NOT NULL,
        Staff_id INT NOT NULL,
        Order_id INT NULL,
        Warehouse_export_id INT NULL,
        Total_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Tax_amount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
        Parent_invoice_id INT NULL,
        Merged_into_invoice_id INT NULL,
        Split_merge_note NVARCHAR(500) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Invoices_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id),
        CONSTRAINT FK_Invoices_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id),
        CONSTRAINT FK_Invoices_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id),
        CONSTRAINT FK_Invoices_Parent FOREIGN KEY (Parent_invoice_id) REFERENCES Invoices(Id),
        CONSTRAINT FK_Invoices_Merged FOREIGN KEY (Merged_into_invoice_id) REFERENCES Invoices(Id)
    );
    PRINT N'Da tao bang Invoices';
END
GO

-- InvoiceDetails table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceDetails')
BEGIN
    CREATE TABLE InvoiceDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Invoice_id INT NOT NULL,
        Product_id INT NOT NULL,
        Quantity INT NOT NULL,
        Unit_price DECIMAL(18,2) NOT NULL,
        Total_price DECIMAL(18,2) NOT NULL,
        Tax_rate DECIMAL(5,2) NOT NULL DEFAULT 10,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_InvoiceDetails_Invoices FOREIGN KEY (Invoice_id) REFERENCES Invoices(Id) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id)
    );
    PRINT N'Da tao bang InvoiceDetails';
END
GO

-- =============================================
-- 8. TAO BANG XUAT KHO
-- =============================================

-- WarehouseExports table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WarehouseExports')
BEGIN
    CREATE TABLE WarehouseExports (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Warehouse_export_code NVARCHAR(50) NOT NULL UNIQUE,
        Warehouse_id INT NOT NULL,
        Invoice_id INT NOT NULL,
        Order_id INT NULL,
        Customer_id INT NOT NULL,
        Staff_id INT NOT NULL,
        Export_date DATETIME2 NOT NULL,
        Carrier_name NVARCHAR(100) NULL,
        Tracking_number NVARCHAR(100) NULL,
        Delivery_status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        Status NVARCHAR(20) NOT NULL DEFAULT 'Draft',
        Parent_export_id INT NULL,
        Merged_into_export_id INT NULL,
        Split_merge_note NVARCHAR(500) NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_WarehouseExports_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id),
        CONSTRAINT FK_WarehouseExports_Invoices FOREIGN KEY (Invoice_id) REFERENCES Invoices(Id),
        CONSTRAINT FK_WarehouseExports_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id),
        CONSTRAINT FK_WarehouseExports_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id),
        CONSTRAINT FK_WarehouseExports_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id),
        CONSTRAINT FK_WarehouseExports_Parent FOREIGN KEY (Parent_export_id) REFERENCES WarehouseExports(Id),
        CONSTRAINT FK_WarehouseExports_Merged FOREIGN KEY (Merged_into_export_id) REFERENCES WarehouseExports(Id)
    );
    PRINT N'Da tao bang WarehouseExports';
END
GO

-- WarehouseExportDetails table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WarehouseExportDetails')
BEGIN
    CREATE TABLE WarehouseExportDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Warehouse_export_id INT NOT NULL,
        Warehouse_id INT NOT NULL,
        Product_id INT NOT NULL,
        Quantity_shipped INT NOT NULL,
        Unit_price DECIMAL(18,2) NOT NULL,
        Total_price DECIMAL(18,2) NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_WarehouseExportDetails_Exports FOREIGN KEY (Warehouse_export_id) REFERENCES WarehouseExports(Id) ON DELETE CASCADE,
        CONSTRAINT FK_WarehouseExportDetails_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id),
        CONSTRAINT FK_WarehouseExportDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id)
    );
    PRINT N'Da tao bang WarehouseExportDetails';
END
GO

-- =============================================
-- 9. TAO BANG QUAN LY KHACH HANG
-- =============================================

-- CustomerProducts table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerProducts')
BEGIN
    CREATE TABLE CustomerProducts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_id INT NOT NULL,
        Product_id INT NOT NULL,
        Custom_price DECIMAL(18,2) NULL,
        Discount_percent DECIMAL(5,2) NULL,
        Max_quantity INT NULL,
        Is_active BIT NOT NULL DEFAULT 1,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CustomerProducts_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id),
        CONSTRAINT FK_CustomerProducts_Products FOREIGN KEY (Product_id) REFERENCES Products(Id),
        CONSTRAINT UQ_CustomerProducts UNIQUE(Customer_id, Product_id)
    );
    PRINT N'Da tao bang CustomerProducts';
END
GO

-- CustomerCategories table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerCategories')
BEGIN
    CREATE TABLE CustomerCategories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_id INT NOT NULL,
        Category_id INT NOT NULL,
        Discount_percent DECIMAL(5,2) NULL,
        Is_active BIT NOT NULL DEFAULT 1,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CustomerCategories_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id),
        CONSTRAINT FK_CustomerCategories_Categories FOREIGN KEY (Category_id) REFERENCES Categories(Id),
        CONSTRAINT UQ_CustomerCategories UNIQUE(Customer_id, Category_id)
    );
    PRINT N'Da tao bang CustomerCategories';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerManagements')
BEGIN
    CREATE TABLE CustomerManagements (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Staff_id INT NOT NULL,
        Customer_id INT NOT NULL,
        Province_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_CustomerManagements_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id),
        CONSTRAINT FK_CustomerManagements_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id),
        CONSTRAINT FK_CustomerManagements_Provinces FOREIGN KEY (Province_id) REFERENCES Provinces(Id),
        CONSTRAINT UQ_CustomerManagements UNIQUE(Staff_id, Customer_id)
    );
    PRINT N'Da tao bang CustomerManagements';
END
GO

-- OrganizationInformations table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationInformations')
BEGIN
    CREATE TABLE OrganizationInformations (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Organization_code NVARCHAR(50) NOT NULL UNIQUE,
        Organization_name NVARCHAR(200) NOT NULL,
        Address NVARCHAR(500) NULL,
        Tax_number NVARCHAR(50) NULL,
        Recipient_name NVARCHAR(100) NULL,
        Recipient_phone NVARCHAR(20) NULL,
        Recipient_address NVARCHAR(500) NULL,
        Customer_id INT NOT NULL,
        Created_by INT NOT NULL DEFAULT 0,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Is_deleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_OrganizationInformations_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE CASCADE
    );
    PRINT N'Da tao bang OrganizationInformations';
END
GO

-- SystemSettings table (key-value settings)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemSettings')
BEGIN
    CREATE TABLE SystemSettings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SettingKey NVARCHAR(100) NOT NULL UNIQUE,
        SettingValue NVARCHAR(1000) NULL,
        Description NVARCHAR(200) NULL,
        Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        Created_by INT NOT NULL DEFAULT 0,
        Is_deleted BIT NOT NULL DEFAULT 0,
        Updated_by INT NOT NULL DEFAULT 0,
        Updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
    );
    PRINT N'Da tao bang SystemSettings';
END
GO

-- =============================================
-- 10. TAO INDEX
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username')
    CREATE INDEX IX_Users_Username ON Users(Username);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
    CREATE INDEX IX_Users_Email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Products_Product_code')
    CREATE INDEX IX_Products_Product_code ON Products(Product_code);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Order_code')
    CREATE INDEX IX_Orders_Order_code ON Orders(Order_code);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Customer_id')
    CREATE INDEX IX_Orders_Customer_id ON Orders(Customer_id);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Invoices_Invoice_code')
    CREATE INDEX IX_Invoices_Invoice_code ON Invoices(Invoice_code);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserPermissions_UserId')
    CREATE INDEX IX_UserPermissions_UserId ON UserPermissions(UserId);

PRINT N'Da tao cac index';
GO

PRINT N'';
PRINT N'========================================';
PRINT N'HOAN THANH TAO DATABASE VA CAC BANG!';
PRINT N'========================================';
GO
