using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaHierarquiaMatrizFiliaisEPerfilDono : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatrizId",
                table: "oticas",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_oticas_MatrizId",
                table: "oticas",
                column: "MatrizId");

            migrationBuilder.AddForeignKey(
                name: "FK_oticas_oticas_MatrizId",
                table: "oticas",
                column: "MatrizId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oticas_oticas_MatrizId",
                table: "oticas");

            migrationBuilder.DropIndex(
                name: "IX_oticas_MatrizId",
                table: "oticas");

            migrationBuilder.DropColumn(
                name: "MatrizId",
                table: "oticas");
        }
    }
}
