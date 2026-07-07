using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteReceitaComputadaEOrdemServicoManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos");

            migrationBuilder.DropForeignKey(
                name: "FK_os_financeiro_lentes_LenteId",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "IndiceRefracao",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "PrecoCusto",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "PrecoVenda",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "QuantidadeEstoque",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "QuantidadeMinima",
                table: "lentes");

            migrationBuilder.DropColumn(
                name: "Tratamento",
                table: "lentes");

            migrationBuilder.RenameColumn(
                name: "LenteId",
                table: "os_financeiro",
                newName: "LentePrecoId");

            migrationBuilder.RenameIndex(
                name: "IX_os_financeiro_LenteId",
                table: "os_financeiro",
                newName: "IX_os_financeiro_LentePrecoId");

            migrationBuilder.AlterColumn<string>(
                name: "LenteDescricaoManual",
                table: "ordens_servico",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArmacaoModeloManual",
                table: "ordens_servico",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaCompra",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProdutoAdquirido",
                table: "clientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorGasto",
                table: "clientes",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos",
                column: "tratamento_id",
                principalTable: "lentes_tratamentos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_os_financeiro_lentes_tabela_precos_LentePrecoId",
                table: "os_financeiro",
                column: "LentePrecoId",
                principalTable: "lentes_tabela_precos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos");

            migrationBuilder.DropForeignKey(
                name: "FK_os_financeiro_lentes_tabela_precos_LentePrecoId",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "DataUltimaCompra",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "ProdutoAdquirido",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "ValorGasto",
                table: "clientes");

            migrationBuilder.RenameColumn(
                name: "LentePrecoId",
                table: "os_financeiro",
                newName: "LenteId");

            migrationBuilder.RenameIndex(
                name: "IX_os_financeiro_LentePrecoId",
                table: "os_financeiro",
                newName: "IX_os_financeiro_LenteId");

            migrationBuilder.AlterColumn<string>(
                name: "LenteDescricaoManual",
                table: "ordens_servico",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ArmacaoModeloManual",
                table: "ordens_servico",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "IndiceRefracao",
                table: "lentes",
                type: "numeric(4,2)",
                precision: 4,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoCusto",
                table: "lentes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoVenda",
                table: "lentes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeEstoque",
                table: "lentes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeMinima",
                table: "lentes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tratamento",
                table: "lentes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_lentes_tabela_precos_lentes_tratamentos_tratamento_id",
                table: "lentes_tabela_precos",
                column: "tratamento_id",
                principalTable: "lentes_tratamentos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_os_financeiro_lentes_LenteId",
                table: "os_financeiro",
                column: "LenteId",
                principalTable: "lentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
