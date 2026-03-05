using AutoMapper;
using ErpOnlineOrder.Application.DTOs;
using ErpOnlineOrder.Application.DTOs.InvoiceDTOs;
using ErpOnlineOrder.Application.DTOs.ProductDTOs;
using ErpOnlineOrder.Application.DTOs.CustomerDTOs;
using ErpOnlineOrder.Application.DTOs.DistributorDTOs;
using ErpOnlineOrder.Application.DTOs.WarehouseDTOs;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Infrastructure.Mappers
{
    
    public class RepositoryMappingProfile : Profile
    {
        public RepositoryMappingProfile()
        {
            // Order -> OrderDTO
            CreateMap<Order, OrderDTO>()
                .ForMember(d => d.Order_code, o => o.MapFrom(s => s.Order_code ?? ""))
                .ForMember(d => d.Order_status, o => o.MapFrom(s => s.Order_status ?? ""))
                .ForMember(d => d.Customer_name, o => o.MapFrom(s =>
                    s.Customer != null
                        ? (s.Customer.Full_name != null && s.Customer.Full_name.Trim() != ""
                            ? s.Customer.Full_name
                            : (s.Customer.Customer_code ?? "—"))
                        : "—"))
                .ForMember(d => d.Order_details, o => o.MapFrom(s => s.Order_Details.Select(od => new OrderDetailDTO
                {
                    Product_id = od.Product_id,
                    Product_name = od.Product != null ? (od.Product.Product_name ?? "") : "",
                    Quantity = od.Quantity,
                    Unit_price = od.Unit_price,
                    Total_price = od.Total_price
                })));

            // Product -> ProductDTO (base: không có customerId, dùng Product_price)
            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.Product_code, o => o.MapFrom(s => s.Product_code ?? ""))
                .ForMember(d => d.Product_name, o => o.MapFrom(s => s.Product_name ?? ""))
                .ForMember(d => d.Product_description, o => o.MapFrom(s => s.Product_description ?? ""))
                .ForMember(d => d.Product_price, o => o.MapFrom(s => s.Product_price))
                .ForMember(d => d.Product_link, o => o.MapFrom(s => s.Product_link ?? ""))
                .ForMember(d => d.Publisher_name, o => o.MapFrom(s => s.Publisher != null ? (s.Publisher.Publisher_name ?? "") : ""))
                .ForMember(d => d.Authors, o => o.MapFrom(s => s.Product_Authors.Where(pa => pa.Author != null).Select(pa => pa.Author!.Author_name ?? "")))
                .ForMember(d => d.Categories, o => o.MapFrom(s => s.Product_Categories.Where(pc => pc.Category != null).Select(pc => pc.Category!.Category_name ?? "")))
                .ForMember(d => d.Images, o => o.MapFrom(s => s.Product_Images.Select(pi => pi.image_url ?? "")));

            // Product -> ProductSelectDto
            CreateMap<Product, ProductSelectDto>()
                .ForMember(d => d.Product_code, o => o.MapFrom(s => s.Product_code ?? ""))
                .ForMember(d => d.Product_name, o => o.MapFrom(s => s.Product_name ?? ""));

            // Invoice -> InvoiceDto
            CreateMap<Invoice, InvoiceDto>()
                .ForMember(d => d.Invoice_code, o => o.MapFrom(s => s.Invoice_code ?? ""))
                .ForMember(d => d.Customer_name, o => o.MapFrom(s => s.Customer != null ? (s.Customer.Full_name ?? "") : ""))
                .ForMember(d => d.Province_id, o => o.MapFrom(s => s.Customer != null
                    ? s.Customer.Customer_managements.Where(cm => cm.Province != null).Select(cm => (int?)cm.Province_id).FirstOrDefault()
                    : null))
                .ForMember(d => d.Province_name, o => o.MapFrom(s => s.Customer != null
                    ? s.Customer.Customer_managements.Where(cm => cm.Province != null).Select(cm => cm.Province!.Province_name).FirstOrDefault()
                    : null))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status ?? ""))
                .ForMember(d => d.Parent_invoice_code, o => o.MapFrom(s => s.Parent_invoice != null ? s.Parent_invoice.Invoice_code : null))
                .ForMember(d => d.Details, o => o.MapFrom(s => s.Invoice_Details.Select(d => new InvoiceDetailDto
                {
                    Id = d.Id,
                    Product_id = d.Product_id,
                    Product_name = d.Product != null ? (d.Product.Product_name ?? "") : "",
                    Quantity = d.Quantity,
                    Unit_price = d.Unit_price,
                    Total_price = d.Total_price,
                    Tax_rate = d.Tax_rate
                })));

            // Invoice -> InvoiceSelectDto
            CreateMap<Invoice, InvoiceSelectDto>()
                .ForMember(d => d.Invoice_code, o => o.MapFrom(s => s.Invoice_code ?? ""));

            // Customer -> CustomerSelectDto
            CreateMap<Customer, CustomerSelectDto>()
                .ForMember(d => d.Customer_code, o => o.MapFrom(s => s.Customer_code ?? ""))
                .ForMember(d => d.Full_name, o => o.MapFrom(s => s.Full_name ?? ""));

            // Warehouse -> WarehouseSelectDto
            CreateMap<Warehouse, WarehouseSelectDto>()
                .ForMember(d => d.Warehouse_name, o => o.MapFrom(s => s.Warehouse_name ?? ""));

            // Distributor -> DistributorSelectDto
            CreateMap<Distributor, DistributorSelectDto>()
                .ForMember(d => d.Distributor_code, o => o.MapFrom(s => s.Distributor_code ?? ""))
                .ForMember(d => d.Distributor_name, o => o.MapFrom(s => s.Distributor_name ?? ""));
        }
    }
}
