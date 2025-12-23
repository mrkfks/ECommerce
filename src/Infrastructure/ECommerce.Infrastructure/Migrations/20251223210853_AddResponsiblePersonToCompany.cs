using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsiblePersonToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonEmail",
                table: "Companies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonName",
                table: "Companies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonPhone",
                table: "Companies",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsiblePersonEmail",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonName",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonPhone",
                table: "Companies");
        }
    }
}
