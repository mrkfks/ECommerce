using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_Order",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_GlobalAttributeValues_GlobalAttributeId_Value",
                table: "GlobalAttributeValues");

            migrationBuilder.DropIndex(
                name: "IX_GlobalAttributes_CompanyId",
                table: "GlobalAttributes");

            migrationBuilder.DropIndex(
                name: "IX_GlobalAttributes_CompanyId_Name",
                table: "GlobalAttributes");

            migrationBuilder.DropIndex(
                name: "IX_GlobalAttributes_DisplayOrder",
                table: "GlobalAttributes");

            migrationBuilder.DropIndex(
                name: "IX_CategoryGlobalAttributes_CategoryId_GlobalAttributeId",
                table: "CategoryGlobalAttributes");

            migrationBuilder.DropIndex(
                name: "IX_BrandCategories_BrandId_CategoryId",
                table: "BrandCategories");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "GlobalAttributeValues",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "GlobalAttributes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Companies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Companies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "Companies",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "CategoryGlobalAttributes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CategoryGlobalAttributes",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BrandCategories",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "Companies");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "GlobalAttributeValues",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "GlobalAttributes",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "CategoryGlobalAttributes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CategoryGlobalAttributes",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "BrandCategories",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_IsPrimary",
                table: "ProductImages",
                columns: new[] { "ProductId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_Order",
                table: "ProductImages",
                columns: new[] { "ProductId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalAttributeValues_GlobalAttributeId_Value",
                table: "GlobalAttributeValues",
                columns: new[] { "GlobalAttributeId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalAttributes_CompanyId",
                table: "GlobalAttributes",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalAttributes_CompanyId_Name",
                table: "GlobalAttributes",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlobalAttributes_DisplayOrder",
                table: "GlobalAttributes",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryGlobalAttributes_CategoryId_GlobalAttributeId",
                table: "CategoryGlobalAttributes",
                columns: new[] { "CategoryId", "GlobalAttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BrandCategories_BrandId_CategoryId",
                table: "BrandCategories",
                columns: new[] { "BrandId", "CategoryId" },
                unique: true);
        }
    }
}
