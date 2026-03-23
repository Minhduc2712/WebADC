using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình encoding UTF-8
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// --- PHẦN 1: ĐĂNG KÝ CÁC DỊCH VỤ (SERVICES) ---

builder.Services.AddControllersWithViews();

// Cấu hình Response Encoding
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueCountLimit = int.MaxValue;
});

// ============================================
// CẤU HÌNH SESSION + COOKIE AUTHENTICATION
// ============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;                  // Cookie chỉ truy cập từ server
    options.Cookie.IsEssential = true;               // Cookie cần thiết cho GDPR
    options.Cookie.Name = ".ErpOnlineOrder.Session"; // Tên cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS khi production
    options.Cookie.SameSite = SameSiteMode.Lax;      // Bảo vệ CSRF
});

// Cần thiết để truy cập Session từ các lớp khác ngoài Controller
builder.Services.AddHttpContextAccessor();

// ============================================
// API CLIENT: WebMVC gọi WebAPI cho Auth (và sau này có thể mở rộng)
// ============================================
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5051/api/";
builder.Services.AddTransient<ErpOnlineOrder.WebMVC.Services.ErpApiUserIdHandler>();
builder.Services.AddHttpClient("ErpApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<ErpOnlineOrder.WebMVC.Services.ErpApiUserIdHandler>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IAuthApiClient, ErpOnlineOrder.WebMVC.Services.AuthApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ICategoryApiClient, ErpOnlineOrder.WebMVC.Services.CategoryApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IAuthorApiClient, ErpOnlineOrder.WebMVC.Services.AuthorApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IPublisherApiClient, ErpOnlineOrder.WebMVC.Services.PublisherApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ICoverTypeApiClient, ErpOnlineOrder.WebMVC.Services.CoverTypeApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IWarehouseApiClient, ErpOnlineOrder.WebMVC.Services.WarehouseApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IDistributorApiClient, ErpOnlineOrder.WebMVC.Services.DistributorApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IRegionApiClient, ErpOnlineOrder.WebMVC.Services.RegionApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IProvinceApiClient, ErpOnlineOrder.WebMVC.Services.ProvinceApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IWardApiClient, ErpOnlineOrder.WebMVC.Services.WardApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IProductApiClient, ErpOnlineOrder.WebMVC.Services.ProductApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ICustomerApiClient, ErpOnlineOrder.WebMVC.Services.CustomerApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IOrderApiClient, ErpOnlineOrder.WebMVC.Services.OrderApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IPermissionApiClient, ErpOnlineOrder.WebMVC.Services.PermissionApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ICustomerManagementApiClient, ErpOnlineOrder.WebMVC.Services.CustomerManagementApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IStaffRegionRuleApiClient, ErpOnlineOrder.WebMVC.Services.StaffRegionRuleApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ICustomerProductApiClient, ErpOnlineOrder.WebMVC.Services.CustomerProductApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IAdminApiClient, ErpOnlineOrder.WebMVC.Services.AdminApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IInvoiceApiClient, ErpOnlineOrder.WebMVC.Services.InvoiceApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IWarehouseExportApiClient, ErpOnlineOrder.WebMVC.Services.WarehouseExportApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.IStockApiClient, ErpOnlineOrder.WebMVC.Services.StockApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.Interfaces.ISettingApiClient, ErpOnlineOrder.WebMVC.Services.SettingApiClient>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// --- PHẦN 2: CẤU HÌNH PIPELINE (MIDDLEWARE) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Xử lý các trang lỗi theo HTTP Status Code (404, 403, 500, ...)
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ============================================
// SESSION + AUTHENTICATION MIDDLEWARE
// ============================================
// Session PHẢI được cấu hình TRƯỚC custom middleware
app.UseSession();

// Custom Authentication Middleware - Kiểm tra session và khôi phục từ cookie
app.UseSessionAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
