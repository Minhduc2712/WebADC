using Microsoft.AspNetCore.Mvc;
using ErpOnlineOrder.Application.DTOs.AuthDTOs;
using ErpOnlineOrder.WebMVC.Extensions;
using ErpOnlineOrder.WebMVC.Services;
using ErpOnlineOrder.WebMVC.Services.Interfaces;
using System.Text.Json;

namespace ErpOnlineOrder.WebMVC.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly IProvinceApiClient _provinceApiClient;
        private readonly IWardApiClient _wardApiClient;
        private readonly ILogger<AuthController> _logger;

        private const int RememberMeDays = 30;
        private const string CustomerRegisterAccountSessionKey = "CustomerRegister.Account";
        private const string CustomerRegisterPersonalSessionKey = "CustomerRegister.Personal";
        private const string CustomerRegisterOrganizationSessionKey = "CustomerRegister.Organization";

        public AuthController(
            IAuthApiClient authApiClient,
            IProvinceApiClient provinceApiClient,
            IWardApiClient wardApiClient,
            ILogger<AuthController> logger)
        {
            _authApiClient = authApiClient;
            _provinceApiClient = provinceApiClient;
            _wardApiClient = wardApiClient;
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
                // Gọi WebAPI để đăng nhập, nhận về user + permissions
                var apiResult = await _authApiClient.LoginAsync(model);

                if (apiResult != null)
                {
                    SetUserSessionFromApiResult(apiResult);

                    if (model.RememberMe)
                    {
                        var token = await _authApiClient.GenerateTokenAsync(apiResult.Username);
                        if (!string.IsNullOrEmpty(token))
                            SetRememberMeCookies(apiResult.Username, token);
                    }
                    else
                    {
                        ClearRememberMeCookies();
                    }

                    _logger.LogInformation("User {Username} logged in successfully (via API)", apiResult.Username);
                    return RedirectToReturnUrl(returnUrl);
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Login failed for {Identifier}", model.Identifier);
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
                return RedirectToAction("Index", "Order");
            }

            var model = GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey) ?? new RegisterCustomerAccountStepDto();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CustomerRegister(RegisterCustomerAccountStepDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            SaveToSession(CustomerRegisterAccountSessionKey, model);
            return RedirectToAction(nameof(CustomerRegisterPersonal));
        }

        [HttpGet]
        public async Task<IActionResult> CustomerRegisterPersonal()
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Order");

            if (GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegister));

            var model = GetFromSession<RegisterCustomerPersonalStepDto>(CustomerRegisterPersonalSessionKey) ?? new RegisterCustomerPersonalStepDto();

            var provinces = await _provinceApiClient.GetAllAsync();
            ViewBag.Provinces = provinces.OrderBy(p => p.Province_name).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegisterPersonal(RegisterCustomerPersonalStepDto model)
        {
            if (GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegister));

            if (!ModelState.IsValid)
            {
                var provinces = await _provinceApiClient.GetAllAsync();
                ViewBag.Provinces = provinces.OrderBy(p => p.Province_name).ToList();
                return View(model);
            }

            SaveToSession(CustomerRegisterPersonalSessionKey, model);
            return RedirectToAction(nameof(CustomerRegisterOrganization));
        }

        [HttpGet]
        public IActionResult CustomerRegisterOrganization()
        {
            if (HttpContext.Session.IsAuthenticated())
                return RedirectToAction("Index", "Order");

            if (GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegister));
            if (GetFromSession<RegisterCustomerPersonalStepDto>(CustomerRegisterPersonalSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegisterPersonal));

            var model = GetFromSession<RegisterCustomerOrganizationStepDto>(CustomerRegisterOrganizationSessionKey) ?? new RegisterCustomerOrganizationStepDto();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegisterOrganization(RegisterCustomerOrganizationStepDto model)
        {
            if (GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegister));
            if (GetFromSession<RegisterCustomerPersonalStepDto>(CustomerRegisterPersonalSessionKey) == null)
                return RedirectToAction(nameof(CustomerRegisterPersonal));

            if (!ModelState.IsValid)
                return View(model);

            SaveToSession(CustomerRegisterOrganizationSessionKey, model);

            var account = GetFromSession<RegisterCustomerAccountStepDto>(CustomerRegisterAccountSessionKey);
            var personal = GetFromSession<RegisterCustomerPersonalStepDto>(CustomerRegisterPersonalSessionKey);
            var organization = GetFromSession<RegisterCustomerOrganizationStepDto>(CustomerRegisterOrganizationSessionKey);

            if (account == null || personal == null || organization == null)
            {
                ModelState.AddModelError("", "Phiên đăng ký đã hết hạn. Vui lòng thử lại từ đầu.");
                return View(model);
            }

            try
            {
                var request = new FinalizeCustomerRegistrationDto
                {
                    Account = account,
                    Personal = personal,
                    Organization = organization
                };

                var (success, errorMessage) = await _authApiClient.FinalizeCustomerRegistrationAsync(request);

                if (success)
                {
                    _logger.LogInformation("New customer registered via wizard: {Username}", account.Username);
                    ClearCustomerRegisterSession();
                    SetSuccessMessage("Đăng ký thành công! Vui lòng đăng nhập.");
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError("", errorMessage ?? "Không thể hoàn tất đăng ký khách hàng.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Customer wizard registration failed");
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetWardsByProvince(int provinceId)
        {
            try
            {
                var wards = await _wardApiClient.GetByProvinceIdAsync(provinceId);
                return Json(wards.Select(w => new { w.Id, w.Ward_name, w.Ward_code }));
            }
            catch
            {
                return Json(Array.Empty<object>());
            }
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
                var (success, errorMessage) = await _authApiClient.RegisterStaffAsync(model);

                if (success)
                {
                    SetSuccessMessage("Tạo tài khoản nhân viên thành công!");
                    return RedirectToAction("Index", "Staff");
                }

                ModelState.AddModelError("", errorMessage ?? "Không thể tạo tài khoản nhân viên. Vui lòng kiểm tra thông tin đăng nhập và email.");
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

                var (success, errorMessage) = await _authApiClient.ChangePasswordAsync(model);

                if (success)
                {
                    ClearRememberMeCookies();
                    _logger.LogInformation("User {Username} changed password (via API)", model.Identifier);
                    SetSuccessMessage("Đổi mật khẩu thành công!");
                    return RedirectToAction("Index", "Order");
                }

                ModelState.AddModelError("", errorMessage ?? "Không thể đổi mật khẩu. Vui lòng kiểm tra mật khẩu hiện tại và chính sách mật khẩu mới.");
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
                var userExists = await _authApiClient.CheckEmailExistsAsync(email);

                SetSuccessMessage("Nếu email tồn tại trong hệ thống, bạn sẽ nhận được hướng dẫn đặt lại mật khẩu.");

                if (userExists)
                {
                    // TODO: G?i email reset password
                    _logger.LogInformation("Password reset requested for email: {Email}", email);
                }

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", email);
                ModelState.AddModelError("", "Không thể xử lý yêu cầu quên mật khẩu lúc này. Vui lòng thử lại sau ít phút.");
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

                var response = await _authApiClient.AutoLoginAsync(username, token);
                if (response == null)
                {
                    ClearRememberMeCookies();
                    return false;
                }

                SetUserSessionFromApiResult(response);
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

        private void SaveToSession<T>(string key, T value)
        {
            HttpContext.Session.SetString(key, JsonSerializer.Serialize(value));
        }

        private T? GetFromSession<T>(string key)
        {
            var raw = HttpContext.Session.GetString(key);
            if (string.IsNullOrWhiteSpace(raw)) return default;
            try
            {
                return JsonSerializer.Deserialize<T>(raw);
            }
            catch
            {
                return default;
            }
        }

        private void ClearCustomerRegisterSession()
        {
            HttpContext.Session.Remove(CustomerRegisterAccountSessionKey);
            HttpContext.Session.Remove(CustomerRegisterPersonalSessionKey);
            HttpContext.Session.Remove(CustomerRegisterOrganizationSessionKey);
        }
        private void SetUserSessionFromApiResult(LoginResponseDto dto)
        {
            HttpContext.Session.SetInt32("UserId", dto.UserId);
            HttpContext.Session.SetString("Username", dto.Username);
            HttpContext.Session.SetString("Email", dto.Email ?? "");
            HttpContext.Session.SetString("FullName", dto.FullName ?? dto.Username);
            HttpContext.Session.SetString("Roles", string.Join(",", dto.Roles ?? new List<string>()));
            HttpContext.Session.SetString("Permissions", string.Join(",", dto.Permissions ?? new List<string>()));
            HttpContext.Session.SetString("UserType", dto.UserType ?? "");
            HttpContext.Session.SetString("StaffCode", dto.StaffCode ?? "");
            HttpContext.Session.SetString("CustomerCode", dto.CustomerCode ?? "");
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
            var isCustomer = HttpContext.Session.IsCustomer();

            // Khách hàng luôn về Shop - không redirect đến trang nội bộ (Order, Home, Staff...)
            if (isCustomer)
            {
                HttpContext.Session.Remove("ReturnUrl");
                return RedirectToAction("Index", "Shop");
            }

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

            return RedirectToAction("Index", "Order");
        }

        #endregion
    }
}
