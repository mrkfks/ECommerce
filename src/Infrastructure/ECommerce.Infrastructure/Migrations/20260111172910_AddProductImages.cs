using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "Banners");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Banners",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Banners",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Banners",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Banners",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Banners",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Banners",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Banners",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Banners",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "ProductImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImage_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banners_CompanyId_Order",
                table: "Banners",
                columns: new[] { "CompanyId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductId_IsPrimary",
                table: "ProductImage",
                columns: new[] { "ProductId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImage_ProductId_Order",
                table: "ProductImage",
                columns: new[] { "ProductId", "Order" });

            migrationBuilder.AddForeignKey(
                name: "FK_Banners_Companies_CompanyId",
                table: "Banners",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banners_Companies_CompanyId",
                table: "Banners");

            migrationBuilder.DropTable(
                name: "ProductImage");

            migrationBuilder.DropIndex(
                name: "IX_Banners_CompanyId_Order",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Banners");

            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "Banners",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
