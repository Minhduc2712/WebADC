using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationInformations_Organization_code",
                table: "OrganizationInformations",
                column: "Organization_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Product_code",
                table: "Products",
                column: "Product_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationInformations_Organization_code",
                table: "OrganizationInformations");

            migrationBuilder.DropIndex(
                name: "IX_Products_Product_code",
                table: "Products");
        }
    }
}
