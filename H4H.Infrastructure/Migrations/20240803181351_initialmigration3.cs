using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace H4H.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initialmigration3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "AddressItem",
                columns: table => new
                {
                    AddressesAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemsItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressItem", x => new { x.AddressesAddressId, x.ItemsItemId });
                    table.ForeignKey(
                        name: "FK_AddressItem_Addresses_AddressesAddressId",
                        column: x => x.AddressesAddressId,
                        principalTable: "Addresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddressItem_Items_ItemsItemId",
                        column: x => x.ItemsItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressItem_ItemsItemId",
                table: "AddressItem",
                column: "ItemsItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressItem");

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
    }
}
