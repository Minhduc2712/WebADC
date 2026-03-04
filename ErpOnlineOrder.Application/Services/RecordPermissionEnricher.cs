using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ErpOnlineOrder.Application.Services
{
    public static class RecordPermissionEnricher
    {
        public static async Task EnrichOrderAsync(OrderDTO dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichOrder(dto, userPermissions.ToHashSet());  
        }

        public static void EnrichOrder(OrderDTO dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.OrderUpdate);
            dto.AllowDelete = userPermissions.Contains(PermissionCodes.OrderDelete);
            dto.AllowApprove = userPermissions.Contains(PermissionCodes.OrderApprove);
            dto.AllowReject = userPermissions.Contains(PermissionCodes.OrderReject);
            dto.AllowExport = userPermissions.Contains(PermissionCodes.OrderExport);
        }

        public static async Task EnrichProductAsync(ProductDTO dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichProduct(dto, userPermissions.ToHashSet());
        }

        public static void EnrichProduct(ProductDTO dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.ProductUpdate);
            dto.AllowDelete = userPermissions.Contains(PermissionCodes.ProductDelete);
            dto.AllowExport = userPermissions.Contains(PermissionCodes.ProductExport);
            dto.AllowImport = userPermissions.Contains(PermissionCodes.ProductImport);
        }

        public static async Task EnrichInvoiceAsync(InvoiceDto dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichInvoice(dto, userPermissions.ToHashSet());
        }

        public static async Task EnrichInvoice(InvoiceDto dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.InvoiceUpdate);
            dto.AllowDelete = userPermissions.Contains(PermissionCodes.InvoiceDelete);
        }

        public static async Task EnrichAsync(IRecordPermissionDto dto, int userId, IPermissionService permissionService,
            string updateCode, string deleteCode)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            Enrich(dto, userPermissions.ToHashSet(), updateCode, deleteCode);
        }

        public static void Enrich(IRecordPermissionDto dto, IReadOnlySet<string> userPermissions, string updateCode, string deleteCode)
        {
            dto.AllowUpdate = userPermissions.Contains(updateCode);
            dto.AllowDelete = userPermissions.Contains(deleteCode);
        }

        public static async Task EnrichStaffAsync(StaffAccountDto dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichStaff(dto, userPermissions.ToHashSet());
        }

        public static void EnrichStaff(StaffAccountDto dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.StaffUpdate);
            dto.AllowDelete = userPermissions.Contains(PermissionCodes.StaffDelete);
            dto.AllowAssign = userPermissions.Contains(PermissionCodes.StaffAssignRole);
        }

        public static async Task EnrichSettingAsync(SystemSettingDto dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichSetting(dto, userPermissions.ToHashSet());
        }

        public static void EnrichSetting(SystemSettingDto dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.SettingsUpdate);
            dto.AllowDelete = false;
        }

        public static async Task EnrichWarehouseExportAsync(WarehouseExportDto dto, int userId, IPermissionService permissionService)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(userId);
            EnrichWarehouseExport(dto, userPermissions.ToHashSet());
        }

        public static void EnrichWarehouseExport(WarehouseExportDto dto, IReadOnlySet<string> userPermissions)
        {
            dto.AllowUpdate = userPermissions.Contains(PermissionCodes.WarehouseExportUpdate);
            dto.AllowDelete = userPermissions.Contains(PermissionCodes.WarehouseExportDelete);
        }
    }
}
