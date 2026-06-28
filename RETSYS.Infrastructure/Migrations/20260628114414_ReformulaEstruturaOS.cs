using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReformulaEstruturaOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_armacoes_ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_lentes_LenteId",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_LenteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Adicao",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "AlturaMontagem",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "CilindricoLongeDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "CilindricoLongeEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "CilindricoPertoDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "CilindricoPertoEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataUltimoPagamento",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DescontoPercentual",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DescontoReais",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DnpOd",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DnpOe",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EixoLongeDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EixoLongeEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EixoPertoDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EixoPertoEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EsfericoLongeDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EsfericoLongeEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EsfericoPertoDireito",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EsfericoPertoEsquerdo",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LenteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Medico",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "TipoLente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ValorEntrada",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ValorTotal",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ValorTotalBruto",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "RG",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Rua",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "TelefoneFixo",
                table: "clientes");

            migrationBuilder.RenameColumn(
                name: "DataVenda",
                table: "ordens_servico",
                newName: "DataEntrada");

            migrationBuilder.RenameColumn(
                name: "DataReceita",
                table: "ordens_servico",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CEP",
                table: "clientes",
                newName: "Cep");

            migrationBuilder.RenameColumn(
                name: "DataCadastro",
                table: "clientes",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "Celular",
                table: "clientes",
                newName: "Telefone");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroOS",
                table: "ordens_servico",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "MedicoCrm",
                table: "ordens_servico",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevistaEntrega",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicoNome",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "clientes",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "clientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "clientes",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "clientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cidade",
                table: "clientes",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cep",
                table: "clientes",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "clientes",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "clientes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Convenio",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "clientes",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "os_financeiro",
                columns: table => new
                {
                    OsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArmacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    LenteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorTotalBruto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DescontoReais = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DescontoPercentual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ValorTotalLiquido = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Parcelas = table.Column<int>(type: "integer", nullable: false),
                    ValorEntrada = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_financeiro", x => x.OsId);
                    table.ForeignKey(
                        name: "FK_os_financeiro_armacoes_ArmacaoId",
                        column: x => x.ArmacaoId,
                        principalTable: "armacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_os_financeiro_lentes_LenteId",
                        column: x => x.LenteId,
                        principalTable: "lentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_os_financeiro_ordens_servico_OsId",
                        column: x => x.OsId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "os_receita",
                columns: table => new
                {
                    OsId = table.Column<Guid>(type: "uuid", nullable: false),
                    OdEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OdCilindrico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OdEixo = table.Column<int>(type: "integer", nullable: false),
                    OeEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OeCilindrico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OeEixo = table.Column<int>(type: "integer", nullable: false),
                    Adicao = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    DnpOd = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    DnpOe = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    AlturaMontagem = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    ObsReceita = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_receita", x => x.OsId);
                    table.ForeignKey(
                        name: "FK_os_receita_ordens_servico_OsId",
                        column: x => x.OsId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_os_financeiro_ArmacaoId",
                table: "os_financeiro",
                column: "ArmacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_os_financeiro_LenteId",
                table: "os_financeiro",
                column: "LenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "os_financeiro");

            migrationBuilder.DropTable(
                name: "os_receita");

            migrationBuilder.DropColumn(
                name: "MedicoNome",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Convenio",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "Logradouro",
                table: "clientes");

            migrationBuilder.RenameColumn(
                name: "DataEntrada",
                table: "ordens_servico",
                newName: "DataVenda");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ordens_servico",
                newName: "DataReceita");

            migrationBuilder.RenameColumn(
                name: "Cep",
                table: "clientes",
                newName: "CEP");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "clientes",
                newName: "DataCadastro");

            migrationBuilder.RenameColumn(
                name: "Telefone",
                table: "clientes",
                newName: "Celular");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroOS",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "MedicoCrm",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataPrevistaEntrega",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<decimal>(
                name: "Adicao",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AlturaMontagem",
                table: "ordens_servico",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ArmacaoId",
                table: "ordens_servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "CilindricoLongeDireito",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CilindricoLongeEsquerdo",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CilindricoPertoDireito",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CilindricoPertoEsquerdo",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimoPagamento",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DescontoPercentual",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DescontoReais",
                table: "ordens_servico",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DnpOd",
                table: "ordens_servico",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DnpOe",
                table: "ordens_servico",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "EixoLongeDireito",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EixoLongeEsquerdo",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EixoPertoDireito",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EixoPertoEsquerdo",
                table: "ordens_servico",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "EsfericoLongeDireito",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EsfericoLongeEsquerdo",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EsfericoPertoDireito",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EsfericoPertoEsquerdo",
                table: "ordens_servico",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LenteId",
                table: "ordens_servico",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Medico",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoLente",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorEntrada",
                table: "ordens_servico",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotal",
                table: "ordens_servico",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorTotalBruto",
                table: "ordens_servico",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "clientes",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "clientes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cidade",
                table: "clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "Bairro",
                table: "clientes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.AddColumn<string>(
                name: "RG",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rua",
                table: "clientes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelefoneFixo",
                table: "clientes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ArmacaoId",
                table: "ordens_servico",
                column: "ArmacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_LenteId",
                table: "ordens_servico",
                column: "LenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_armacoes_ArmacaoId",
                table: "ordens_servico",
                column: "ArmacaoId",
                principalTable: "armacoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_lentes_LenteId",
                table: "ordens_servico",
                column: "LenteId",
                principalTable: "lentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
