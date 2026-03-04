-- =============================================
-- Script: AddSettingsPermissions.sql
-- Description: Them quyen SETTINGS, WAREHOUSE_EXPORT va gan cho ROLE_ADMIN
-- Chay script nay neu DB da co san va chua co quyen Cai dat / Xuất kho
-- =============================================

USE ErpOnlineOrderDb;
GO

PRINT N'Them quyen SETTINGS va WAREHOUSE_EXPORT...';

-- ===== SETTINGS (Cai dat he thong) =====
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS' AND (Parent_id IS NULL OR Parent_id = 0))
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'SETTINGS_VIEW', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'SETTINGS' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'SETTINGS_UPDATE', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'SETTINGS' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'SETTINGS_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('SETTINGS_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

UPDATE Permissions SET Parent_id = (SELECT Id FROM Permissions WHERE Permission_code = 'SETTINGS' AND Parent_id IS NULL)
WHERE Permission_code IN ('SETTINGS_VIEW', 'SETTINGS_UPDATE') AND (Parent_id IS NULL OR Parent_id = 0);

-- ===== WAREHOUSE_EXPORT (Quan li xuat kho / Phieu xuat kho) =====
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT' AND (Parent_id IS NULL OR Parent_id = 0))
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT', NULL, 0, 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_VIEW')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'WAREHOUSE_EXPORT_VIEW', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'WAREHOUSE_EXPORT' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_VIEW')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_VIEW', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_CREATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'WAREHOUSE_EXPORT_CREATE', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'WAREHOUSE_EXPORT' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_CREATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_CREATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_UPDATE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'WAREHOUSE_EXPORT_UPDATE', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'WAREHOUSE_EXPORT' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_UPDATE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_UPDATE', 0, GETDATE(), 0, GETDATE(), 0);

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_DELETE')
    INSERT INTO Permissions (Permission_code, Parent_id, Is_special, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT 'WAREHOUSE_EXPORT_DELETE', p.Id, 0, 0, GETDATE(), 0, GETDATE(), 0
    FROM Permissions p WHERE p.Permission_code = 'WAREHOUSE_EXPORT' AND p.Parent_id IS NULL;

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT_DELETE')
    INSERT INTO Permissions (Permission_code, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    VALUES ('WAREHOUSE_EXPORT_DELETE', 0, GETDATE(), 0, GETDATE(), 0);

UPDATE Permissions SET Parent_id = (SELECT Id FROM Permissions WHERE Permission_code = 'WAREHOUSE_EXPORT' AND Parent_id IS NULL)
WHERE Permission_code IN ('WAREHOUSE_EXPORT_VIEW', 'WAREHOUSE_EXPORT_CREATE', 'WAREHOUSE_EXPORT_UPDATE', 'WAREHOUSE_EXPORT_DELETE') AND (Parent_id IS NULL OR Parent_id = 0);

-- Gan quyen cho ROLE_ADMIN
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Role_name = 'ROLE_ADMIN' AND Is_deleted = 0);
IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
    SELECT @AdminRoleId, p.Id, 1, GETDATE(), 1, GETDATE(), 0
    FROM Permissions p
    WHERE p.Permission_code IN ('SETTINGS_VIEW', 'SETTINGS_UPDATE', 'WAREHOUSE_EXPORT_VIEW', 'WAREHOUSE_EXPORT_CREATE', 'WAREHOUSE_EXPORT_UPDATE', 'WAREHOUSE_EXPORT_DELETE')
      AND p.Is_deleted = 0
      AND NOT EXISTS (SELECT 1 FROM RolePermissions rp WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.Id AND rp.Is_deleted = 0);
    PRINT N'Da gan quyen SETTINGS cho ROLE_ADMIN';
END

PRINT N'Hoan thanh.';
GO
