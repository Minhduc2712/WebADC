using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceWardToStaffCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Province_id",
                table: "Staffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ward_id",
                table: "Staffs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Province_id",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Ward_id",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Province_id",
                table: "Staffs",
                column: "Province_id");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Ward_id",
                table: "Staffs",
                column: "Ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Province_id",
                table: "Customers",
                column: "Province_id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Ward_id",
                table: "Customers",
                column: "Ward_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Provinces_Province_id",
                table: "Staffs",
                column: "Province_id",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Wards_Ward_id",
                table: "Staffs",
                column: "Ward_id",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Provinces_Province_id",
                table: "Customers",
                column: "Province_id",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Wards_Ward_id",
                table: "Customers",
                column: "Ward_id",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Provinces_Province_id",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Wards_Ward_id",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Provinces_Province_id",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Wards_Ward_id",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_Province_id",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_Ward_id",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Province_id",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Ward_id",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Province_id",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Ward_id",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Province_id",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Ward_id",
                table: "Customers");
        }
    }
}
