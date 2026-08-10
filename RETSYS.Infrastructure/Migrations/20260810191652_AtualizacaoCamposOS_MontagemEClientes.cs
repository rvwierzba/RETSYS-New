using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizacaoCamposOS_MontagemEClientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Aro",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoOd",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoOe",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Dm",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Po",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Vert",
                table: "os_receita",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LentePrecoId",
                table: "os_financeiro",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArmacaoId",
                table: "os_financeiro",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOeEsferico",
                table: "clientes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOeCilindrico",
                table: "clientes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOdEsferico",
                table: "clientes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOdCilindrico",
                table: "clientes",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaDnpOe",
                table: "clientes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaDnpOd",
                table: "clientes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaAdicao",
                table: "clientes",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaAlturaMontagem",
                table: "clientes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aro",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "CoOd",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "CoOe",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "Dm",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "Po",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "Vert",
                table: "os_receita");

            migrationBuilder.DropColumn(
                name: "UltimaAlturaMontagem",
                table: "clientes");

            migrationBuilder.AlterColumn<Guid>(
                name: "LentePrecoId",
                table: "os_financeiro",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ArmacaoId",
                table: "os_financeiro",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOeEsferico",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOeCilindrico",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOdEsferico",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaOdCilindrico",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaDnpOe",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaDnpOd",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UltimaAdicao",
                table: "clientes",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)",
                oldPrecision: 4,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
