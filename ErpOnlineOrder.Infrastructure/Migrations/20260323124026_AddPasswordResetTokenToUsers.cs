using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokenToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Warehouse_email",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Warehouse_phone",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password_reset_token",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Password_reset_token_expiry",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Tax_number",
                table: "OrganizationInformations",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Recipient_phone",
                table: "OrganizationInformations",
                type: "nvarchar(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Ward_id",
                table: "CustomerManagements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StaffRegionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Staff_id = table.Column<int>(type: "int", nullable: false),
                    Province_id = table.Column<int>(type: "int", nullable: false),
                    Ward_ids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created_by = table.Column<int>(type: "int", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_by = table.Column<int>(type: "int", nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRegionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffRegionRules_Provinces_Province_id",
                        column: x => x.Province_id,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffRegionRules_Staffs_Staff_id",
                        column: x => x.Staff_id,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ward_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ward_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Province_id = table.Column<int>(type: "int", nullable: false),
                    Created_by = table.Column<int>(type: "int", nullable: false),
                    Created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_by = table.Column<int>(type: "int", nullable: false),
                    Updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wards_Provinces_Province_id",
                        column: x => x.Province_id,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerManagements_Ward_id",
                table: "CustomerManagements",
                column: "Ward_id");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRegionRules_Province_id",
                table: "StaffRegionRules",
                column: "Province_id");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRegionRules_Staff_id_Province_id",
                table: "StaffRegionRules",
                columns: new[] { "Staff_id", "Province_id" },
                unique: true,
                filter: "[Is_deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_Province_id",
                table: "Wards",
                column: "Province_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerManagements_Wards_Ward_id",
                table: "CustomerManagements",
                column: "Ward_id",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerManagements_Wards_Ward_id",
                table: "CustomerManagements");

            migrationBuilder.DropTable(
                name: "StaffRegionRules");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropIndex(
                name: "IX_CustomerManagements_Ward_id",
                table: "CustomerManagements");

            migrationBuilder.DropColumn(
                name: "Warehouse_email",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Warehouse_phone",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Password_reset_token",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Password_reset_token_expiry",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Ward_id",
                table: "CustomerManagements");

            migrationBuilder.AlterColumn<int>(
                name: "Tax_number",
                table: "OrganizationInformations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "Recipient_phone",
                table: "OrganizationInformations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)");
        }
    }
}
