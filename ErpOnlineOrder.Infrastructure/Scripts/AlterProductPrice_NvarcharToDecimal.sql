-- ===========================================
-- Script: Đổi cột Product_price từ nvarchar(50) sang decimal(18,0)
-- Bảng: Products
-- Ngày: 2026-03-10
-- ===========================================

-- Bước 1: Cập nhật các giá trị rỗng/NULL/không phải số thành '0'
UPDATE Products 
SET Product_price = '0' 
WHERE Product_price IS NULL 
   OR LTRIM(RTRIM(Product_price)) = '' 
   OR ISNUMERIC(Product_price) = 0;

-- Bước 2: Đổi kiểu cột sang DECIMAL(18,0)
ALTER TABLE Products 
ALTER COLUMN Product_price DECIMAL(18,0) NOT NULL;

PRINT N'Hoàn tất: Product_price đã đổi sang DECIMAL(18,0)';
