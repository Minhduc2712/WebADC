using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpOnlineOrder.Infrastructure.Migrations
{
    public partial class RemoveModuleActionAddParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action_id",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Module_id",
                table: "Permissions");

            migrationBuilder.AddColumn<int>(
                name: "Parent_id",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Is_special",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Parent_id",
                table: "Permissions",
                column: "Parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Permissions_Parent_id",
                table: "Permissions",
                column: "Parent_id",
                principalTable: "Permissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Permissions_Parent_id",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Parent_id",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Is_special",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Parent_id",
                table: "Permissions");

            migrationBuilder.AddColumn<int>(
                name: "Action_id",
                table: "Permissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Module_id",
                table: "Permissions",
                type: "int",
                nullable: true);
        }
    }
}
