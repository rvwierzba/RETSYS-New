using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AtualizacoesPdf2708 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConferidoPorId",
                table: "os_financeiro",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataConferencia",
                table: "os_financeiro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataQuitacao",
                table: "os_financeiro",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamentoRetirada",
                table: "os_financeiro",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PagamentoConferido",
                table: "os_financeiro",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParcelasRetirada",
                table: "os_financeiro",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QuitacaoRegistradaPorId",
                table: "os_financeiro",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorRecebidoRetirada",
                table: "os_financeiro",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorRestante",
                table: "os_financeiro",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "NumeroOS",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPedidoLente",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LentePedida",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PedidoLentePorId",
                table: "ordens_servico",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_os_financeiro_ConferidoPorId",
                table: "os_financeiro",
                column: "ConferidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_os_financeiro_QuitacaoRegistradaPorId",
                table: "os_financeiro",
                column: "QuitacaoRegistradaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_PedidoLentePorId",
                table: "ordens_servico",
                column: "PedidoLentePorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_usuarios_PedidoLentePorId",
                table: "ordens_servico",
                column: "PedidoLentePorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_os_financeiro_usuarios_ConferidoPorId",
                table: "os_financeiro",
                column: "ConferidoPorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_os_financeiro_usuarios_QuitacaoRegistradaPorId",
                table: "os_financeiro",
                column: "QuitacaoRegistradaPorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_usuarios_PedidoLentePorId",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "FK_os_financeiro_usuarios_ConferidoPorId",
                table: "os_financeiro");

            migrationBuilder.DropForeignKey(
                name: "FK_os_financeiro_usuarios_QuitacaoRegistradaPorId",
                table: "os_financeiro");

            migrationBuilder.DropIndex(
                name: "IX_os_financeiro_ConferidoPorId",
                table: "os_financeiro");

            migrationBuilder.DropIndex(
                name: "IX_os_financeiro_QuitacaoRegistradaPorId",
                table: "os_financeiro");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_PedidoLentePorId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ConferidoPorId",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "DataConferencia",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "DataQuitacao",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "FormaPagamentoRetirada",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "PagamentoConferido",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "ParcelasRetirada",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "QuitacaoRegistradaPorId",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "ValorRecebidoRetirada",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "ValorRestante",
                table: "os_financeiro");

            migrationBuilder.DropColumn(
                name: "DataPedidoLente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LentePedida",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "PedidoLentePorId",
                table: "ordens_servico");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroOS",
                table: "ordens_servico",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
