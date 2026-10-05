using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcompanhamentoOsEWhatsapp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataChegadaLente",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPrevisaoLente",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LenteChegou",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppNotificadoCriacao",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppNotificadoPronto",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappMsgCadastroTemplate",
                table: "configuracoes_loja",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappMsgProntoTemplate",
                table: "configuracoes_loja",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappNumero",
                table: "configuracoes_loja",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataChegadaLente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataPrevisaoLente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LenteChegou",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "WhatsAppNotificadoCriacao",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "WhatsAppNotificadoPronto",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "WhatsappMsgCadastroTemplate",
                table: "configuracoes_loja");

            migrationBuilder.DropColumn(
                name: "WhatsappMsgProntoTemplate",
                table: "configuracoes_loja");

            migrationBuilder.DropColumn(
                name: "WhatsappNumero",
                table: "configuracoes_loja");
        }
    }
}
