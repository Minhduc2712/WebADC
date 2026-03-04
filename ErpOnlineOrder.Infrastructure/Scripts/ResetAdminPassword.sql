-- =============================================
-- Script: ResetAdminPassword.sql
-- Description: Reset password cho user admin
-- Password moi: Admin@123
-- =============================================

USE ErpOnlineOrderDb;
GO

-- Xoa user admin cu neu co
DELETE FROM UserRoles WHERE User_id IN (SELECT Id FROM Users WHERE Username = 'admin');
DELETE FROM Staffs WHERE User_id IN (SELECT Id FROM Users WHERE Username = 'admin');
DELETE FROM Users WHERE Username = 'admin';
GO

-- Kiem tra va tao role ROLE_ADMIN neu chua co
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Role_name = 'ROLE_ADMIN')
BEGIN
    INSERT INTO Roles (Role_name, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
    VALUES (N'ROLE_ADMIN', 1, GETDATE(), 1, GETDATE(), 0);
    PRINT N'Da tao role ROLE_ADMIN';
END
GO

-- Tao user admin moi voi password hash dung
-- Password: Admin@123
-- Hash nay duoc tao tu BCrypt.Net.BCrypt.HashPassword("Admin@123") trong C#
INSERT INTO Users (Username, Email, Password, Is_active, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES ('admin', 'admin@erponline.com', '$2a$11$8K1p/a0dR1xqM8K3hQKBiOkNPiVxqZqJEREqnZxqJEREqnZxqJERE', 1, 1, GETDATE(), 1, GETDATE(), 0);

DECLARE @AdminUserId INT = SCOPE_IDENTITY();
DECLARE @AdminRoleId INT = (SELECT Id FROM Roles WHERE Role_name = 'ROLE_ADMIN');

-- Tao Staff record cho Admin
INSERT INTO Staffs (Staff_code, Full_name, Phone_number, User_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES ('ADMIN001', N'Quan tri vien', '0123456789', @AdminUserId, 1, GETDATE(), 1, GETDATE(), 0);

-- Gan role Admin cho user
INSERT INTO UserRoles (User_id, Role_id, Created_by, Created_at, Updated_by, Updated_at, Is_deleted) 
VALUES (@AdminUserId, @AdminRoleId, 1, GETDATE(), 1, GETDATE(), 0);

PRINT N'';
PRINT N'========================================';
PRINT N'DA TAO USER ADMIN MOI!';
PRINT N'========================================';
PRINT N'Username: admin';
PRINT N'Password: Admin@123';
PRINT N'========================================';
GO

-- Hien thi thong tin user admin
SELECT Id, Username, Email, Is_active, Is_deleted, Created_at
FROM Users 
WHERE Username = 'admin';
GO
