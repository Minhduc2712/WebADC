-- =============================================
-- Script: AddWardsToExistingDb.sql
-- Muc dich: Them bang Wards va cot Ward_id vao
--           database dang chay san (khong dung EF migration)
-- Chay tren: ErpOnlineOrderDb
-- =============================================

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. TAO BANG Wards (neu chua co)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Wards')
BEGIN
    CREATE TABLE Wards (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Ward_code   NVARCHAR(20)  NOT NULL,
        Ward_name   NVARCHAR(100) NOT NULL,
        Province_id INT           NOT NULL,
        Created_by  INT           NOT NULL DEFAULT 0,
        Created_at  DATETIME2     NOT NULL DEFAULT GETDATE(),
        Updated_by  INT           NOT NULL DEFAULT 0,
        Updated_at  DATETIME2     NOT NULL DEFAULT GETDATE(),
        Is_deleted  BIT           NOT NULL DEFAULT 0,
        CONSTRAINT FK_Wards_Provinces FOREIGN KEY (Province_id)
            REFERENCES Provinces(Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_Wards_Province_id ON Wards (Province_id);

    PRINT N'[OK] Da tao bang Wards';
END
ELSE
    PRINT N'[SKIP] Bang Wards da ton tai';
GO

-- =============================================
-- 2. THEM COT Ward_id VAO CustomerManagements
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('CustomerManagements') AND name = 'Ward_id'
)
BEGIN
    ALTER TABLE CustomerManagements
    ADD Ward_id INT NULL;

    -- FK
    ALTER TABLE CustomerManagements
    ADD CONSTRAINT FK_CustomerManagements_Wards
        FOREIGN KEY (Ward_id) REFERENCES Wards(Id) ON DELETE NO ACTION;

    -- Index
    CREATE INDEX IX_CustomerManagements_Ward_id ON CustomerManagements (Ward_id);

    PRINT N'[OK] Da them cot Ward_id vao CustomerManagements';
END
ELSE
    PRINT N'[SKIP] Cot Ward_id da ton tai';
GO

-- =============================================
-- 3. THEM __EFMigrationsHistory (danh dau migration)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId]    nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32)  NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT N'[OK] Da tao bang __EFMigrationsHistory';
END
GO

-- Danh dau tat ca migration cu la da chay (vi DB duoc tao tu script thu cong)
DECLARE @ver NVARCHAR(32) = '8.0.0';

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260209104150_FixProvinceMapping')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260209104150_FixProvinceMapping', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260212100000_RemoveModuleActionAddParentId')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260212100000_RemoveModuleActionAddParentId', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260304120000_RemoveCustomerCategory')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260304120000_RemoveCustomerCategory', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260305094445_ConvertProductPriceToDecimal')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260305094445_ConvertProductPriceToDecimal', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260314093000_AddArrivalDateToWarehouseExport')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260314093000_AddArrivalDateToWarehouseExport', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260319132702_MakeWarehouseExportInvoiceOptional')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260319132702_MakeWarehouseExportInvoiceOptional', @ver);

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260322100000_AddWardsTableAndWardId')
    INSERT INTO [__EFMigrationsHistory] VALUES ('20260322100000_AddWardsTableAndWardId', @ver);

PRINT N'[OK] Da cap nhat __EFMigrationsHistory';
GO

-- =============================================
-- 4. THEM WARD PERMISSIONS (neu chua co)
-- =============================================
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

-- Cap quyen WARD cho ROLE_ADMIN
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = Id FROM Roles WHERE Role_name = 'ROLE_ADMIN';

INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
SELECT @AdminRoleId, p.Id, 0, GETDATE(), 0, GETDATE(), 0
FROM Permissions p
WHERE p.Permission_code IN ('WARD_VIEW','WARD_CREATE','WARD_UPDATE','WARD_DELETE')
  AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.Id AND rp.Is_deleted = 0
  );

PRINT N'[OK] Da them Ward permissions';
GO

PRINT N'=== Hoan tat! Wards da duoc them vao database ===';
GO
