using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{
    public partial class addShoppingCartTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ShoppingCartId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShoppingCarts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    EcommerceUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_AspNetUsers_EcommerceUserId",
                        column: x => x.EcommerceUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingCarts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 26, 11, 49, 45, 219, DateTimeKind.Unspecified).AddTicks(6549), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 26, 11, 49, 45, 219, DateTimeKind.Unspecified).AddTicks(6548), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 26, 11, 49, 45, 219, DateTimeKind.Unspecified).AddTicks(6803), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 26, 11, 49, 45, 219, DateTimeKind.Unspecified).AddTicks(6802), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ShoppingCartId",
                table: "Products",
                column: "ShoppingCartId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_EcommerceUserId",
                table: "ShoppingCarts",
                column: "EcommerceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCarts_ProductId",
                table: "ShoppingCarts",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_ShoppingCarts_ShoppingCartId",
                table: "Products",
                column: "ShoppingCartId",
                principalTable: "ShoppingCarts",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_ShoppingCarts_ShoppingCartId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "ShoppingCarts");

            migrationBuilder.DropIndex(
                name: "IX_Products_ShoppingCartId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShoppingCartId",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 25, 15, 12, 0, 509, DateTimeKind.Unspecified).AddTicks(3032), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 25, 15, 12, 0, 509, DateTimeKind.Unspecified).AddTicks(3031), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "TimeCreated", "TimeUpdated" },
                values: new object[] { new DateTimeOffset(new DateTime(2022, 12, 25, 15, 12, 0, 509, DateTimeKind.Unspecified).AddTicks(3177), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2022, 12, 25, 15, 12, 0, 509, DateTimeKind.Unspecified).AddTicks(3177), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
