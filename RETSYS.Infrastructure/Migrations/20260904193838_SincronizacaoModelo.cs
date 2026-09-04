using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacaoModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_clientes_CPF",
                table: "clientes");

            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "ordens_servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "clientes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_OticaId_NumeroOS",
                table: "ordens_servico",
                columns: new[] { "OticaId", "NumeroOS" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientes_OticaId_CPF",
                table: "clientes",
                columns: new[] { "OticaId", "CPF" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_clientes_oticas_OticaId",
                table: "clientes",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_oticas_OticaId",
                table: "ordens_servico",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clientes_oticas_OticaId",
                table: "clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_oticas_OticaId",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_OticaId_NumeroOS",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_clientes_OticaId_CPF",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "clientes");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_CPF",
                table: "clientes",
                column: "CPF",
                unique: true);
        }
    }
}
