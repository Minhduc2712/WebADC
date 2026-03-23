-- =============================================
-- Script: MigrateStaffRegionRulesToJson.sql
-- Muc dich: Chuyen doi bang StaffRegionRules
--           tu mo hinh cu (1 row/ward, co cot Ward_id FK)
--           sang mo hinh moi (1 row/staff+province,
--           Ward_ids luu JSON array e.g. [1,2,3] hoac NULL = toan tinh).
-- Chay tren: ErpOnlineOrderDb
-- CANH BAO: Chay script nay se xoa cot Ward_id va FK constraint.
--           Hay backup truoc khi chay.
-- =============================================

USE ErpOnlineOrderDb;
GO

-- =============================================
-- 0. KIEM TRA TRANG THAI TRUOC KHI CHAY
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StaffRegionRules')
BEGIN
    RAISERROR(N'[ERROR] Bang StaffRegionRules chua ton tai. Chay AddStaffRegionRules.sql truoc.', 16, 1);
    RETURN;
END

-- =============================================
-- 1. THEM COT Ward_ids (neu chua co)
-- =============================================
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('StaffRegionRules') AND name = 'Ward_ids'
)
BEGIN
    ALTER TABLE StaffRegionRules ADD Ward_ids NVARCHAR(MAX) NULL;
    PRINT N'[OK] Da them cot Ward_ids';
END
ELSE
BEGIN
    PRINT N'[SKIP] Cot Ward_ids da ton tai';
END
GO

-- =============================================
-- 2. DI CHUYEN DU LIEU: Ward_id -> Ward_ids (JSON)
--    Gom cac row cung (Staff_id, Province_id) lai
--    thanh 1 row voi Ward_ids la JSON array
-- =============================================
-- Buoc 2a: Voi moi (Staff_id, Province_id), tinh JSON cua tat ca Ward_id
;WITH Grouped AS (
    SELECT
        Staff_id,
        Province_id,
        -- Neu TAN CA ward_id deu NULL -> province-wide -> de NULL
        -- Neu co bat ky ward_id nao -> tao JSON array
        CASE
            WHEN MIN(CAST(Ward_id AS INT)) IS NULL AND MAX(CAST(Ward_id AS INT)) IS NULL
                THEN NULL
            ELSE
                '[' + STRING_AGG(CAST(Ward_id AS VARCHAR(10)), ',') WITHIN GROUP (ORDER BY Ward_id) + ']'
        END AS Ward_ids_json,
        MIN(Id)         AS Keep_Id,
        MIN(Created_by) AS Created_by,
        MIN(Created_at) AS Created_at
    FROM StaffRegionRules
    WHERE Is_deleted = 0
      AND Ward_id IS NOT NULL  -- xu ly rieng cac row co ward_id
    GROUP BY Staff_id, Province_id
)
UPDATE r
SET r.Ward_ids = g.Ward_ids_json
FROM StaffRegionRules r
INNER JOIN Grouped g ON r.Staff_id = g.Staff_id AND r.Province_id = g.Province_id AND r.Id = g.Keep_Id;

-- Buoc 2b: Danh dau cac row trung lap (cung Staff+Province, khong phai row chinh)
;WITH Ranked AS (
    SELECT id,
           ROW_NUMBER() OVER (PARTITION BY Staff_id, Province_id ORDER BY Id) AS rn
    FROM StaffRegionRules
    WHERE Is_deleted = 0
)
UPDATE StaffRegionRules
SET Is_deleted = 1, Updated_at = GETDATE()
WHERE Id IN (SELECT id FROM Ranked WHERE rn > 1);

PRINT N'[OK] Da di chuyen du lieu Ward_id -> Ward_ids';
GO

-- =============================================
-- 3. XOA CONSTRAINT FK Ward_id (neu con)
-- =============================================
DECLARE @fk_name NVARCHAR(256);

SELECT @fk_name = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
WHERE fk.parent_object_id = OBJECT_ID('StaffRegionRules')
  AND c.name = 'Ward_id';

IF @fk_name IS NOT NULL
BEGIN
    EXEC('ALTER TABLE StaffRegionRules DROP CONSTRAINT [' + @fk_name + ']');
    PRINT N'[OK] Da xoa FK constraint cot Ward_id: ' + @fk_name;
END
ELSE
    PRINT N'[SKIP] Khong tim thay FK constraint cho Ward_id';
GO

-- =============================================
-- 4. XOA COT Ward_id (neu con)
-- =============================================
IF EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('StaffRegionRules') AND name = 'Ward_id'
)
BEGIN
    ALTER TABLE StaffRegionRules DROP COLUMN Ward_id;
    PRINT N'[OK] Da xoa cot Ward_id';
END
ELSE
    PRINT N'[SKIP] Cot Ward_id da duoc xoa truoc do';
GO

-- =============================================
-- 5. XOA INDEX CU (Province_id, Ward_id)
-- =============================================
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('StaffRegionRules') AND name = 'UIX_StaffRegionRules_Province_Ward')
BEGIN
    DROP INDEX UIX_StaffRegionRules_Province_Ward ON StaffRegionRules;
    PRINT N'[OK] Da xoa index UIX_StaffRegionRules_Province_Ward';
END
ELSE
    PRINT N'[SKIP] Index UIX_StaffRegionRules_Province_Ward khong ton tai';
GO

-- =============================================
-- 6. TAO INDEX MOI (Staff_id, Province_id) UNIQUE
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('StaffRegionRules') AND name = 'UIX_StaffRegionRules_Staff_Province')
BEGIN
    CREATE UNIQUE INDEX UIX_StaffRegionRules_Staff_Province
        ON StaffRegionRules (Staff_id, Province_id)
        WHERE Is_deleted = 0;
    PRINT N'[OK] Da tao index UIX_StaffRegionRules_Staff_Province';
END
ELSE
    PRINT N'[SKIP] Index UIX_StaffRegionRules_Staff_Province da ton tai';
GO

-- =============================================
-- 7. KIEM TRA KET QUA
-- =============================================
SELECT
    Id,
    Staff_id,
    Province_id,
    Ward_ids,
    Is_deleted,
    Updated_at
FROM StaffRegionRules
ORDER BY Province_id, Staff_id;
GO

PRINT N'=====================================';
PRINT N'[DONE] Migration StaffRegionRules -> JSON column hoan tat';
PRINT N'=====================================';
GO
