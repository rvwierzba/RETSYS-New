using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjustaLentePrecoEClienteHistoricoReceita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataReceita",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaAdicao",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaDnpOd",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaDnpOe",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaOdCilindrico",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UltimaOdEixo",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaOdEsferico",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaOeCilindrico",
                table: "clientes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UltimaOeEixo",
                table: "clientes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UltimaOeEsferico",
                table: "clientes",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataReceita",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaAdicao",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaDnpOd",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaDnpOe",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOdCilindrico",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOdEixo",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOdEsferico",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOeCilindrico",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOeEixo",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "UltimaOeEsferico",
                table: "clientes");
        }
    }
}
