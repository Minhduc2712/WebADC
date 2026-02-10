using ErpOnlineOrder.WebMVC.Models;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Attributes;
using ErpOnlineOrder.WebMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPermissionApiClient _permissionApiClient;

        public HomeController(
            ILogger<HomeController> logger,
            IPermissionApiClient permissionApiClient)
        {
            _logger = logger;
            _permissionApiClient = permissionApiClient;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Order");
            return RedirectToAction("Login", "Auth");
        }

        [RequireAuth]
        public IActionResult GetSessionInfo()
        {
            return Json(new
            {
                isAuthenticated = HttpContext.Session.IsAuthenticated(),
                userId = HttpContext.Session.GetUserId(),
                username = HttpContext.Session.GetUsername(),
                fullName = HttpContext.Session.GetFullName(),
                email = HttpContext.Session.GetEmail(),
                userType = HttpContext.Session.GetUserType(),
                roles = HttpContext.Session.GetRoles(),
                permissionCount = HttpContext.Session.GetPermissions().Count,
                loginTime = HttpContext.Session.GetLoginTime()
            });
        }
        [RequireAuth]
        public async Task<IActionResult> DebugPermissions()
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == 0)
                return Json(new { error = "Chưa đăng nhập" });

            try
            {
                var permissions = await _permissionApiClient.GetUserPermissionCodesAsync(userId);
                var fullPermissions = await _permissionApiClient.GetUserFullPermissionsAsync(userId);

                return Json(new
                {
                    session = new
                    {
                        userId = userId,
                        username = HttpContext.Session.GetUsername(),
                        fullName = HttpContext.Session.GetFullName(),
                        userType = HttpContext.Session.GetUserType(),
                        roles = HttpContext.Session.GetRoles()
                    },
                    permissionCount = permissions?.Count() ?? 0,
                    hasStaffView = permissions?.Contains("STAFF_VIEW") ?? false,
                    hasRoleView = permissions?.Contains("ROLE_VIEW") ?? false,
                    allPermissions = permissions?.ToList() ?? new List<string>(),
                    fullDetails = fullPermissions
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
        public IActionResult AccessDenied()
        {
            ViewBag.Username = HttpContext.Session.GetUsername();
            ViewBag.FullName = HttpContext.Session.GetFullName();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            var statusCode = id ?? 500;
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var model = ErrorViewModel.FromStatusCode(statusCode, requestId);
            model.Path = HttpContext.Request.Path;
            model.Timestamp = DateTime.Now;

            // Trong Development, hiển thị thêm thông tin kỹ thuật
            var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error != null)
            {
                var ex = exceptionFeature.Error;
                model.Message = ex.Message;

                if (HttpContext.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
                {
                    model.TechnicalDetails = $"Exception: {ex.GetType().FullName}\nMessage: {ex.Message}\nSource: {ex.Source}\n\nStackTrace:\n{ex.StackTrace}";

                    if (ex.InnerException != null)
                    {
                        model.TechnicalDetails += $"\n\nInner Exception: {ex.InnerException.GetType().FullName}\nMessage: {ex.InnerException.Message}\nStackTrace:\n{ex.InnerException.StackTrace}";
                    }
                }

                // Thêm chi tiết lỗi nếu là validation error
                if (ex is AggregateException aggEx)
                {
                    foreach (var innerEx in aggEx.InnerExceptions)
                    {
                        model.ErrorDetails.Add(innerEx.Message);
                    }
                }
            }

            // Lấy error details từ TempData nếu có
            if (TempData["ErrorDetailsList"] is string errorDetailsJson)
            {
                try
                {
                    var errors = System.Text.Json.JsonSerializer.Deserialize<List<string>>(errorDetailsJson);
                    if (errors != null)
                    {
                        model.ErrorDetails.AddRange(errors);
                    }
                }
                catch { }
            }

            Response.StatusCode = statusCode;
            return View("Error", model);
        }
    }
}
