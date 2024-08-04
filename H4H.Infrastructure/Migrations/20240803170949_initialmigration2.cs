using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H4H.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ItemId",
                table: "Addresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ItemId",
                table: "Addresses",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Items_ItemId",
                table: "Addresses",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Items_ItemId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_ItemId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Addresses");
        }
    }
}
