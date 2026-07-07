using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverLenteTratamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos");

            migrationBuilder.DropTable(
                name: "lentes_tratamentos");

            migrationBuilder.DropIndex(
                name: "IX_lentes_tabela_precos_tratamento_id",
                table: "lentes_tabela_precos");

            migrationBuilder.DropColumn(
                name: "tratamento_id",
                table: "lentes_tabela_precos");

            migrationBuilder.AddColumn<string>(
                name: "tratamento",
                table: "lentes_tabela_precos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tratamento",
                table: "lentes_tabela_precos");

            migrationBuilder.AddColumn<Guid>(
                name: "tratamento_id",
                table: "lentes_tabela_precos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lentes_tratamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    acrescimo_valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lentes_tratamentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lentes_tabela_precos_tratamento_id",
                table: "lentes_tabela_precos",
                column: "tratamento_id");

            migrationBuilder.AddForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos",
                column: "tratamento_id",
                principalTable: "lentes_tratamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
