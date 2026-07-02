using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposHistoricosOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "VendedorId",
                table: "ordens_servico",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ArmacaoModeloManual",
                table: "ordens_servico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetroativa",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LenteDescricaoManual",
                table: "ordens_servico",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArmacaoModeloManual",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "IsRetroativa",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LenteDescricaoManual",
                table: "ordens_servico");

            migrationBuilder.AlterColumn<Guid>(
                name: "VendedorId",
                table: "ordens_servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
