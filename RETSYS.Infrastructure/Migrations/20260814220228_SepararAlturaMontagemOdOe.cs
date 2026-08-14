using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SepararAlturaMontagemOdOe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AlturaMontagem",
                table: "os_receita",
                newName: "AlturaMontagemOe");

            migrationBuilder.AddColumn<decimal>(
                name: "AlturaMontagemOd",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlturaMontagemOd",
                table: "os_receita");

            migrationBuilder.RenameColumn(
                name: "AlturaMontagemOe",
                table: "os_receita",
                newName: "AlturaMontagem");
        }
    }
}
