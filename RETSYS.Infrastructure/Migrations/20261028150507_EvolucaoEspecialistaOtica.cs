using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EvolucaoEspecialistaOtica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "JurosAtraso",
                table: "parcelas_pagamento",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MultaAtraso",
                table: "parcelas_pagamento",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroCarne",
                table: "parcelas_pagamento",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacaoPagamento",
                table: "parcelas_pagamento",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPago",
                table: "parcelas_pagamento",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ConferidoLensometro",
                table: "ordens_servico",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConferidoLensometroEm",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConferidoLensometroPorId",
                table: "ordens_servico",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataEnvioLaboratorio",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRetornoLaboratorio",
                table: "ordens_servico",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtapaLaboratorio",
                table: "ordens_servico",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "BALCAO");

            migrationBuilder.AddColumn<string>(
                name: "LaboratorioParceiro",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroPedidoLaboratorio",
                table: "ordens_servico",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceitaClienteId",
                table: "ordens_servico",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Aro",
                table: "armacoes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Formato",
                table: "armacoes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Retangular");

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "armacoes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Unissex");

            migrationBuilder.AddColumn<decimal>(
                name: "Haste",
                table: "armacoes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Ponte",
                table: "armacoes",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "receitas_cliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OticaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataExame = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataValidade = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MedicoNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MedicoCrm = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MedicoTipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OFTALMOLOGISTA"),
                    Clinica = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OdEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OdCilindrico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OdEixo = table.Column<int>(type: "integer", nullable: false),
                    OdDnp = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OdAltura = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OdPrisma = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OdBasePrisma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OeEsferico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OeCilindrico = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OeEixo = table.Column<int>(type: "integer", nullable: false),
                    OeDnp = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OeAltura = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OePrisma = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    OeBasePrisma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Adicao = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    Aro = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    Ponte = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    MaiorDiagonal = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    AlturaVertical = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    FotoReceitaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_receitas_cliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_receitas_cliente_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_receitas_cliente_oticas_OticaId",
                        column: x => x.OticaId,
                        principalTable: "oticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vendas_balcao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OticaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroVenda = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataVenda = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorTotalBruto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DescontoReais = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorTotalLiquido = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    FormaPagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Parcelas = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "CONCLUIDA"),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    LojaUnidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Matriz"),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendas_balcao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vendas_balcao_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendas_balcao_oticas_OticaId",
                        column: x => x.OticaId,
                        principalTable: "oticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_vendas_balcao_usuarios_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "itens_venda_balcao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendaBalcaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoItem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ArmacaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_venda_balcao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_itens_venda_balcao_armacoes_ArmacaoId",
                        column: x => x.ArmacaoId,
                        principalTable: "armacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_itens_venda_balcao_vendas_balcao_VendaBalcaoId",
                        column: x => x.VendaBalcaoId,
                        principalTable: "vendas_balcao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ConferidoLensometroPorId",
                table: "ordens_servico",
                column: "ConferidoLensometroPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ReceitaClienteId",
                table: "ordens_servico",
                column: "ReceitaClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_venda_balcao_ArmacaoId",
                table: "itens_venda_balcao",
                column: "ArmacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_itens_venda_balcao_VendaBalcaoId",
                table: "itens_venda_balcao",
                column: "VendaBalcaoId");

            migrationBuilder.CreateIndex(
                name: "IX_receitas_cliente_ClienteId",
                table: "receitas_cliente",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_receitas_cliente_OticaId",
                table: "receitas_cliente",
                column: "OticaId");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_balcao_ClienteId",
                table: "vendas_balcao",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_vendas_balcao_OticaId_NumeroVenda",
                table: "vendas_balcao",
                columns: new[] { "OticaId", "NumeroVenda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendas_balcao_VendedorId",
                table: "vendas_balcao",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_receitas_cliente_ReceitaClienteId",
                table: "ordens_servico",
                column: "ReceitaClienteId",
                principalTable: "receitas_cliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ordens_servico_usuarios_ConferidoLensometroPorId",
                table: "ordens_servico",
                column: "ConferidoLensometroPorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_receitas_cliente_ReceitaClienteId",
                table: "ordens_servico");

            migrationBuilder.DropForeignKey(
                name: "FK_ordens_servico_usuarios_ConferidoLensometroPorId",
                table: "ordens_servico");

            migrationBuilder.DropTable(
                name: "itens_venda_balcao");

            migrationBuilder.DropTable(
                name: "receitas_cliente");

            migrationBuilder.DropTable(
                name: "vendas_balcao");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_ConferidoLensometroPorId",
                table: "ordens_servico");

            migrationBuilder.DropIndex(
                name: "IX_ordens_servico_ReceitaClienteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "JurosAtraso",
                table: "parcelas_pagamento");

            migrationBuilder.DropColumn(
                name: "MultaAtraso",
                table: "parcelas_pagamento");

            migrationBuilder.DropColumn(
                name: "NumeroCarne",
                table: "parcelas_pagamento");

            migrationBuilder.DropColumn(
                name: "ObservacaoPagamento",
                table: "parcelas_pagamento");

            migrationBuilder.DropColumn(
                name: "ValorPago",
                table: "parcelas_pagamento");

            migrationBuilder.DropColumn(
                name: "ConferidoLensometro",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ConferidoLensometroEm",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ConferidoLensometroPorId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataEnvioLaboratorio",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "DataRetornoLaboratorio",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "EtapaLaboratorio",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "LaboratorioParceiro",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "NumeroPedidoLaboratorio",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "ReceitaClienteId",
                table: "ordens_servico");

            migrationBuilder.DropColumn(
                name: "Aro",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "Formato",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "Haste",
                table: "armacoes");

            migrationBuilder.DropColumn(
                name: "Ponte",
                table: "armacoes");
        }
    }
}
