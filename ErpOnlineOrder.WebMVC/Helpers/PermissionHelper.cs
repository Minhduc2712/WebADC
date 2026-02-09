namespace ErpOnlineOrder.WebMVC.Helpers
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this ISession session, string permissionCode)
        {
            var permissionsStr = session.GetString("Permissions");
            if (string.IsNullOrEmpty(permissionsStr))
                return false;

            var permissions = permissionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
        }
        public static bool HasAnyPermission(this ISession session, params string[] permissionCodes)
        {
            var permissionsStr = session.GetString("Permissions");
            if (string.IsNullOrEmpty(permissionsStr))
                return false;

            var permissions = permissionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return permissionCodes.Any(p => permissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }
        public static bool HasAllPermissions(this ISession session, params string[] permissionCodes)
        {
            var permissionsStr = session.GetString("Permissions");
            if (string.IsNullOrEmpty(permissionsStr))
                return false;

            var permissions = permissionsStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return permissionCodes.All(p => permissions.Contains(p, StringComparer.OrdinalIgnoreCase));
        }
        public static bool CanView(this ISession session, string module)
        {
            return session.HasPermission($"{module}_VIEW");
        }
        public static bool CanCreate(this ISession session, string module)
        {
            return session.HasPermission($"{module}_CREATE");
        }
        public static bool CanUpdate(this ISession session, string module)
        {
            return session.HasPermission($"{module}_UPDATE");
        }
        public static bool CanDelete(this ISession session, string module)
        {
            return session.HasPermission($"{module}_DELETE");
        }
    }
}
