using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEstoqueEComissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_usuarios_UsuarioId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "MarcaModeloLente",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "MaterialLente",
                table: "ordens_servico");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "ordens_servico",
                newName: "VendedorId");

            migrationBuilder.RenameIndex(
                name: "IX_ordens_servico_UsuarioId",
                table: "ordens_servico",
                newName: "IX_ordens_servico_VendedorId");

            migrationBuilder.RenameColumn(
                name: "PrecoFinal",
                table: "armacoes",
                newName: "PrecoVenda");

            migrationBuilder.RenameColumn(
                name: "Modelo",
                table: "armacoes",
                newName: "ModeloReferencia");

            migrationBuilder.RenameColumn(
                name: "Codigo",
                table: "armacoes",
                newName: "CodigoSku");

            migrationBuilder.AddColumn<bool>(
                name: "ComissaoAtiva",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteDesconto",
                table: "usuarios",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MetaMensal",
                table: "usuarios",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "TipoLente",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Medico",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEntregaReal",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPrevistaEntrega",
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
                name: "MedicoCrm",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
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
                name: "ValorTotalBruto",
                table: "ordens_servico",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Tamanho",
                table: "armacoes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                table: "armacoes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cor",
                table: "armacoes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "armacoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Fornecedor",
                table: "armacoes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoCusto",
                table: "armacoes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeMinima",
                table: "armacoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "comissoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentualAplicado = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ValorComissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodoReferencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comissoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comissoes_ordens_servico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comissoes_usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "configuracao_comissao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PercentualComissao = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    BaseCalculo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MomentoGeracao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracao_comissao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_configuracao_comissao_usuarios_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fechamentos_comissao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodoReferencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    TotalVendasBrutas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalComissao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QtdOs = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechadoPorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechamentos_comissao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fechamentos_comissao_usuarios_FechadoPorId",
                        column: x => x.FechadoPorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fechamentos_comissao_usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lentes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoSku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Laboratorio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tratamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IndiceRefracao = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    GraduacaoMin = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    GraduacaoMax = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PrecoCusto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoVenda = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMinima = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lentes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ArmacaoId",
                table: "ordens_servico",
                column: "ArmacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_LenteId",
                table: "ordens_servico",
                column: "LenteId");

            migrationBuilder.CreateIndex(
                name: "IX_comissoes_OrdemServicoId",
                table: "comissoes",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_comissoes_VendedorId",
                table: "comissoes",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_configuracao_comissao_UpdatedById",
                table: "configuracao_comissao",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_fechamentos_comissao_FechadoPorId",
                table: "fechamentos_comissao",
                column: "FechadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_fechamentos_comissao_VendedorId",
                table: "fechamentos_comissao",
                column: "VendedorId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_usuarios_VendedorId",
                table: "ordens_servico",
                column: "VendedorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_armacoes_ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_lentes_LenteId",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_usuarios_VendedorId",
                table: "ordens_servico");

            migrationBuilder.DropTable(
                name: "comissoes");

            migrationBuilder.DropTable(
                name: "configuracao_comissao");

            migrationBuilder.DropTable(
                name: "fechamentos_comissao");

            migrationBuilder.DropTable(
                name: "lentes");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_LenteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ComissaoAtiva",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "LimiteDesconto",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "MetaMensal",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "AlturaMontagem",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ArmacaoId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataEntregaReal",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataPrevistaEntrega",
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
                name: "FormaPagamento",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LenteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "MedicoCrm",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ValorEntrada",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ValorTotalBruto",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "Fornecedor",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "PrecoCusto",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "QuantidadeMinima",
                table: "armacoes");

            migrationBuilder.RenameColumn(
                name: "VendedorId",
                table: "ordens_servico",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_ordens_servico_VendedorId",
                table: "ordens_servico",
                newName: "IX_ordens_servico_UsuarioId");

            migrationBuilder.RenameColumn(
                name: "PrecoVenda",
                table: "armacoes",
                newName: "PrecoFinal");

            migrationBuilder.RenameColumn(
                name: "ModeloReferencia",
                table: "armacoes",
                newName: "Modelo");

            migrationBuilder.RenameColumn(
                name: "CodigoSku",
                table: "armacoes",
                newName: "Codigo");

            migrationBuilder.AlterColumn<string>(
                name: "TipoLente",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Medico",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "MarcaModeloLente",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaterialLente",
                table: "ordens_servico",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Tamanho",
                table: "armacoes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Material",
                table: "armacoes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Cor",
                table: "armacoes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_usuarios_UsuarioId",
                table: "ordens_servico",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
