using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaMultiTenantOtica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OticaId",
                table: "usuarios",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "oticas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oticas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_OticaId",
                table: "usuarios",
                column: "OticaId");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_oticas_OticaId",
                table: "usuarios",
                column: "OticaId",
                principalTable: "oticas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_oticas_OticaId",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "oticas");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_OticaId",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "OticaId",
                table: "usuarios");
        }
    }
}
