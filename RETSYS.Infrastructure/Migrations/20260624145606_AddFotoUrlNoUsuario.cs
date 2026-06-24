using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RETSYS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoUrlNoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TelefoneFixo = table.Column<string>(type: "text", nullable: true),
                    Celular = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CPF = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    RG = table.Column<string>(type: "text", nullable: true),
                    DataNascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CEP = table.Column<string>(type: "text", nullable: false),
                    Rua = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<string>(type: "text", nullable: false),
                    Bairro = table.Column<string>(type: "text", nullable: false),
                    Cidade = table.Column<string>(type: "text", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "configuracoes_loja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomeLoja = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PixApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracoes_loja", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "marcas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marcas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    FilialLoja = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    Perfil = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "armacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MarcaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cor = table.Column<string>(type: "text", nullable: false),
                    Tamanho = table.Column<string>(type: "text", nullable: false),
                    Material = table.Column<string>(type: "text", nullable: false),
                    QuantidadeEstoque = table.Column<int>(type: "integer", nullable: false),
                    PrecoFinal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_armacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_armacoes_marcas_MarcaId",
                        column: x => x.MarcaId,
                        principalTable: "marcas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroOS = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Medico = table.Column<string>(type: "text", nullable: false),
                    DataReceita = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoLente = table.Column<string>(type: "text", nullable: false),
                    MarcaModeloLente = table.Column<string>(type: "text", nullable: false),
                    MaterialLente = table.Column<string>(type: "text", nullable: false),
                    EsfericoLongeDireito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EsfericoLongeEsquerdo = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CilindricoLongeDireito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CilindricoLongeEsquerdo = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EixoLongeDireito = table.Column<int>(type: "integer", nullable: false),
                    EixoLongeEsquerdo = table.Column<int>(type: "integer", nullable: false),
                    Adicao = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EsfericoPertoDireito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EsfericoPertoEsquerdo = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CilindricoPertoDireito = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CilindricoPertoEsquerdo = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    EixoPertoDireito = table.Column<int>(type: "integer", nullable: false),
                    EixoPertoEsquerdo = table.Column<int>(type: "integer", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataVenda = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataUltimoPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordens_servico_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordens_servico_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "parcelas_pagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroParcela = table.Column<int>(type: "integer", nullable: false),
                    DescricaoParcela = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metodo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PixQrCodePayload = table.Column<string>(type: "text", nullable: true),
                    PixTxId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcelas_pagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parcelas_pagamento_ordens_servico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "ordens_servico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_armacoes_MarcaId",
                table: "armacoes",
                column: "MarcaId");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_CPF",
                table: "clientes",
                column: "CPF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ClienteId",
                table: "ordens_servico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_UsuarioId",
                table: "ordens_servico",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_parcelas_pagamento_OrdemServicoId",
                table: "parcelas_pagamento",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "armacoes");

            migrationBuilder.DropTable(
                name: "configuracoes_loja");

            migrationBuilder.DropTable(
                name: "parcelas_pagamento");

            migrationBuilder.DropTable(
                name: "marcas");

            migrationBuilder.DropTable(
                name: "ordens_servico");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
