using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.Application.Interfaces.Security;
using ErpOnlineOrder.Infrastructure.Repositories;
using ErpOnlineOrder.Infrastructure.Persistence;
using ErpOnlineOrder.WebMVC.Middleware;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IAuthApiClient, ErpOnlineOrder.WebMVC.Services.AuthApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.ICategoryApiClient, ErpOnlineOrder.WebMVC.Services.CategoryApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IAuthorApiClient, ErpOnlineOrder.WebMVC.Services.AuthorApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IPublisherApiClient, ErpOnlineOrder.WebMVC.Services.PublisherApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.ICoverTypeApiClient, ErpOnlineOrder.WebMVC.Services.CoverTypeApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IWarehouseApiClient, ErpOnlineOrder.WebMVC.Services.WarehouseApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IDistributorApiClient, ErpOnlineOrder.WebMVC.Services.DistributorApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IRegionApiClient, ErpOnlineOrder.WebMVC.Services.RegionApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IProvinceApiClient, ErpOnlineOrder.WebMVC.Services.ProvinceApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IProductApiClient, ErpOnlineOrder.WebMVC.Services.ProductApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.ICustomerApiClient, ErpOnlineOrder.WebMVC.Services.CustomerApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IOrderApiClient, ErpOnlineOrder.WebMVC.Services.OrderApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IPermissionApiClient, ErpOnlineOrder.WebMVC.Services.PermissionApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.ICustomerManagementApiClient, ErpOnlineOrder.WebMVC.Services.CustomerManagementApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IAdminApiClient, ErpOnlineOrder.WebMVC.Services.AdminApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IInvoiceApiClient, ErpOnlineOrder.WebMVC.Services.InvoiceApiClient>();
builder.Services.AddScoped<ErpOnlineOrder.WebMVC.Services.IWarehouseExportApiClient, ErpOnlineOrder.WebMVC.Services.WarehouseExportApiClient>();

// Add DbContext
builder.Services.AddDbContext<ErpOnlineOrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
builder.Services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IProvinceRepository, ProvinceRepository>();
builder.Services.AddScoped<IDistributorRepository, DistributorRepository>();
builder.Services.AddScoped<ICustomerManagementRepository, CustomerManagementRepository>();
builder.Services.AddScoped<ICustomerProductRepository, CustomerProductRepository>();
builder.Services.AddScoped<ICustomerCategoryRepository, CustomerCategoryRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IWarehouseExportRepository, WarehouseExportRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<ICoverTypeRepository, CoverTypeRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();

// Add security services
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// Add services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRememberMeService, RememberMeService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<IDistributorService, DistributorService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<ICustomerManagementService, CustomerManagementService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICustomerProductService, CustomerProductService>();
builder.Services.AddScoped<ICustomerCategoryService, CustomerCategoryService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IWarehouseExportService, WarehouseExportService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<ICoverTypeService, CoverTypeService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

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
