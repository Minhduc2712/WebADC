using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    public partial class AddPackageAndIsExcluded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Is_Excluded to CustomerProducts (idempotent)
            migrationBuilder.Sql(@"IF COL_LENGTH('CustomerProducts', 'Is_Excluded') IS NULL
                ALTER TABLE [CustomerProducts] ADD [Is_Excluded] bit NOT NULL DEFAULT 0;");

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Package_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Package_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Organization_information_id = table.Column<int>(type: "int", nullable: true),
                    Region_id = table.Column<int>(type: "int", nullable: true),
                    Province_id = table.Column<int>(type: "int", nullable: true),
                    Ward_id = table.Column<int>(type: "int", nullable: true),
                    Is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created_by = table.Column<int>(type: "int", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_by = table.Column<int>(type: "int", nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Packages_OrganizationInformations_Organization_information_id",
                        column: x => x.Organization_information_id,
                        principalTable: "OrganizationInformations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packages_Regions_Region_id",
                        column: x => x.Region_id,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packages_Provinces_Province_id",
                        column: x => x.Province_id,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Packages_Wards_Ward_id",
                        column: x => x.Ward_id,
                        principalTable: "Wards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Package_id = table.Column<int>(type: "int", nullable: false),
                    Product_id = table.Column<int>(type: "int", nullable: false),
                    Is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Created_by = table.Column<int>(type: "int", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_by = table.Column<int>(type: "int", nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageProducts_Packages_Package_id",
                        column: x => x.Package_id,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageProducts_Products_Product_id",
                        column: x => x.Product_id,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Organization_information_id",
                table: "Packages",
                column: "Organization_information_id");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Region_id",
                table: "Packages",
                column: "Region_id");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Province_id",
                table: "Packages",
                column: "Province_id");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_Ward_id",
                table: "Packages",
                column: "Ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_PackageProducts_Package_id_Product_id",
                table: "PackageProducts",
                columns: new[] { "Package_id", "Product_id" },
                unique: true,
                filter: "[Is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PackageProducts_Product_id",
                table: "PackageProducts",
                column: "Product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageProducts");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropColumn(
                name: "Is_Excluded",
                table: "CustomerProducts");
        }
    }
}
