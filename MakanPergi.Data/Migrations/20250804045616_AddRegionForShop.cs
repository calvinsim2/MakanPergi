using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakanPergi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionForShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RegionId",
                table: "Shop",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shop_RegionId",
                table: "Shop",
                column: "RegionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shop_Shop_RegionId",
                table: "Shop",
                column: "RegionId",
                principalTable: "Shop",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shop_Shop_RegionId",
                table: "Shop");

            migrationBuilder.DropIndex(
                name: "IX_Shop_RegionId",
                table: "Shop");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Shop");
        }
    }
}
