using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiTokenToInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiToken",
                table: "Inventories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiToken",
                table: "Inventories");
        }
    }
}
