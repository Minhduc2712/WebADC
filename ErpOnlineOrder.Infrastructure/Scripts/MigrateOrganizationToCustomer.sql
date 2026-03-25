-- ============================================================
-- Script: MigrateOrganizationToCustomer.sql
-- Mô tả: Tách cấu trúc OrganizationInformations ↔ Customers
--
-- Trước (cũ):
--   OrganizationInformations có Customer_id (FK → Customers.Id)
--   OrganizationInformations có Recipient_name, Recipient_phone, Recipient_address
--   Quan hệ: 1 Customer → 1 Organization
--
-- Sau (mới):
--   Customers có Organization_information_id (FK → OrganizationInformations.Id)
--   Customers có Recipient_name, Recipient_phone (nullable), Recipient_address
--   Quan hệ: 1 Organization → nhiều Customers
--
-- Cách dùng:
--   EXEC dbo.sp_MigrateOrganizationToCustomer;
-- ============================================================

-- ----------------------------------------------------------
-- Tạo stored procedure chuyển dữ liệu
-- ----------------------------------------------------------
IF OBJECT_ID(N'dbo.sp_MigrateOrganizationToCustomer', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_MigrateOrganizationToCustomer;
GO

CREATE PROCEDURE dbo.sp_MigrateOrganizationToCustomer
AS
BEGIN
    SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN TRY

    -- ----------------------------------------------------------
    -- BƯỚC 1: Thêm cột mới vào bảng Customers (tất cả nullable)
    -- ----------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'Customers') AND name = N'Organization_information_id'
    )
    BEGIN
        ALTER TABLE [Customers]
            ADD [Organization_information_id] INT           NULL,
                [Recipient_name]              NVARCHAR(100) NULL,
                [Recipient_phone]             NVARCHAR(50)  NULL,
                [Recipient_address]           NVARCHAR(500) NULL;
        PRINT 'Bước 1: Đã thêm cột mới vào Customers.';
    END
    ELSE
        PRINT 'Bước 1: Các cột đã tồn tại — bỏ qua.';

    -- ----------------------------------------------------------
    -- BƯỚC 2: Migrate dữ liệu từ OrganizationInformations → Customers
    -- Dùng dynamic SQL vì cột mới chưa tồn tại lúc stored procedure được compile.
    -- Với mỗi Customer, lấy Organization mới nhất chưa xóa
    -- ----------------------------------------------------------
    EXEC(N'
        UPDATE c
        SET
            c.[Organization_information_id] = latest.[Id],
            c.[Recipient_name]              = latest.[Recipient_name],
            c.[Recipient_phone]             = latest.[Recipient_phone],
            c.[Recipient_address]           = latest.[Recipient_address]
        FROM [Customers] c
        CROSS APPLY (
            SELECT TOP 1
                [Id], [Recipient_name], [Recipient_phone], [Recipient_address]
            FROM [OrganizationInformations]
            WHERE [Customer_id] = c.[Id]
            ORDER BY [Is_deleted] ASC, [Id] DESC
        ) AS latest
        WHERE c.[Organization_information_id] IS NULL
    ');

    PRINT 'Bước 2: Đã migrate dữ liệu Recipient và Organization_information_id.';

    -- ----------------------------------------------------------
    -- BƯỚC 3: Tạo Organization mặc định cho Customer chưa có org
    -- Cursor query dùng NOT EXISTS (tránh tham chiếu cột mới lúc compile)
    -- UPDATE dùng sp_executesql vì cột mới chưa tồn tại lúc compile
    -- ----------------------------------------------------------
    DECLARE @CustomerId INT;
    DECLARE @NewOrgId   INT;

    -- Không tham chiếu cột Organization_information_id (chưa tồn tại lúc compile);
    -- sau Bước 2, customer nào vẫn chưa có org chính là customer không có bất kỳ
    -- bản ghi nào trong OrganizationInformations
    DECLARE customer_cursor CURSOR FAST_FORWARD FOR
        SELECT c.[Id]
        FROM   [Customers] c
        WHERE  NOT EXISTS (
            SELECT 1 FROM [OrganizationInformations] oi
            WHERE oi.[Customer_id] = c.[Id]
        );

    OPEN customer_cursor;
    FETCH NEXT FROM customer_cursor INTO @CustomerId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- INSERT vẫn dùng cột cũ (Customer_id, Recipient_* trên OrganizationInformations)
        -- nên không cần dynamic SQL ở đây
        INSERT INTO [OrganizationInformations]
            ([Organization_code],
             [Organization_name],
             [Address],
             [Tax_number],
             [Customer_id],
             [Recipient_name],
             [Recipient_phone],
             [Recipient_address],
             [Is_deleted],
             [Created_at], [Updated_at],
             [Created_by], [Updated_by])
        SELECT
            N'ORG-AUTO-' + CAST(c.[Id] AS NVARCHAR(10)),
            ISNULL(NULLIF(LTRIM(RTRIM(c.[Full_name])), ''), N'Khách hàng ' + CAST(c.[Id] AS NVARCHAR(10))),
            ISNULL(c.[Address], N''),
            N'',
            c.[Id],
            c.[Full_name],
            ISNULL(c.[Phone_number], N''),  -- Recipient_phone NOT NULL trong schema cũ
            c.[Address],
            0,
            GETDATE(), GETDATE(),
            1, 1
        FROM [Customers] c
        WHERE c.[Id] = @CustomerId;

        SET @NewOrgId = SCOPE_IDENTITY();

        -- UPDATE các cột mới trên Customers → dùng sp_executesql để bypass compile-time check
        EXEC sp_executesql
            N'UPDATE [Customers]
              SET [Organization_information_id] = @OrgId,
                  [Recipient_name]              = (SELECT [Full_name]    FROM [Customers] WHERE [Id] = @CustId),
                  [Recipient_phone]             = (SELECT [Phone_number] FROM [Customers] WHERE [Id] = @CustId),
                  [Recipient_address]           = (SELECT [Address]      FROM [Customers] WHERE [Id] = @CustId)
              WHERE [Id] = @CustId',
            N'@OrgId INT, @CustId INT',
            @OrgId  = @NewOrgId,
            @CustId = @CustomerId;

        SET @NewOrgId = NULL;
        FETCH NEXT FROM customer_cursor INTO @CustomerId;
    END;

    CLOSE customer_cursor;
    DEALLOCATE customer_cursor;

    PRINT 'Bước 3: Đã tạo Organization mặc định cho các Customer thiếu.';

    -- ----------------------------------------------------------
    -- BƯỚC 4: Đổi Organization_information_id thành NOT NULL
    --          Recipient_phone (số điện thoại nhận hàng) vẫn để NULL được
    -- ----------------------------------------------------------
    ALTER TABLE [Customers]
        ALTER COLUMN [Organization_information_id] INT NOT NULL;

    PRINT 'Bước 4: Organization_information_id → NOT NULL.';

    -- ----------------------------------------------------------
    -- BƯỚC 5: Thêm FK mới: Customers → OrganizationInformations
    -- ----------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_Customers_OrganizationInformations_Organization_information_id'
    )
    BEGIN
        ALTER TABLE [Customers]
            ADD CONSTRAINT [FK_Customers_OrganizationInformations_Organization_information_id]
            FOREIGN KEY ([Organization_information_id])
            REFERENCES [OrganizationInformations] ([Id])
            ON DELETE NO ACTION;
        PRINT 'Bước 5: Đã thêm FK Customers → OrganizationInformations.';
    END
    ELSE
        PRINT 'Bước 5: FK đã tồn tại — bỏ qua.';

    -- ----------------------------------------------------------
    -- BƯỚC 6: Xóa TẤT CẢ FK trên OrganizationInformations tham chiếu cột Customer_id
    --          (tìm động để không phụ thuộc tên constraint cố định)
    -- ----------------------------------------------------------
    DECLARE @fkName   NVARCHAR(256);
    DECLARE @dropSql  NVARCHAR(512);

    DECLARE fk_cursor CURSOR FAST_FORWARD FOR
        SELECT fk.name
        FROM   sys.foreign_keys            fk
        JOIN   sys.foreign_key_columns     fkc ON fkc.constraint_object_id = fk.object_id
        JOIN   sys.columns                 c   ON c.object_id = fkc.parent_object_id
                                               AND c.column_id = fkc.parent_column_id
        WHERE  fk.parent_object_id  = OBJECT_ID(N'OrganizationInformations')
          AND  c.name               = N'Customer_id';

    OPEN fk_cursor;
    FETCH NEXT FROM fk_cursor INTO @fkName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @dropSql = N'ALTER TABLE [OrganizationInformations] DROP CONSTRAINT [' + @fkName + N']';
        EXEC sp_executesql @dropSql;
        PRINT 'Bước 6: Đã xóa FK ' + @fkName;
        FETCH NEXT FROM fk_cursor INTO @fkName;
    END;

    CLOSE fk_cursor;
    DEALLOCATE fk_cursor;

    -- ----------------------------------------------------------
    -- BƯỚC 7: Xóa index cũ trên Customer_id
    -- ----------------------------------------------------------
    IF EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name      = N'IX_OrganizationInformations_Customer_id'
          AND object_id = OBJECT_ID(N'OrganizationInformations')
    )
    BEGIN
        DROP INDEX [IX_OrganizationInformations_Customer_id]
            ON [OrganizationInformations];
        PRINT 'Bước 7: Đã xóa IX_OrganizationInformations_Customer_id.';
    END
    ELSE
        PRINT 'Bước 7: Index cũ không tồn tại — bỏ qua.';

    -- ----------------------------------------------------------
    -- BƯỚC 8a: Drop TẤT CẢ constraints (DEFAULT, CHECK)
    --           trên các cột cũ trước khi DROP COLUMN
    -- ----------------------------------------------------------
    DECLARE @dfName    NVARCHAR(256);
    DECLARE @dfSql     NVARCHAR(512);

    DECLARE df_cursor CURSOR FAST_FORWARD FOR
        -- DEFAULT constraints
        SELECT dc.name
        FROM   sys.default_constraints dc
        JOIN   sys.columns             c  ON  c.object_id = dc.parent_object_id
                                          AND c.column_id = dc.parent_column_id
        WHERE  dc.parent_object_id = OBJECT_ID(N'OrganizationInformations')
          AND  c.name IN (N'Customer_id', N'Recipient_name', N'Recipient_phone', N'Recipient_address')

        UNION ALL

        -- CHECK constraints
        SELECT cc.name
        FROM   sys.check_constraints  cc
        JOIN   sys.columns            c  ON  c.object_id = cc.parent_object_id
                                         AND c.column_id = cc.parent_column_id
        WHERE  cc.parent_object_id = OBJECT_ID(N'OrganizationInformations')
          AND  c.name IN (N'Customer_id', N'Recipient_name', N'Recipient_phone', N'Recipient_address');

    OPEN df_cursor;
    FETCH NEXT FROM df_cursor INTO @dfName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @dfSql = N'ALTER TABLE [OrganizationInformations] DROP CONSTRAINT [' + @dfName + N']';
        EXEC sp_executesql @dfSql;
        PRINT 'Bước 8a: Đã xóa constraint ' + @dfName;
        FETCH NEXT FROM df_cursor INTO @dfName;
    END;

    CLOSE df_cursor;
    DEALLOCATE df_cursor;

    -- ----------------------------------------------------------
    -- BƯỚC 8b: Xóa các cột cũ khỏi OrganizationInformations
    -- ----------------------------------------------------------
    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID(N'OrganizationInformations') AND name = N'Customer_id'
    )
    BEGIN
        ALTER TABLE [OrganizationInformations]
            DROP COLUMN [Customer_id],
                        [Recipient_name],
                        [Recipient_phone],
                        [Recipient_address];
        PRINT 'Bước 8b: Đã xóa các cột cũ khỏi OrganizationInformations.';
    END
    ELSE
        PRINT 'Bước 8b: Các cột cũ đã được xóa trước đó — bỏ qua.';

    -- ----------------------------------------------------------
    -- HOÀN THÀNH
    -- ----------------------------------------------------------
    COMMIT TRANSACTION;
    PRINT '============================================================';
    PRINT 'Migration hoàn thành thành công!';
    PRINT '============================================================';

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    PRINT '============================================================';
    PRINT 'Migration THẤT BẠI — đã rollback toàn bộ thay đổi.';
    PRINT 'Lỗi: ' + ERROR_MESSAGE();
    PRINT '============================================================';

    THROW;
END CATCH;
END;
GO

-- ----------------------------------------------------------
-- Gọi stored procedure để thực hiện migration
-- ----------------------------------------------------------
EXEC dbo.sp_MigrateOrganizationToCustomer;
GO
