using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LentesTabelasETratamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "surfacada",
                table: "lentes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "lentes_tratamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    acrescimo_valor = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lentes_tratamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lentes_tabela_precos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    lente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    indice_refracao = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    tratamento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    preco_custo = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    preco_venda = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lentes_tabela_precos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lentes_tabela_precos_lentes_lente_id",
                        column: x => x.lente_id,
                        principalTable: "lentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                        column: x => x.tratamento_id,
                        principalTable: "lentes_tratamentos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_lentes_tabela_precos_lente_id",
                table: "lentes_tabela_precos",
                column: "lente_id");

            migrationBuilder.CreateIndex(
                name: "IX_lentes_tabela_precos_tratamento_id",
                table: "lentes_tabela_precos",
                column: "tratamento_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lentes_tabela_precos");

            migrationBuilder.DropTable(
                name: "lentes_tratamentos");

            migrationBuilder.DropColumn(
                name: "surfacada",
                table: "lentes");
        }
    }
}
