using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakanPergi.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueAddressForShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shop_Address",
                table: "Shop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shop_Address",
                table: "Shop",
                column: "Address",
                unique: true);
        }
    }
}
