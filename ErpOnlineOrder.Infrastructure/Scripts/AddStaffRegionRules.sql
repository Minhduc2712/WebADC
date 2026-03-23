-- =============================================
-- Script: AddStaffRegionRules.sql
-- Muc dich: Tao bang StaffRegionRules de cau hinh
--           san can bo phu trach theo vung dia ly
--           (tinh/thanh + phuong/xa tuy chon).
--           Bang nay doc lap voi CustomerManagements:
--           CustomerManagements = phan cong thuc te (sau khi KH dang ky)
--           StaffRegionRules    = quy tac dat san (truoc khi KH dang ky)
-- Chay tren: ErpOnlineOrderDb
-- =============================================

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 1. TAO BANG StaffRegionRules (neu chua co)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StaffRegionRules')
BEGIN
    CREATE TABLE StaffRegionRules (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Staff_id    INT           NOT NULL,
        Province_id INT           NOT NULL,
        Ward_id     INT           NULL,       -- NULL = phu trach toan tinh
        Created_by  INT           NOT NULL DEFAULT 0,
        Created_at  DATETIME2     NOT NULL DEFAULT GETDATE(),
        Updated_by  INT           NOT NULL DEFAULT 0,
        Updated_at  DATETIME2     NOT NULL DEFAULT GETDATE(),
        Is_deleted  BIT           NOT NULL DEFAULT 0,

        CONSTRAINT FK_StaffRegionRules_Staffs
            FOREIGN KEY (Staff_id) REFERENCES Staffs(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_StaffRegionRules_Provinces
            FOREIGN KEY (Province_id) REFERENCES Provinces(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_StaffRegionRules_Wards
            FOREIGN KEY (Ward_id) REFERENCES Wards(Id) ON DELETE NO ACTION
    );

    -- Unique index: moi (tinh + phuong) chi co 1 quy tac (trong cac ban ghi chua xoa)
    CREATE UNIQUE INDEX UIX_StaffRegionRules_Province_Ward
        ON StaffRegionRules (Province_id, Ward_id)
        WHERE Is_deleted = 0;

    -- Index ho tro tra cuu theo can bo
    CREATE INDEX IX_StaffRegionRules_Staff_id
        ON StaffRegionRules (Staff_id);

    PRINT N'[OK] Da tao bang StaffRegionRules';
END
ELSE
    PRINT N'[SKIP] Bang StaffRegionRules da ton tai';
GO
