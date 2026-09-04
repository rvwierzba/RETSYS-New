using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiTenantLote1_Catalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "marcas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "lentes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "configuracoes_loja",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "configuracoes_loja",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "armacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_marcas_OticaId",
                table: "marcas",
                column: "OticaId");

            migrationBuilder.CreateIndex(
                name: "IX_lentes_OticaId",
                table: "lentes",
                column: "OticaId");

            migrationBuilder.CreateIndex(
                name: "IX_configuracoes_loja_OticaId",
                table: "configuracoes_loja",
                column: "OticaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_armacoes_OticaId",
                table: "armacoes",
                column: "OticaId");

            migrationBuilder.AddForeignKey(
                name: "FK_armacoes_oticas_OticaId",
                table: "armacoes",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_configuracoes_loja_oticas_OticaId",
                table: "configuracoes_loja",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lentes_oticas_OticaId",
                table: "lentes",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_marcas_oticas_OticaId",
                table: "marcas",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_armacoes_oticas_OticaId",
                table: "armacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_configuracoes_loja_oticas_OticaId",
                table: "configuracoes_loja");

            migrationBuilder.DropForeignKey(
                name: "FK_lentes_oticas_OticaId",
                table: "lentes");

            migrationBuilder.DropForeignKey(
                name: "FK_marcas_oticas_OticaId",
                table: "marcas");

            migrationBuilder.DropIndex(
                name: "IX_marcas_OticaId",
                table: "marcas");

            migrationBuilder.DropIndex(
                name: "IX_lentes_OticaId",
                table: "lentes");

            migrationBuilder.DropIndex(
                name: "IX_configuracoes_loja_OticaId",
                table: "configuracoes_loja");

            migrationBuilder.DropIndex(
                name: "IX_armacoes_OticaId",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "marcas");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "configuracoes_loja");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "configuracoes_loja");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "armacoes");
        }
    }
}
