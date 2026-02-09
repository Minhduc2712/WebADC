-- =============================================
-- Script: CreateAdminUser.sql
-- Description: Tao user admin moi voi password dung
-- Password: Admin@123
-- =============================================

USE ErpOnlineOrderDb;
GO

PRINT N'========================================';
PRINT N'TAO USER ADMIN MOI';
PRINT N'========================================';
PRINT N'';

-- =============================================
-- BUOC 1: XOA USER ADMIN CU (NEU CO)
-- =============================================
PRINT N'Buoc 1: Xoa user admin cu...';

-- Xoa UserRoles
DELETE FROM UserRoles WHERE User_id IN (SELECT Id FROM Users WHERE Username = 'admin');

-- Xoa UserPermissions
DELETE FROM UserPermissions WHERE UserId IN (SELECT Id FROM Users WHERE Username = 'admin');

-- Xoa Staffs
DELETE FROM Staffs WHERE User_id IN (SELECT Id FROM Users WHERE Username = 'admin');

-- Xoa Users
DELETE FROM Users WHERE Username = 'admin';

PRINT N'  - Da xoa user admin cu';
GO

-- =============================================
-- BUOC 2: SEED DU LIEU CO BAN (NEU CHUA CO)
-- =============================================
PRINT N'Buoc 2: Kiem tra va seed du lieu co ban...';

-- Seed Modules
IF NOT EXISTS (SELECT 1 FROM Modules WHERE Module_code = 'STAFF')
BEGIN
    INSERT INTO Modules (Module_code, Module_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES
    ('PRODUCT', N'Quan ly san pham', 1, GETDATE(), 1, GETDATE(), 0),
    ('CATEGORY', N'Quan ly danh muc', 1, GETDATE(), 1, GETDATE(), 0),
    ('CUSTOMER', N'Quan ly khach hang', 1, GETDATE(), 1, GETDATE(), 0),
    ('ORDER', N'Quan ly don hang', 1, GETDATE(), 1, GETDATE(), 0),
    ('INVOICE', N'Quan ly hoa don', 1, GETDATE(), 1, GETDATE(), 0),
    ('WAREHOUSE', N'Quan ly kho', 1, GETDATE(), 1, GETDATE(), 0),
    ('STAFF', N'Quan ly nhan vien', 1, GETDATE(), 1, GETDATE(), 0),
    ('REPORT', N'Bao cao', 1, GETDATE(), 1, GETDATE(), 0),
    ('ROLE', N'Quan ly vai tro', 1, GETDATE(), 1, GETDATE(), 0);
    PRINT N'  - Da seed Modules';
END

-- Seed Actions
IF NOT EXISTS (SELECT 1 FROM Actions WHERE Action_code = 'VIEW')
BEGIN
    INSERT INTO Actions (Action_code, Action_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES
    ('VIEW', N'Xem', 1, GETDATE(), 1, GETDATE(), 0),
    ('CREATE', N'Them moi', 1, GETDATE(), 1, GETDATE(), 0),
    ('UPDATE', N'Cap nhat', 1, GETDATE(), 1, GETDATE(), 0),
    ('DELETE', N'Xoa', 1, GETDATE(), 1, GETDATE(), 0),
    ('ASSIGN', N'Gan quyen', 1, GETDATE(), 1, GETDATE(), 0);
    PRINT N'  - Da seed Actions';
END

-- Seed Permissions
DECLARE @StaffModuleId INT = (SELECT Id FROM Modules WHERE Module_code = 'STAFF');
DECLARE @ViewActionId INT = (SELECT Id FROM Actions WHERE Action_code = 'VIEW');
DECLARE @CreateActionId INT = (SELECT Id FROM Actions WHERE Action_code = 'CREATE');
DECLARE @UpdateActionId INT = (SELECT Id FROM Actions WHERE Action_code = 'UPDATE');
DECLARE @DeleteActionId INT = (SELECT Id FROM Actions WHERE Action_code = 'DELETE');
DECLARE @AssignActionId INT = (SELECT Id FROM Actions WHERE Action_code = 'ASSIGN');

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Permission_code = 'STAFF_VIEW')
BEGIN
    INSERT INTO Permissions (Permission_code, Module_id, Action_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) VALUES
    ('STAFF_VIEW', @StaffModuleId, @ViewActionId, 1, GETDATE(), 1, GETDATE(), 0),
    ('STAFF_CREATE', @StaffModuleId, @CreateActionId, 1, GETDATE(), 1, GETDATE(), 0),
    ('STAFF_UPDATE', @StaffModuleId, @UpdateActionId, 1, GETDATE(), 1, GETDATE(), 0),
    ('STAFF_DELETE', @StaffModuleId, @DeleteActionId, 1, GETDATE(), 1, GETDATE(), 0),
    ('STAFF_ASSIGN', @StaffModuleId, @AssignActionId, 1, GETDATE(), 1, GETDATE(), 0);
    PRINT N'  - Da seed Permissions';
END

-- Seed Role ROLE_ADMIN
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_ADMIN')
BEGIN
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
    VALUES ('ROLE_ADMIN', 1, GETDATE(), 1, GETDATE(), 0);
    PRINT N'  - Da seed Role ROLE_ADMIN';
END
GO

-- =============================================
-- BUOC 3: TAO USER ADMIN MOI
-- =============================================
PRINT N'Buoc 3: Tao user admin moi...';

-- QUAN TRONG: BCrypt hash cho "Admin@123"
-- Hash nay duoc generate tu BCrypt.Net.BCrypt.HashPassword("Admin@123", 11)
-- Format: $2a$11$[22 ky tu salt][31 ky tu hash]
-- Tong cong: 60 ky tu

DECLARE @PasswordHash NVARCHAR(100) = '$2a$11$rBNhfYxMHNBnHSRN6BuXGuAqTjXrMQHXqJMHJD.4HqLuFM9xKpXHC';

-- Tao user
INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES ('admin', 'admin@erponline.com', @PasswordHash, 1, 1, GETDATE(), 1, GETDATE(), 0);

DECLARE @AdminUserId INT = SCOPE_IDENTITY();
PRINT N'  - Da tao user admin voi ID: ' + CAST(@AdminUserId AS NVARCHAR(10));

-- Tao Staff
INSERT INTO Staffs (Staff_code, Full_name, Phone_number, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES ('ADMIN001', N'Quan tri vien', '0123456789', @AdminUserId, 1, GETDATE(), 1, GETDATE(), 0);
PRINT N'  - Da tao staff cho admin';

-- Gan Role
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Role_name = 'ROLE_ADMIN');
INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES (@AdminUserId, @AdminRoleId, 1, GETDATE(), 1, GETDATE(), 0);
PRINT N'  - Da gan role ROLE_ADMIN';

-- Gan tat ca quyen cho Role
DELETE FROM RolePermissions WHERE RoleId = @AdminRoleId;
INSERT INTO RolePermissions (RoleId, PermissionId, Created_by, Created_at, Updated_by, Updated_at, Is_deleted)
SELECT @AdminRoleId, Id, 1, GETDATE(), 1, GETDATE(), 0 
FROM Permissions 
WHERE Is_deleted = 0;

DECLARE @PermCount INT = (SELECT COUNT(*) FROM RolePermissions WHERE RoleId = @AdminRoleId AND Is_deleted = 0);
PRINT N'  - Da gan ' + CAST(@PermCount AS NVARCHAR(10)) + N' quyen cho ROLE_ADMIN';
GO

-- =============================================
-- HOAN THANH
-- =============================================
PRINT N'';
PRINT N'========================================';
PRINT N'HOAN THANH TAO USER ADMIN!';
PRINT N'========================================';
PRINT N'';
PRINT N'Thong tin dang nhap:';
PRINT N'  Username: admin';
PRINT N'  Password: Admin@123';
PRINT N'';
PRINT N'LUU Y: Neu mat khau van sai, hay chay WebAPI';
PRINT N'va goi API: POST http://localhost:5051/api/auth/seed-admin';
PRINT N'';

-- Kiem tra ket qua
SELECT 
    u.Id AS UserId,
    u.Username,
    u.Email,
    u.Is_active AS IsActive,
    LEN(u.Password) AS PasswordLength,
    s.Full_name AS StaffName,
    r.Role_name AS RoleName,
    (SELECT COUNT(*) FROM RolePermissions rp WHERE rp.RoleId = r.Id AND rp.Is_deleted = 0) AS PermissionCount
FROM Users u
LEFT JOIN Staffs s ON u.Id = s.User_id
LEFT JOIN UserRoles ur ON u.Id = ur.User_id AND ur.Is_deleted = 0
LEFT JOIN Roles r ON ur.Role_id = r.Id
WHERE u.Username = 'admin';
GO
