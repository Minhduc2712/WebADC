# WebADC (ERP Online Order)

Hệ thống quản lý đặt hàng/ERP với Web API và giao diện Web MVC. Dự án hỗ trợ quản lý danh mục sản phẩm, đơn hàng, hóa đơn, kho/xuất kho, khách hàng, phân quyền và các cấu hình hệ thống (SMTP, thông báo).

## Tính năng chính

- Xác thực người dùng, phân quyền theo role/permission.
- Quản lý sản phẩm, tác giả, nhà xuất bản, loại bìa, danh mục.
- Quản lý khách hàng, tổ chức, khu vực (vùng/tỉnh/phường).
- Đơn hàng, hóa đơn, xuất kho và theo dõi trạng thái.
- Quản lý tồn kho và giao dịch kho.
- Xuất/Nhập dữ liệu Excel (sản phẩm, đơn hàng, hóa đơn, xuất kho).
- In/xuất PDF cho đơn hàng, hóa đơn, phiếu xuất kho.
- Email thông báo theo luồng nghiệp vụ (đơn hàng, xuất kho, đặt lại mật khẩu...).

## Công nghệ sử dụng

- .NET 8, ASP.NET Core Web API & MVC
- Entity Framework Core + SQL Server
- AutoMapper
- BCrypt (hash mật khẩu)
- ClosedXML / OpenXML (Excel)
- QuestPDF (PDF)
- MailKit (SMTP)
- Swagger (tài liệu API)

## Cấu trúc dự án

- `ErpOnlineOrder.Domain`: Entity, enum, constant.
- `ErpOnlineOrder.Application`: DTO, service, logic nghiệp vụ.
- `ErpOnlineOrder.Infrastructure`: DbContext, repository, migrations, email, scripts.
- `ErpOnlineOrder.WebAPI`: REST API, Swagger, middleware.
- `ErpOnlineOrder.WebMVC`: Giao diện quản trị/khách hàng (MVC).

## Cài đặt & chạy dự án

### 1) Yêu cầu

- .NET 8 SDK
- SQL Server

### 2) Cấu hình

1. Cập nhật `ConnectionStrings:DefaultConnection` trong:
   - `ErpOnlineOrder.WebAPI/appsettings.json`
   - `ErpOnlineOrder.WebMVC/appsettings.json`
2. (Tùy chọn) Cấu hình base URL API cho MVC:
   - Thêm `ApiSettings:BaseUrl` trong `ErpOnlineOrder.WebMVC/appsettings.json`
   - Nên dùng HTTPS (ví dụ `https://localhost:{PORT}/api/` nếu cấu hình HTTPS). HTTP chỉ nên dùng cho môi trường phát triển cục bộ; production cần cấu hình HTTPS.

### 3) Database

Có 2 cách:

- **EF Core Migrations**  
  ```bash
  dotnet ef database update --project ErpOnlineOrder.Infrastructure --startup-project ErpOnlineOrder.WebAPI
  ```
- **Chạy script SQL** trong `ErpOnlineOrder.Infrastructure/Scripts`:
  - `CreateDatabase.sql`
  - `SeedData.sql`
  - `CreateAdminUser.sql` / `ResetAdminPassword.sql`

### 4) Chạy ứng dụng

```bash
# Chạy Web API
dotnet run --project ErpOnlineOrder.WebAPI

# Chạy Web MVC
dotnet run --project ErpOnlineOrder.WebMVC
```

Swagger: `https://localhost:{PORT}/swagger`

## Ghi chú

- Cấu hình SMTP có thể cập nhật trong màn hình Setting trên Web MVC.
- Các chức năng xuất Excel/PDF nằm trong các module đơn hàng, hóa đơn, xuất kho, sản phẩm.
