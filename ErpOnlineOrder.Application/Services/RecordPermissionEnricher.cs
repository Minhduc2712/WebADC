using ErpOnlineOrder.Application.Constants;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.SettingsDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace ErpOnlineOrder.Application.Services
{
    public static class RecordPermissionEnricher
    {
        public static async Task EnrichOrderAsync(OrderDTO dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.OrderUpdate);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, PermissionCodes.OrderDelete);
            dto.AllowApprove = await permissionService.HasPermissionAsync(userId, PermissionCodes.OrderApprove);
            dto.AllowReject = await permissionService.HasPermissionAsync(userId, PermissionCodes.OrderReject);
            dto.AllowExport = await permissionService.HasPermissionAsync(userId, PermissionCodes.OrderExport);
        }

        public static async Task EnrichProductAsync(ProductDTO dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.ProductUpdate);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, PermissionCodes.ProductDelete);
            dto.AllowExport = await permissionService.HasPermissionAsync(userId, PermissionCodes.ProductExport);
            dto.AllowImport = await permissionService.HasPermissionAsync(userId, PermissionCodes.ProductImport);
        }

        public static async Task EnrichInvoiceAsync(InvoiceDto dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.InvoiceUpdate);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, PermissionCodes.InvoiceDelete);
        }

        public static async Task EnrichAsync(IRecordPermissionDto dto, int userId, IPermissionService permissionService,
            string updateCode, string deleteCode)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, updateCode);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, deleteCode);
        }

        public static async Task EnrichStaffAsync(StaffAccountDto dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.StaffUpdate);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, PermissionCodes.StaffDelete);
            dto.AllowAssign = await permissionService.HasPermissionAsync(userId, PermissionCodes.StaffAssignRole);
        }

        public static async Task EnrichSettingAsync(SystemSettingDto dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.SettingsUpdate);
            dto.AllowDelete = false;
        }

        public static async Task EnrichWarehouseExportAsync(WarehouseExportDto dto, int userId, IPermissionService permissionService)
        {
            dto.AllowUpdate = await permissionService.HasPermissionAsync(userId, PermissionCodes.WarehouseExportUpdate);
            dto.AllowDelete = await permissionService.HasPermissionAsync(userId, PermissionCodes.WarehouseExportDelete);
        }
    }
}
