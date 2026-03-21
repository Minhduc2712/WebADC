using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWardsTableAndWardId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tạo bảng Wards
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
                name: "IX_Wards_Province_id",
                table: "Wards",
                column: "Province_id");

            // 2. Thêm cột Ward_id (nullable) vào bảng CustomerManagements
            migrationBuilder.AddColumn<int>(
                name: "Ward_id",
                table: "CustomerManagements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerManagements_Ward_id",
                table: "CustomerManagements",
                column: "Ward_id");

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

            migrationBuilder.DropIndex(
                name: "IX_CustomerManagements_Ward_id",
                table: "CustomerManagements");

            migrationBuilder.DropColumn(
                name: "Ward_id",
                table: "CustomerManagements");

            migrationBuilder.DropTable(name: "Wards");
        }
    }
}
