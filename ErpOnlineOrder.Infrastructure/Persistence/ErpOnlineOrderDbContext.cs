using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ErpOnlineOrder.Domain.Models;

namespace ErpOnlineOrder.Infrastructure.Persistence
{
    public class ErpOnlineOrderDbContext : DbContext
    {
        public ErpOnlineOrderDbContext(
            DbContextOptions<ErpOnlineOrderDbContext> options)
            : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users => Set<User>();
        public DbSet<Staff> Staffs => Set<Staff>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User_role> UserRoles => Set<User_role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Role_permission> RolePermissions => Set<Role_permission>();
        public DbSet<User_permission> UserPermissions => Set<User_permission>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Product_image> ProductImages => Set<Product_image>();
        public DbSet<Product_author> ProductAuthors => Set<Product_author>();
        public DbSet<Product_category> ProductCategories => Set<Product_category>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Publisher> Publishers => Set<Publisher>();
        public DbSet<Cover_type> CoverTypes => Set<Cover_type>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Order_detail> OrderDetails => Set<Order_detail>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Invoice_detail> InvoiceDetails => Set<Invoice_detail>();
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<Warehouse_export> WarehouseExports => Set<Warehouse_export>();
        public DbSet<Warehouse_export_detail> WarehouseExportDetails => Set<Warehouse_export_detail>();
        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<Distributor> Distributors => Set<Distributor>();
        public DbSet<Region> Regions => Set<Region>();
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<Customer_management> CustomerManagements => Set<Customer_management>();
        public DbSet<Organization_information> OrganizationInformations => Set<Organization_information>();
        public DbSet<Customer_product> CustomerProducts => Set<Customer_product>();
        public DbSet<Customer_category> CustomerCategories => Set<Customer_category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Password).IsRequired();
                entity.Property(x => x.Is_active).HasColumnName("Is_active");
                entity.Property(x => x.Created_by).HasColumnName("Created_by");
                entity.Property(x => x.Created_at).HasColumnName("Created_at");
                entity.Property(x => x.Updated_by).HasColumnName("Updated_by");
                entity.Property(x => x.Updated_at).HasColumnName("Updated_at");
                entity.Property(x => x.Is_deleted).HasColumnName("Is_deleted");
                entity.HasMany(x => x.User_roles).WithOne(x => x.User).HasForeignKey(x => x.User_id);
                entity.HasOne(x => x.Staff).WithOne(x => x.User).HasForeignKey<Staff>(x => x.User_id);
                entity.HasOne(x => x.Customer).WithOne(x => x.User).HasForeignKey<Customer>(x => x.User_id);
            });

            // Staff configuration
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staffs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Staff_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Full_name).HasMaxLength(100);
                entity.Property(x => x.Phone_number).HasMaxLength(20);
                entity.Property(x => x.User_id).HasColumnName("User_id");
                entity.HasOne(x => x.User).WithOne(x => x.Staff).HasForeignKey<Staff>(x => x.User_id);
                
                // Cấu hình relationship với Invoice và Warehouse_export
                entity.HasMany(x => x.Invoices)
                    .WithOne(x => x.Staff)
                    .HasForeignKey(x => x.Staff_id)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasMany(x => x.Warehouse_exports)
                    .WithOne(x => x.Staff)
                    .HasForeignKey(x => x.Staff_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Customer_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Full_name).HasMaxLength(100);
                entity.Property(x => x.Phone_number).HasMaxLength(20);
                entity.Property(x => x.Address).HasMaxLength(200);
                entity.Property(x => x.User_id).HasColumnName("User_id");
                entity.HasOne(x => x.User).WithOne(x => x.Customer).HasForeignKey<Customer>(x => x.User_id);
            });

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Role_name).HasMaxLength(50).IsRequired();
                entity.HasMany(x => x.User_roles).WithOne(x => x.Role).HasForeignKey(x => x.Role_id);
                entity.HasMany(x => x.Role_Permissions).WithOne(x => x.Role).HasForeignKey(x => x.RoleId);
            });

            // User_role configuration
            modelBuilder.Entity<User_role>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.User_id).HasColumnName("User_id");
                entity.Property(x => x.Role_id).HasColumnName("Role_id");
                entity.HasOne(x => x.User).WithMany(x => x.User_roles).HasForeignKey(x => x.User_id);
                entity.HasOne(x => x.Role).WithMany(x => x.User_roles).HasForeignKey(x => x.Role_id);
            });

            // Permission configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Permission_code).HasMaxLength(100).IsRequired();
                // Module_id va Action_id la optional
                entity.Property(x => x.Module_id).IsRequired(false);
                entity.Property(x => x.Action_id).IsRequired(false);
            });

            // Role_permission configuration
            modelBuilder.Entity<Role_permission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.RoleId).HasColumnName("RoleId");
                entity.Property(x => x.PermissionId).HasColumnName("PermissionId");
                entity.Property(x => x.Created_by).HasColumnName("Created_by");
                entity.Property(x => x.Created_at).HasColumnName("Created_at");
                entity.Property(x => x.Updated_by).HasColumnName("Updated_by");
                entity.Property(x => x.Updated_at).HasColumnName("Updated_at");
                entity.Property(x => x.Is_deleted).HasColumnName("Is_deleted");
                entity.HasOne(x => x.Role).WithMany(x => x.Role_Permissions).HasForeignKey(x => x.RoleId);
                entity.HasOne(x => x.Permission).WithMany(x => x.Role_Permissions).HasForeignKey(x => x.PermissionId);
            });

            // User_permission configuration (quyen gan truc tiep cho user)
            modelBuilder.Entity<User_permission>(entity =>
            {
                entity.ToTable("UserPermissions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).HasColumnName("UserId");
                entity.Property(x => x.PermissionId).HasColumnName("PermissionId");
                entity.Property(x => x.Note).HasMaxLength(500);
                entity.HasOne(x => x.User).WithMany(x => x.User_permissions).HasForeignKey(x => x.UserId);
                entity.HasOne(x => x.Permission).WithMany(x => x.User_Permissions).HasForeignKey(x => x.PermissionId);
                entity.HasIndex(x => new { x.UserId, x.PermissionId }).IsUnique();
            });

            // Customer_product configuration
            modelBuilder.Entity<Customer_product>(entity =>
            {
                entity.ToTable("CustomerProducts");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.Property(x => x.Custom_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Discount_percent).HasColumnType("decimal(5,2)");
                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Customer_Products)
                    .HasForeignKey(x => x.Customer_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Customer_Products)
                    .HasForeignKey(x => x.Product_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.Customer_id, x.Product_id }).IsUnique();
            });

            // Customer_category configuration
            modelBuilder.Entity<Customer_category>(entity =>
            {
                entity.ToTable("CustomerCategories");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.Property(x => x.Category_id).HasColumnName("Category_id");
                entity.Property(x => x.Discount_percent).HasColumnType("decimal(5,2)");
                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Customer_Categories)
                    .HasForeignKey(x => x.Customer_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Customer_Categories)
                    .HasForeignKey(x => x.Category_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.Customer_id, x.Category_id }).IsUnique();
            });

            // Invoice configuration
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Invoice_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Total_amount).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Tax_amount).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Status).HasMaxLength(20);
                entity.Property(x => x.Split_merge_note).HasMaxLength(500);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.Property(x => x.Staff_id).HasColumnName("Staff_id");
                entity.Property(x => x.Order_id).HasColumnName("Order_id");
                entity.Property(x => x.Warehouse_export_id).HasColumnName("Warehouse_export_id");
                entity.Property(x => x.Parent_invoice_id).HasColumnName("Parent_invoice_id");
                entity.Property(x => x.Merged_into_invoice_id).HasColumnName("Merged_into_invoice_id");
                entity.HasOne(x => x.Customer).WithMany(x => x.Invoices).HasForeignKey(x => x.Customer_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Order).WithMany(x => x.Invoices).HasForeignKey(x => x.Order_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Parent_invoice).WithMany(x => x.Child_invoices).HasForeignKey(x => x.Parent_invoice_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Merged_into_invoice).WithMany(x => x.Merged_invoices).HasForeignKey(x => x.Merged_into_invoice_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Invoice_detail configuration
            modelBuilder.Entity<Invoice_detail>(entity =>
            {
                entity.ToTable("InvoiceDetails");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Unit_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Total_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Tax_rate).HasColumnType("decimal(5,2)");
                entity.Property(x => x.Invoice_id).HasColumnName("Invoice_id");
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.HasOne(x => x.Invoice).WithMany(x => x.Invoice_Details).HasForeignKey(x => x.Invoice_id).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Product).WithMany(x => x.Invoice_Details).HasForeignKey(x => x.Product_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Warehouse_export configuration
            modelBuilder.Entity<Warehouse_export>(entity =>
            {
                entity.ToTable("WarehouseExports");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Warehouse_export_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Carrier_name).HasMaxLength(100);
                entity.Property(x => x.Tracking_number).HasMaxLength(100);
                entity.Property(x => x.Delivery_status).HasMaxLength(20);
                entity.Property(x => x.Status).HasMaxLength(20);
                entity.Property(x => x.Split_merge_note).HasMaxLength(500);
                entity.Property(x => x.Warehouse_id).HasColumnName("Warehouse_id");
                entity.Property(x => x.Invoice_id).HasColumnName("Invoice_id");
                entity.Property(x => x.Order_id).HasColumnName("Order_id");
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.Property(x => x.Staff_id).HasColumnName("Staff_id");
                entity.Property(x => x.Parent_export_id).HasColumnName("Parent_export_id");
                entity.Property(x => x.Merged_into_export_id).HasColumnName("Merged_into_export_id");
                entity.HasOne(x => x.Warehouse)
                    .WithMany(x => x.Warehouse_Exports)
                    .HasForeignKey(x => x.Warehouse_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.Invoice_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Order).WithMany().HasForeignKey(x => x.Order_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.Customer_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Parent_export).WithMany(x => x.Child_exports).HasForeignKey(x => x.Parent_export_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Merged_into_export).WithMany(x => x.Merged_exports).HasForeignKey(x => x.Merged_into_export_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Warehouse_export_detail configuration
            modelBuilder.Entity<Warehouse_export_detail>(entity =>
            {
                entity.ToTable("WarehouseExportDetails");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Unit_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Total_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Warehouse_export_id).HasColumnName("Warehouse_export_id");
                entity.Property(x => x.Warehouse_id).HasColumnName("Warehouse_id");
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.HasOne(x => x.Warehouse_export).WithMany(x => x.Warehouse_Export_Details).HasForeignKey(x => x.Warehouse_export_id).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.Warehouse_id).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.Product_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Product_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Product_name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Product_price).HasMaxLength(50);
                entity.Property(x => x.Tax_rate).HasColumnType("decimal(5,2)");
                entity.Property(x => x.Cover_type_id).HasColumnName("Cover_type_id");
                entity.Property(x => x.Publisher_id).HasColumnName("Publisher_id");
                entity.Property(x => x.Distributor_id).HasColumnName("Distributor_id");
                
                // Cau hinh relationship - cho phep NULL va chi ro navigation property
                entity.HasOne(x => x.Publisher)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.Publisher_id)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(x => x.Cover_type)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.Cover_type_id)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(x => x.Distributor)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.Distributor_id)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Product_category configuration
            modelBuilder.Entity<Product_category>(entity =>
            {
                entity.ToTable("ProductCategories");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.Property(x => x.Category_id).HasColumnName("Category_id");
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Product_Categories)
                    .HasForeignKey(x => x.Product_id)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Product_Categories)
                    .HasForeignKey(x => x.Category_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product_author configuration
            modelBuilder.Entity<Product_author>(entity =>
            {
                entity.ToTable("ProductAuthors");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.Property(x => x.Author_id).HasColumnName("Author_id");
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Product_Authors)
                    .HasForeignKey(x => x.Product_id)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Author)
                    .WithMany(x => x.Product_Authors)
                    .HasForeignKey(x => x.Author_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product_image configuration
            modelBuilder.Entity<Product_image>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.Property(x => x.image_url).HasColumnName("Image_url");
                entity.Property(x => x.Is_primary).HasColumnName("Is_main");
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Product_Images)
                    .HasForeignKey(x => x.Product_id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Category_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Category_name).HasMaxLength(100).IsRequired();
            });

            // Author configuration
            modelBuilder.Entity<Author>(entity =>
            {
                entity.ToTable("Authors");
                entity.HasKey(x => x.Id);
            });

            // Publisher configuration
            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.ToTable("Publishers");
                entity.HasKey(x => x.Id);
            });

            // Cover_type configuration
            modelBuilder.Entity<Cover_type>(entity =>
            {
                entity.ToTable("CoverTypes");
                entity.HasKey(x => x.Id);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Order_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Total_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Order_status).HasMaxLength(20);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.Customer_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Order_detail configuration
            modelBuilder.Entity<Order_detail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Unit_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Total_price).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Tax_rate).HasColumnType("decimal(5,2)");
                entity.Property(x => x.Order_id).HasColumnName("Order_id");
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.HasOne(x => x.Order).WithMany(x => x.Order_Details).HasForeignKey(x => x.Order_id).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Product).WithMany(x => x.Order_Details).HasForeignKey(x => x.Product_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Warehouse configuration
            modelBuilder.Entity<Warehouse>(entity =>
            {
                entity.ToTable("Warehouses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Warehouse_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Warehouse_name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Warehouse_address).HasMaxLength(500).IsRequired();
                entity.Property(x => x.Province_id).HasColumnName("Province_id");
                entity.HasOne(x => x.Province)
                    .WithMany()
                    .HasForeignKey(x => x.Province_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.Stocks)
                    .WithOne(x => x.Warehouse)
                    .HasForeignKey(x => x.Warehouse_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Stock configuration
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("Stocks");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Warehouse_id).HasColumnName("Warehouse_id");
                entity.Property(x => x.Product_id).HasColumnName("Product_id");
                entity.HasOne(x => x.Product)
                    .WithMany(x => x.Stocks)
                    .HasForeignKey(x => x.Product_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Region configuration
            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Regions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Region_code).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Region_name).HasMaxLength(100).IsRequired();
            });

            // Province configuration
            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("Provinces");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Province_code).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Province_name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Region_id).HasColumnName("Region_id");
                entity.HasOne(x => x.Region).WithMany(x => x.Provinces).HasForeignKey(x => x.Region_id).OnDelete(DeleteBehavior.Restrict);
            });

            // Distributor configuration
            modelBuilder.Entity<Distributor>(entity =>
            {
                entity.ToTable("Distributors");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Distributor_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Distributor_name).HasMaxLength(200).IsRequired();
            });

            // Organization_information configuration
            modelBuilder.Entity<Organization_information>(entity =>
            {
                entity.ToTable("OrganizationInformations");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Organization_code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Organization_name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Address).HasMaxLength(500);
                entity.Property(x => x.Recipient_name).HasMaxLength(100);
                entity.Property(x => x.Recipient_address).HasMaxLength(500);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.HasOne(x => x.Customer).WithMany(x => x.Organization_informations).HasForeignKey(x => x.Customer_id).OnDelete(DeleteBehavior.Cascade);
            });

            // Customer_management configuration
            modelBuilder.Entity<Customer_management>(entity =>
            {
                entity.ToTable("CustomerManagements");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Customer_id).HasColumnName("Customer_id");
                entity.Property(x => x.Staff_id).HasColumnName("Staff_id");
                entity.Property(x => x.Province_id).HasColumnName("Province_id");
                entity.HasOne(x => x.Customer)
                    .WithMany(x => x.Customer_managements)
                    .HasForeignKey(x => x.Customer_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Staff)
                    .WithMany(x => x.Customer_managements)
                    .HasForeignKey(x => x.Staff_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Province)
                    .WithMany(x => x.Customer_managements)
                    .HasForeignKey(x => x.Province_id)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.Staff_id, x.Customer_id }).IsUnique();
            });
        }
    }
}
