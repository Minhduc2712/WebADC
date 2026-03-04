namespace ErpOnlineOrder.WebMVC.Extensions
{
    public static class SessionExtensions
    {
        #region User Info
        public static int GetUserId(this ISession session)
        {
            return session.GetInt32("UserId") ?? 0;
        }
        public static string GetUsername(this ISession session)
        {
            return session.GetString("Username") ?? "";
        }
        public static string GetEmail(this ISession session)
        {
            return session.GetString("Email") ?? "";
        }
        public static string GetFullName(this ISession session)
        {
            return session.GetString("FullName") ?? session.GetUsername();
        }
        public static string GetUserType(this ISession session)
        {
            return session.GetString("UserType") ?? "";
        }
        public static bool IsStaff(this ISession session)
        {
            return session.GetUserType() == "Staff";
        }
        public static bool IsCustomer(this ISession session)
        {
            return session.GetUserType() == "Customer";
        }

        #endregion

        #region Authentication
        public static bool IsAuthenticated(this ISession session)
        {
            var userId = session.GetInt32("UserId");
            var username = session.GetString("Username");
            return userId.HasValue && userId.Value > 0 && !string.IsNullOrEmpty(username);
        }
        public static DateTime? GetLoginTime(this ISession session)
        {
            var loginTime = session.GetString("LoginTime");
            if (DateTime.TryParse(loginTime, out var result))
            {
                return result;
            }
            return null;
        }

        #endregion

        #region Roles
        public static List<string> GetRoles(this ISession session)
        {
            var roles = session.GetString("Roles") ?? "";
            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public static bool HasRole(this ISession session, string roleName)
        {
            return session.GetRoles().Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }
        public static bool HasAnyRole(this ISession session, params string[] roleNames)
        {
            var userRoles = session.GetRoles();
            return roleNames.Any(r => userRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
        }

        #endregion

        #region Permissions
        public static List<string> GetPermissions(this ISession session)
        {
            var permissions = session.GetString("Permissions") ?? "";
            return permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        public static bool HasPermission(this ISession session, string permissionCode)
        {
            // ROLE_ADMIN bypass - admin có toàn quyền
            if (session.HasRole("ROLE_ADMIN")) return true;
            return session.GetPermissions().Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
        }
        public static bool HasAnyPermission(this ISession session, params string[] permissionCodes)
        {
            if (session.HasRole("ROLE_ADMIN")) return true;
            var userPermissions = session.GetPermissions();
            return permissionCodes.Any(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }
        public static bool HasAllPermissions(this ISession session, params string[] permissionCodes)
        {
            if (session.HasRole("ROLE_ADMIN")) return true;
            var userPermissions = session.GetPermissions();
            return permissionCodes.All(p => userPermissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }

        #endregion

        #region Set Session
        public static void SetUserInfo(
            this ISession session,
            int userId,
            string username,
            string email,
            string fullName,
            string userType,
            IEnumerable<string> roles,
            IEnumerable<string> permissions)
        {
            session.SetInt32("UserId", userId);
            session.SetString("Username", username);
            session.SetString("Email", email);
            session.SetString("FullName", fullName);
            session.SetString("UserType", userType);
            session.SetString("Roles", string.Join(",", roles));
            session.SetString("Permissions", string.Join(",", permissions));
            session.SetString("LoginTime", DateTime.Now.ToString("o"));
        }

        #endregion
    }
}
