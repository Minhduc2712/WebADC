using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.AdminDTOs;
using ErpOnlineOrder.Application.DTOs.AuthorDTOs;
using ErpOnlineOrder.Application.DTOs.CoverTypeDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerProductDTOs;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.OrderDTOs;
using ErpOnlineOrder.Application.DTOs.OrganizationDTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.DTOs.ProvinceDTOs;
using ErpOnlineOrder.Application.DTOs.PublisherDTOs;
using ErpOnlineOrder.Application.DTOs.RegionDTOs;
using ErpOnlineOrder.Application.DTOs.StockDTOs;
using ErpOnlineOrder.Application.DTOs.WardDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseExportDTOs;
using ErpOnlineOrder.Application.DTOs.PackageDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Application.Mappers
{

    public static class EntityMappers
    {
        #region Invoice

        public static InvoiceDto ToInvoiceDto(Invoice invoice)
        {
            var firstMgmt = invoice.Customer?.Customer_managements?
                .FirstOrDefault(cm => !cm.Is_deleted && cm.Province != null);
            var provinceId = firstMgmt?.Province_id;
            var provinceName = firstMgmt?.Province?.Province_name;

            return new InvoiceDto
            {
                Id = invoice.Id,
                Invoice_code = invoice.Invoice_code,
                Invoice_date = invoice.Invoice_date,
                Customer_id = invoice.Customer_id,
                Customer_name = invoice.Customer?.Full_name ?? "",
                Province_id = provinceId,
                Province_name = provinceName,
                Total_amount = invoice.Total_amount,
                Tax_amount = invoice.Tax_amount,
                Status = invoice.Status ?? "",
                Parent_invoice_id = invoice.Parent_invoice_id,
                Parent_invoice_code = invoice.Parent_invoice?.Invoice_code,
                Details = invoice.Invoice_Details
                    .Where(d => !d.Is_deleted)
                    .Select(d => new InvoiceDetailDto
                    {
                        Id = d.Id,
                        Product_id = d.Product_id,
                        Product_name = d.Product?.Product_name ?? "",
                        Quantity = d.Quantity,
                        Unit_price = d.Unit_price,
                        Total_price = d.Total_price,
                        Tax_rate = d.Tax_rate
                    }).ToList()
            };
        }

        #endregion

        #region Order

        public static OrderDTO ToOrderDto(Order order)
        {
            var customer = order.Customer;
            var customerName = customer != null
                ? (!string.IsNullOrWhiteSpace(customer.Full_name) ? customer.Full_name : customer.Customer_code ?? "—")
                : "—";

            return new OrderDTO
            {
                Id = order.Id,
                Order_code = order.Order_code ?? "",
                Order_date = order.Order_date,
                Total_price = order.Total_price,
                Order_status = order.Order_status ?? "",
                Customer_name = customerName,
                Shipping_address = order.Shipping_address,
                note = order.note,
                Order_details = order.Order_Details.Select(od => new OrderDetailDTO
                {
                    Product_id = od.Product_id,
                    Product_name = od.Product?.Product_name ?? "",
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price
                }).ToList()
            };
        }

        #endregion

        #region Product

        private static readonly System.Globalization.CultureInfo ViCulture = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

        public static ProductDTO ToProductDto(Product p)
        {
            return new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description ?? "",
                Product_price = p.Product_price,
                Product_link = p.Product_link ?? "",
                Publisher_name = p.Publisher?.Publisher_name ?? "",
                Authors = p.Product_Authors?
                    .Where(pa => pa?.Author != null)
                    .Select(pa => pa.Author!.Author_name ?? "")
                    .ToList() ?? new List<string>(),
                Categories = p.Product_Categories?
                    .Where(pc => pc?.Category != null)
                    .Select(pc => pc.Category!.Category_name ?? "")
                    .ToList() ?? new List<string>(),
                Images = p.Product_Images?
                    .Where(pi => pi != null)
                    .Select(pi => pi.image_url ?? "")
                    .ToList() ?? new List<string>()
            };
        }

        public static ProductDTO ToProductDtoForShop(Product p, int? customerId)
        {
            return new ProductDTO
            {
                Id = p.Id,
                Product_code = p.Product_code ?? "",
                Product_name = p.Product_name ?? "",
                Product_description = p.Product_description ?? "",
                Product_price = p.Product_price,
                Product_link = p.Product_link ?? "",
                Publisher_name = p.Publisher?.Publisher_name ?? "",
                Authors = p.Product_Authors?
                    .Where(pa => pa?.Author != null)
                    .Select(pa => pa.Author!.Author_name ?? "")
                    .ToList() ?? new List<string>(),
                Categories = p.Product_Categories?
                    .Where(pc => pc?.Category != null)
                    .Select(pc => pc.Category!.Category_name ?? "")
                    .ToList() ?? new List<string>(),
                Images = p.Product_Images?
                    .Where(pi => pi != null)
                    .Select(pi => pi.image_url ?? "")
                    .ToList() ?? new List<string>()
            };
        }

        #endregion

        #region WarehouseExport

        public static WarehouseExportDto ToWarehouseExportDto(Warehouse_export export)
        {
            return new WarehouseExportDto
            {
                Id = export.Id,
                Warehouse_export_code = export.Warehouse_export_code,
                Export_date = export.Export_date,
                Arrival_date = export.Arrival_date,
                Warehouse_id = export.Warehouse_id,
                Warehouse_name = export.Warehouse?.Warehouse_name ?? "",
                Invoice_code = export.Invoices != null && export.Invoices.Any() ? string.Join(", ", export.Invoices.Select(i => i.Invoice_code)) : null,
                Order_id = export.Order_id,
                Order_code = export.Order?.Order_code,
                Customer_id = export.Customer_id,
                Customer_name = export.Customer?.Full_name ?? "",
                Staff_id = export.Staff_id,
                Staff_name = export.Staff?.Full_name ?? "",
                Delivery_status = export.Delivery_status,
                Status = export.Status ?? "",
                Parent_export_id = export.Parent_export_id,
                Parent_export_code = export.Parent_export?.Warehouse_export_code,
                Merged_into_export_id = export.Merged_into_export_id,
                Merged_into_export_code = export.Merged_into_export?.Warehouse_export_code,
                Split_merge_note = export.Split_merge_note,
                Total_amount = export.Warehouse_Export_Details.Where(d => !d.Is_deleted).Sum(d => d.Total_price),
                Total_quantity = export.Warehouse_Export_Details.Where(d => !d.Is_deleted).Sum(d => d.Quantity_shipped),
                Created_at = export.Created_at,
                Details = export.Warehouse_Export_Details
                    .Where(d => !d.Is_deleted)
                    .Select(d => new WarehouseExportDetailDto
                    {
                        Id = d.Id,
                        Warehouse_export_id = d.Warehouse_export_id,
                        Warehouse_id = d.Warehouse_id,
                        Warehouse_name = d.Warehouse?.Warehouse_name ?? "",
                        Product_id = d.Product_id,
                        Product_code = d.Product?.Product_code ?? "",
                        Product_name = d.Product?.Product_name ?? "",
                        Quantity_shipped = d.Quantity_shipped,
                        Unit_price = d.Unit_price,
                        Total_price = d.Total_price
                    }).ToList()
            };
        }

        #endregion

        #region Customer

        public static CustomerDTO ToCustomerDto(Customer c)
        {
            return new CustomerDTO
            {
                Id = c.Id,
                Customer_code = c.Customer_code ?? "",
                Full_name = c.Full_name ?? "",
                Phone_number = c.Phone_number ?? "",
                Address = c.Address ?? "",
                Recipient_name = c.Recipient_name,
                Recipient_phone = c.Recipient_phone ?? "",
                Recipient_address = c.Recipient_address,
                Organization_information_id = c.Organization_information_id,
                Organization_name = c.Organization_information?.Organization_name ?? string.Empty,
                Created_at = c.Created_at,
                Updated_at = c.Updated_at,
                Is_deleted = c.Is_deleted,
                Username = c.User?.Username,
                Email = c.User?.Email
            };
        }

        #endregion

        #region Warehouse

        public static WarehouseDto ToWarehouseDto(Warehouse w)
        {
            return new WarehouseDto
            {
                Id = w.Id,
                Warehouse_code = w.Warehouse_code ?? "",
                Warehouse_name = w.Warehouse_name ?? "",
                Warehouse_address = w.Warehouse_address ?? "",
                Warehouse_phone = w.Warehouse_phone,
                Warehouse_email = w.Warehouse_email,
                Province_id = w.Province_id,
                Province_name = w.Province?.Province_name,
                Created_at = w.Created_at,
                Updated_at = w.Updated_at
            };
        }

        #endregion

        #region Stock

        public static StockDto ToStockDto(Stock s)
        {
            return new StockDto
            {
                Id = s.Id,
                Warehouse_id = s.Warehouse_id,
                Warehouse_name = s.Warehouse?.Warehouse_name ?? string.Empty,
                Product_id = s.Product_id,
                Product_name = s.Product?.Product_name ?? string.Empty,
                Quantity = s.Quantity,
                Updated_at = s.Updated_at
            };
        }

        #endregion

        #region Distributor

        public static DistributorDto ToDistributorDto(Distributor d)
        {
            return new DistributorDto
            {
                Id = d.Id,
                Distributor_code = d.Distributor_code ?? "",
                Distributor_name = d.Distributor_name ?? "",
                Distributor_address = d.Distributor_address ?? "",
                Distributor_phone = d.Distributor_phone ?? "",
                Distributor_email = d.Distributor_email ?? "",
                Created_at = d.Created_at,
                Updated_at = d.Updated_at
            };
        }

        #endregion

        #region CoverType

        public static CoverTypeDto ToCoverTypeDto(Cover_type c)
        {
            return new CoverTypeDto
            {
                Id = c.Id,
                Cover_type_code = c.Cover_type_code ?? "",
                Cover_type_name = c.Cover_type_name ?? "",
                Created_at = c.Created_at,
                Updated_at = c.Updated_at
            };
        }

        #endregion

        #region Author

        public static AuthorDto ToAuthorDto(Author a)
        {
            return new AuthorDto
            {
                Id = a.Id,
                Author_code = a.Author_code ?? "",
                Author_name = a.Author_name ?? "",
                Pen_name = a.Pen_name,
                Email_author = a.Email_author,
                Phone_number = a.Phone_number,
                birth_date = a.birth_date,
                death_date = a.death_date,
                Nationality = a.Nationality,
                Biography = a.Biography,
                Created_at = a.Created_at,
                Updated_at = a.Updated_at
            };
        }

        #endregion

        #region Category

        public static CategoryDto ToCategoryDto(Category c)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Category_code = c.Category_code ?? "",
                Category_name = c.Category_name ?? "",
                Created_at = c.Created_at,
                Updated_at = c.Updated_at
            };
        }

        #endregion

        #region Publisher

        public static PublisherDto ToPublisherDto(Publisher p)
        {
            return new PublisherDto
            {
                Id = p.Id,
                Publisher_code = p.Publisher_code ?? "",
                Publisher_name = p.Publisher_name ?? "",
                Publisher_address = p.Publisher_address ?? "",
                Publisher_phone = p.Publisher_phone ?? "",
                Publisher_email = p.Publisher_email ?? "",
                Created_at = p.Created_at,
                Updated_at = p.Updated_at
            };
        }

        #endregion

        #region CustomerProduct

        public static CustomerProductDto ToCustomerProductDto(Customer_product cp)
        {
            return new CustomerProductDto
            {
                Id = cp.Id,
                Customer_id = cp.Customer_id,
                Customer_name = cp.Customer?.Full_name ?? string.Empty,
                Product_id = cp.Product_id,
                Product_code = cp.Product?.Product_code ?? string.Empty,
                Product_name = cp.Product?.Product_name ?? string.Empty,
                Original_price = cp.Product?.Product_price ?? 0,
                Max_quantity = cp.Max_quantity,
                Is_active = cp.Is_active,
                Is_Excluded = cp.Is_Excluded,
                Created_at = cp.Created_at
            };
        }

        #endregion

        #region Organization

        public static OrganizationDTO ToOrganizationDto(Organization_information org)
        {
            return new OrganizationDTO
            {
                Id = org.Id,
                Organization_code = org.Organization_code ?? "",
                Organization_name = org.Organization_name ?? "",
                Address = org.Address ?? "",
                Tax_number = org.Tax_number,
                Created_at = org.Created_at,
                Updated_at = org.Updated_at
            };
        }

        #endregion

        #region Province

        public static ProvinceDTO ToProvinceDto(Province province)
        {
            return new ProvinceDTO
            {
                Id = province.Id,
                Province_code = province.Province_code ?? "",
                Province_name = province.Province_name ?? "",
                Region_id = province.Region_id,
                Region_name = province.Region?.Region_name ?? string.Empty,
                Created_at = province.Created_at,
                Updated_at = province.Updated_at
            };
        }

        #endregion

        #region Region

        public static RegionDTO ToRegionDto(Region region)
        {
            return new RegionDTO
            {
                Id = region.Id,
                Region_code = region.Region_code ?? "",
                Region_name = region.Region_name ?? "",
                Province_count = region.Provinces?.Count(p => !p.Is_deleted) ?? 0,
                Created_at = region.Created_at,
                Updated_at = region.Updated_at
            };
        }

        #endregion

        #region Ward

        public static WardDTO ToWardDto(Ward ward)
        {
            return new WardDTO
            {
                Id = ward.Id,
                Ward_code = ward.Ward_code ?? "",
                Ward_name = ward.Ward_name ?? "",
                Province_id = ward.Province_id,
                Province_name = ward.Province?.Province_name ?? string.Empty,
                Created_at = ward.Created_at,
                Updated_at = ward.Updated_at
            };
        }

        #endregion

        #region Staff (User -> StaffAccountDto)

        public static StaffAccountDto ToStaffAccountDto(User user)
        {
            return new StaffAccountDto
            {
                User_id = user.Id,
                Staff_id = user.Staff?.Id ?? 0,
                Staff_code = user.Staff?.Staff_code ?? string.Empty,
                Username = user.Username ?? "",
                Email = user.Email ?? "",
                Full_name = user.Staff?.Full_name ?? string.Empty,
                Phone_number = user.Staff?.Phone_number,
                Is_active = user.Is_active,
                Roles = user.User_roles
                    .Where(ur => !ur.Is_deleted && ur.Role != null && !ur.Role.Is_deleted)
                    .Select(ur => ur.Role!.Role_name ?? "")
                    .ToList(),
                Created_at = user.Created_at
            };
        }

        #endregion

        #region Package

        public static PackageDto ToPackageDto(Package p)
        {
            return new PackageDto
            {
                Id = p.Id,
                Package_code = p.Package_code,
                Package_name = p.Package_name,
                Description = p.Description,
                Organization_information_id = p.Organization_information_id,
                Organization_name = p.Organization_information?.Organization_name,
                Region_id = p.Region_id,
                Region_name = p.Region?.Region_name,
                Province_id = p.Province_id,
                Province_name = p.Province?.Province_name,
                Ward_id = p.Ward_id,
                Ward_name = p.Ward?.Ward_name,
                Is_active = p.Is_active,
                Created_at = p.Created_at,
                Updated_at = p.Updated_at,
                Products = p.Package_products
                    .Where(pp => !pp.Is_deleted)
                    .Select(pp => new PackageProductDto
                    {
                        Id = pp.Id,
                        Package_id = pp.Package_id,
                        Product_id = pp.Product_id,
                        Product_code = pp.Product?.Product_code ?? string.Empty,
                        Product_name = pp.Product?.Product_name ?? string.Empty,
                        Original_price = pp.Product?.Product_price ?? 0,
                        Is_active = pp.Is_active
                    }).ToList()
            };
        }

        #endregion
    }
}