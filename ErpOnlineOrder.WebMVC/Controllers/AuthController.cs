using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using ErpOnlineOrder.Application.Interfaces.Repositories;
using ErpOnlineOrder.WebMVC.Extensions;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionService _permissionService;
        private readonly IRememberMeService _rememberMeService;
        private readonly ILogger<AuthController> _logger;

        private const int RememberMeDays = 30;

        public AuthController(
            IAuthService authService,
            IUserRepository userRepository,
            IPermissionService permissionService,
            IRememberMeService rememberMeService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userRepository = userRepository;
            _permissionService = permissionService;
            _rememberMeService = rememberMeService;
            _logger = logger;
        }

        #region Login

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (await TryAutoLoginFromCookie())
            {
                return RedirectToReturnUrl(returnUrl);
            }

            if (HttpContext.Session.IsAuthenticated())
            {
                return RedirectToReturnUrl(returnUrl);
            }

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginUserDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserDto model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _authService.LoginAsync(model);

                if (!string.IsNullOrEmpty(result))
                {
                    var user = await _userRepository.FindByIdentifierAsync(model.Identifier ?? "");

                    if (user != null)
                    {
                        // Ki?m tra t?i kho?n active
                        if (!user.Is_active)
                        {
                            ModelState.AddModelError("", "T?i kho?n c?a b?n d? b? v? hi?u h?a. Vui l?ng li?n h? qu?n tr? vi?n.");
                            return View(model);
                        }

                        // Luu v?o Session
                        await SetUserSession(user);

                        if (model.RememberMe)
                        {
                            SetRememberMeCookies(user.Username, _rememberMeService.GenerateToken(user));
                        }
                        else
                        {
                            ClearRememberMeCookies();
                        }

                        _logger.LogInformation("User {Username} logged in successfully", user.Username);

                        return RedirectToReturnUrl(returnUrl);
                    }
                }

                ModelState.AddModelError("", "T?n dang nh?p ho?c m?t kh?u kh?ng d?ng.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Login failed for {Identifier}: {Message}", model.Identifier, ex.Message);
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        #endregion

        #region Customer Register

        [HttpGet]
        public IActionResult CustomerRegister()
        {
            if (HttpContext.Session.IsAuthenticated())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new RegisterCustomerDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegister(RegisterCustomerDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var success = await _authService.RegisterByCustomerAsync(model);

                if (success)
                {
                    _logger.LogInformation("New customer registered: {Username}", model.Username);
                    TempData["SuccessMessage"] = "?ang k? th?nh c?ng! Vui l?ng dang nh?p.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError("", "?ang k? th?t b?i. Vui l?ng th? l?i.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Customer registration failed: {Message}", ex.Message);
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        #endregion

        #region Staff Register (Admin only)

        [HttpGet]
        public IActionResult Register()
        {
            if (!HttpContext.Session.IsStaff())
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            return View(new RegisterStaffDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterStaffDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var success = await _authService.RegisterByAdminAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "T?o t?i kho?n nh?n vi?n th?nh c?ng!";
                    return RedirectToAction("Index", "Staff");
                }

                ModelState.AddModelError("", "T?o t?i kho?n th?t b?i. Vui l?ng th? l?i.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        #endregion

        #region Change Password

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!HttpContext.Session.IsAuthenticated())
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ChangePasswordDto
            {
                Identifier = HttpContext.Session.GetUsername()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                model.Identifier = HttpContext.Session.GetUsername();

                var success = await _authService.ChangePasswordAsync(model);

                if (success)
                {
                    ClearRememberMeCookies();

                    _logger.LogInformation("User {Username} changed password", model.Identifier);
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Dashboard", "Home");
                }

                ModelState.AddModelError("", "Đổi mật khẩu thất bại.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        #endregion

        #region Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email.");
                return View();
            }

            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                TempData["SuccessMessage"] = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.";

                if (user != null)
                {
                    // TODO: G?i email reset password
                    _logger.LogInformation("Password reset requested for email: {Email}", email);
                }

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", email);
                ModelState.AddModelError("", "Có lỗi xảy ra. Vui lòng thử lại.");
                ViewBag.Email = email;
                return View();
            }
        }

        #endregion

        #region Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetUsername();

            // X?a Session
            HttpContext.Session.Clear();

            // X?a Remember Me cookies
            ClearRememberMeCookies();

            _logger.LogInformation("User {Username} logged out", username);

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult LogoutGet()
        {
            var username = HttpContext.Session.GetUsername();

            HttpContext.Session.Clear();
            ClearRememberMeCookies();

            _logger.LogInformation("User {Username} logged out (GET)", username);

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Private Methods
        private async Task<bool> TryAutoLoginFromCookie()
        {
            try
            {
                if (!Request.Cookies.ContainsKey("RememberMe_Username") ||
                    !Request.Cookies.ContainsKey("RememberMe_Token"))
                {
                    return false;
                }

                var username = Request.Cookies["RememberMe_Username"];
                var token = Request.Cookies["RememberMe_Token"];

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(token))
                {
                    return false;
                }

                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null || !user.Is_active || user.Is_deleted)
                {
                    ClearRememberMeCookies();
                    return false;
                }

                if (!await _rememberMeService.ValidateTokenAsync(username, token))
                {
                    ClearRememberMeCookies();
                    return false;
                }

                await SetUserSession(user);
                _logger.LogInformation("Auto-login from cookie for user: {Username}", username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during auto-login from cookie");
                ClearRememberMeCookies();
                return false;
            }
        }
        private async Task SetUserSession(Domain.Models.User user)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);

            var fullName = user.Staff?.Full_name ?? user.Customer?.Full_name ?? user.Username;
            HttpContext.Session.SetString("FullName", fullName);

            var roles = user.User_roles?
                .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                .Select(ur => ur.Role!.Role_name)
                .ToList() ?? new List<string>();
            HttpContext.Session.SetString("Roles", string.Join(",", roles));

            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
            HttpContext.Session.SetString("Permissions", string.Join(",", permissions));

            if (user.Staff != null)
            {
                HttpContext.Session.SetString("UserType", "Staff");
                HttpContext.Session.SetString("StaffCode", user.Staff.Staff_code ?? "");
            }
            else if (user.Customer != null)
            {
                HttpContext.Session.SetString("UserType", "Customer");
                HttpContext.Session.SetString("CustomerCode", user.Customer.Customer_code ?? "");
            }

            HttpContext.Session.SetString("LoginTime", DateTime.Now.ToString("o"));
        }
        private void SetRememberMeCookies(string username, string token)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(RememberMeDays),
                HttpOnly = true,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Append("RememberMe_Username", username, cookieOptions);
            Response.Cookies.Append("RememberMe_Token", token, cookieOptions);
        }
        private void ClearRememberMeCookies()
        {
            Response.Cookies.Delete("RememberMe_Username");
            Response.Cookies.Delete("RememberMe_Token");
        }
        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var sessionReturnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (!string.IsNullOrEmpty(sessionReturnUrl) && Url.IsLocalUrl(sessionReturnUrl))
            {
                HttpContext.Session.Remove("ReturnUrl");
                return Redirect(sessionReturnUrl);
            }

            if (HttpContext.Session.IsCustomer())
            {
                return RedirectToAction("Index", "Shop");
            }

            return RedirectToAction("Dashboard", "Home");
        }

        #endregion
    }
}
