-- =============================================
-- Script: CreateDatabase.sql
-- Description: Tao database va tat ca cac bang cho ErpOnlineOrder
-- Synchronized with Domain Models + DbContext Fluent API
-- =============================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ErpOnlineOrderDb')
BEGIN
    CREATE DATABASE ErpOnlineOrderDb;
    PRINT N'Database ErpOnlineOrderDb da duoc tao!';
END
ELSE
    PRINT N'Database ErpOnlineOrderDb da ton tai.';
GO

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. NGUOI DUNG VA PHAN QUYEN
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Password NVARCHAR(MAX) NOT NULL,
    Is_active BIT NOT NULL DEFAULT 1,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Role_name NVARCHAR(50) NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
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
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
CREATE TABLE Permissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Permission_code NVARCHAR(100) NOT NULL,
    Parent_id INT NULL,
    Is_special BIT NOT NULL DEFAULT 0,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Permissions_Parent FOREIGN KEY (Parent_id) REFERENCES Permissions(Id) ON DELETE NO ACTION
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
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
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPermissions')
CREATE TABLE UserPermissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    PermissionId INT NOT NULL,
    GrantedBy INT NULL,
    GrantedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    ExpiresAt DATETIME2 NULL,
    Note NVARCHAR(500) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_UserPermissions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_UserPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id),
    CONSTRAINT UQ_UserPermissions_User_Permission UNIQUE (UserId, PermissionId)
);
GO

-- =============================================
-- 2. NHAN VIEN VA KHACH HANG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Staffs')
CREATE TABLE Staffs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Staff_code NVARCHAR(50) NOT NULL,
    Full_name NVARCHAR(100) NULL,
    Phone_number NVARCHAR(20) NULL,
    User_id INT NOT NULL UNIQUE,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Staffs_Users FOREIGN KEY (User_id) REFERENCES Users(Id)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Customer_code NVARCHAR(50) NOT NULL,
    Full_name NVARCHAR(100) NULL,
    Phone_number NVARCHAR(20) NULL,
    Address NVARCHAR(200) NULL,
    User_id INT NOT NULL UNIQUE,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Customers_Users FOREIGN KEY (User_id) REFERENCES Users(Id)
);
GO

-- =============================================
-- 3. DIA LY
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Regions')
CREATE TABLE Regions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Region_code NVARCHAR(20) NOT NULL,
    Region_name NVARCHAR(100) NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Provinces')
CREATE TABLE Provinces (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Province_code NVARCHAR(20) NOT NULL,
    Province_name NVARCHAR(100) NOT NULL,
    Region_id INT NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Provinces_Regions FOREIGN KEY (Region_id) REFERENCES Regions(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 4. DANH MUC SAN PHAM
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Category_code NVARCHAR(50) NOT NULL,
    Category_name NVARCHAR(100) NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Authors')
CREATE TABLE Authors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Author_code NVARCHAR(50) NOT NULL,
    Author_name NVARCHAR(200) NOT NULL,
    Pen_name NVARCHAR(200) NULL,
    Email_author NVARCHAR(100) NULL,
    Phone_number NVARCHAR(20) NULL,
    birth_date NVARCHAR(50) NULL,
    death_date NVARCHAR(50) NULL,
    Nationality NVARCHAR(100) NULL,
    Biography NVARCHAR(MAX) NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Publishers')
CREATE TABLE Publishers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Publisher_code NVARCHAR(50) NOT NULL,
    Publisher_name NVARCHAR(200) NOT NULL,
    Publisher_address NVARCHAR(500) NULL,
    Publisher_phone NVARCHAR(20) NULL,
    Publisher_email NVARCHAR(100) NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CoverTypes')
CREATE TABLE CoverTypes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Cover_type_code NVARCHAR(50) NOT NULL,
    Cover_type_name NVARCHAR(100) NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Distributors')
CREATE TABLE Distributors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Distributor_code NVARCHAR(50) NOT NULL,
    Distributor_name NVARCHAR(200) NOT NULL,
    Distributor_address NVARCHAR(500) NULL,
    Distributor_phone NVARCHAR(20) NULL,
    Distributor_email NVARCHAR(100) NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Product_code NVARCHAR(50) NOT NULL,
    Product_name NVARCHAR(200) NOT NULL,
    Product_price DECIMAL(18,0) NOT NULL DEFAULT 0,
    Product_link NVARCHAR(500) NULL,
    Product_description NVARCHAR(MAX) NULL,
    Tax_rate DECIMAL(5,2) NULL,
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
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductCategories')
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
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductAuthors')
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
GO

-- Column names: Image_url, Is_main (mapped from C# image_url, Is_primary via DbContext)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductImages')
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
GO

-- =============================================
-- 5. KHO HANG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warehouses')
CREATE TABLE Warehouses (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Warehouse_code NVARCHAR(50) NOT NULL,
    Warehouse_name NVARCHAR(200) NOT NULL,
    Warehouse_address NVARCHAR(500) NOT NULL,
    Province_id INT NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Warehouses_Provinces FOREIGN KEY (Province_id) REFERENCES Provinces(Id) ON DELETE NO ACTION
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stocks')
CREATE TABLE Stocks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Quantity INT NOT NULL DEFAULT 0,
    Warehouse_id INT NOT NULL,
    Product_id INT NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Stocks_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Stocks_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 6. DON HANG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Order_code NVARCHAR(50) NOT NULL,
    Order_date DATETIME2 NOT NULL DEFAULT GETDATE(),
    Total_amount INT NOT NULL DEFAULT 0,
    Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Order_status NVARCHAR(20) NULL DEFAULT 'Pending',
    Shipping_address NVARCHAR(500) NULL,
    note NVARCHAR(MAX) NULL,
    Customer_id INT NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Orders_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE NO ACTION
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderDetails')
CREATE TABLE OrderDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Order_id INT NOT NULL,
    Product_id INT NOT NULL,
    Tax_rate DECIMAL(5,2) NOT NULL DEFAULT 0,
    Quantity INT NOT NULL DEFAULT 0,
    Unit_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 7. HOA DON
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
CREATE TABLE Invoices (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Invoice_code NVARCHAR(50) NOT NULL,
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
    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Invoices_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Invoices_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Invoices_Parent FOREIGN KEY (Parent_invoice_id) REFERENCES Invoices(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Invoices_Merged FOREIGN KEY (Merged_into_invoice_id) REFERENCES Invoices(Id) ON DELETE NO ACTION
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InvoiceDetails')
CREATE TABLE InvoiceDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Invoice_id INT NOT NULL,
    Product_id INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 0,
    Unit_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax_rate DECIMAL(5,2) NOT NULL DEFAULT 0,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_InvoiceDetails_Invoices FOREIGN KEY (Invoice_id) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvoiceDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 8. XUAT KHO
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WarehouseExports')
CREATE TABLE WarehouseExports (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Warehouse_export_code NVARCHAR(50) NOT NULL,
    Warehouse_id INT NOT NULL,
    Order_id INT NULL,
    Invoice_id INT NOT NULL,
    Staff_id INT NOT NULL,
    Customer_id INT NOT NULL,
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
    CONSTRAINT FK_WarehouseExports_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Invoices FOREIGN KEY (Invoice_id) REFERENCES Invoices(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Orders FOREIGN KEY (Order_id) REFERENCES Orders(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Parent FOREIGN KEY (Parent_export_id) REFERENCES WarehouseExports(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExports_Merged FOREIGN KEY (Merged_into_export_id) REFERENCES WarehouseExports(Id) ON DELETE NO ACTION
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WarehouseExportDetails')
CREATE TABLE WarehouseExportDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Warehouse_export_id INT NOT NULL,
    Warehouse_id INT NOT NULL,
    Product_id INT NOT NULL,
    Quantity_shipped INT NOT NULL DEFAULT 0,
    Unit_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Total_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_WarehouseExportDetails_Exports FOREIGN KEY (Warehouse_export_id) REFERENCES WarehouseExports(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WarehouseExportDetails_Warehouses FOREIGN KEY (Warehouse_id) REFERENCES Warehouses(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_WarehouseExportDetails_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE NO ACTION
);
GO

-- =============================================
-- 9. QUAN LY KHACH HANG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerProducts')
CREATE TABLE CustomerProducts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Customer_id INT NOT NULL,
    Product_id INT NOT NULL,
    Custom_price DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount_percent DECIMAL(5,2) NULL,
    Max_quantity INT NULL,
    Is_active BIT NOT NULL DEFAULT 1,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_CustomerProducts_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CustomerProducts_Products FOREIGN KEY (Product_id) REFERENCES Products(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_CustomerProducts UNIQUE (Customer_id, Product_id)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerManagements')
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
    CONSTRAINT FK_CustomerManagements_Staffs FOREIGN KEY (Staff_id) REFERENCES Staffs(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CustomerManagements_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_CustomerManagements_Provinces FOREIGN KEY (Province_id) REFERENCES Provinces(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_CustomerManagements UNIQUE (Staff_id, Customer_id)
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrganizationInformations')
CREATE TABLE OrganizationInformations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Organization_code NVARCHAR(50) NOT NULL,
    Organization_name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500) NULL,
    Tax_number INT NOT NULL DEFAULT 0,
    Recipient_name NVARCHAR(100) NULL,
    Recipient_phone INT NOT NULL DEFAULT 0,
    Recipient_address NVARCHAR(500) NULL,
    Customer_id INT NOT NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_OrganizationInformations_Customers FOREIGN KEY (Customer_id) REFERENCES Customers(Id) ON DELETE CASCADE
);
GO

-- =============================================
-- 10. CAI DAT HE THONG
-- =============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemSettings')
CREATE TABLE SystemSettings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL,
    SettingValue NVARCHAR(1000) NOT NULL DEFAULT '',
    Description NVARCHAR(200) NULL,
    Created_by INT NOT NULL DEFAULT 0,
    Created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated_by INT NOT NULL DEFAULT 0,
    Updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    Is_deleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_SystemSettings_Key UNIQUE (SettingKey)
);
GO

PRINT N'=== Da tao xong tat ca 27 bang ===';
GO
