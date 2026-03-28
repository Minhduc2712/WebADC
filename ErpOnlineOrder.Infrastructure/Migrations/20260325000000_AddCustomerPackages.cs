using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    public partial class AddCustomerPackages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Customer_id = table.Column<int>(type: "int", nullable: false),
                    Package_id = table.Column<int>(type: "int", nullable: false),
                    Is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created_by = table.Column<int>(type: "int", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_by = table.Column<int>(type: "int", nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPackages_Customers_Customer_id",
                        column: x => x.Customer_id,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerPackages_Packages_Package_id",
                        column: x => x.Package_id,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPackages_Customer_id",
                table: "CustomerPackages",
                column: "Customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPackages_Package_id",
                table: "CustomerPackages",
                column: "Package_id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPackages_Customer_id_Package_id",
                table: "CustomerPackages",
                columns: new[] { "Customer_id", "Package_id" },
                unique: true,
                filter: "[Is_deleted] = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPackages");
        }
    }
}
