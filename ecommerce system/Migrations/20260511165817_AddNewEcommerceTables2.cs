using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerce_system.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEcommerceTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_proudects_cart_CartId",
                table: "proudects");

            migrationBuilder.DropForeignKey(
                name: "FK_proudects_wishList_WishListId",
                table: "proudects");

            migrationBuilder.DropIndex(
                name: "IX_proudects_CartId",
                table: "proudects");

            migrationBuilder.DropIndex(
                name: "IX_proudects_WishListId",
                table: "proudects");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "proudects");

            migrationBuilder.DropColumn(
                name: "WishListId",
                table: "proudects");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LockoutEnd",
                table: "IdentityUser",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "cartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    ProudectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cartItems_cart_CartId",
                        column: x => x.CartId,
                        principalTable: "cart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cartItems_proudects_ProudectId",
                        column: x => x.ProudectId,
                        principalTable: "proudects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ProudectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_discounts_proudects_ProudectId",
                        column: x => x.ProudectId,
                        principalTable: "proudects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProudectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orderItems_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_orderItems_proudects_ProudectId",
                        column: x => x.ProudectId,
                        principalTable: "proudects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProudectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_productImages_proudects_ProudectId",
                        column: x => x.ProudectId,
                        principalTable: "proudects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WishListId = table.Column<int>(type: "int", nullable: false),
                    ProudectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wishListItems_proudects_ProudectId",
                        column: x => x.ProudectId,
                        principalTable: "proudects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wishListItems_wishList_WishListId",
                        column: x => x.WishListId,
                        principalTable: "wishList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cartItems_CartId",
                table: "cartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_cartItems_ProudectId",
                table: "cartItems",
                column: "ProudectId");

            migrationBuilder.CreateIndex(
                name: "IX_discounts_ProudectId",
                table: "discounts",
                column: "ProudectId");

            migrationBuilder.CreateIndex(
                name: "IX_orderItems_OrderId",
                table: "orderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_orderItems_ProudectId",
                table: "orderItems",
                column: "ProudectId");

            migrationBuilder.CreateIndex(
                name: "IX_productImages_ProudectId",
                table: "productImages",
                column: "ProudectId");

            migrationBuilder.CreateIndex(
                name: "IX_wishListItems_ProudectId",
                table: "wishListItems",
                column: "ProudectId");

            migrationBuilder.CreateIndex(
                name: "IX_wishListItems_WishListId",
                table: "wishListItems",
                column: "WishListId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cartItems");

            migrationBuilder.DropTable(
                name: "discounts");

            migrationBuilder.DropTable(
                name: "orderItems");

            migrationBuilder.DropTable(
                name: "productImages");

            migrationBuilder.DropTable(
                name: "wishListItems");

            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "proudects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WishListId",
                table: "proudects",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "IdentityUser",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_proudects_CartId",
                table: "proudects",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_proudects_WishListId",
                table: "proudects",
                column: "WishListId");

            migrationBuilder.AddForeignKey(
                name: "FK_proudects_cart_CartId",
                table: "proudects",
                column: "CartId",
                principalTable: "cart",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_proudects_wishList_WishListId",
                table: "proudects",
                column: "WishListId",
                principalTable: "wishList",
                principalColumn: "Id");
        }
    }
}
