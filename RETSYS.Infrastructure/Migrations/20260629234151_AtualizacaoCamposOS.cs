using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizacaoCamposOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Parcelas",
                table: "os_financeiro",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorArmacao",
                table: "os_financeiro",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorLente",
                table: "os_financeiro",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MedicoTipo",
                table: "ordens_servico",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "NAO_ESPECIFICADO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorArmacao",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "ValorLente",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "MedicoTipo",
                table: "ordens_servico");

            migrationBuilder.AlterColumn<int>(
                name: "Parcelas",
                table: "os_financeiro",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
