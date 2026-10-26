using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadEstoque.Web.Migrations.Estoque
{
    /// <inheritdoc />
    public partial class AddEntradaFiscalEImagemProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Produto",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EntradaFiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "TEXT", maxLength: 44, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CnpjFornecedor = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    FornecedorNome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfirmadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradaFiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradaFiscal_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VinculoProdutoFornecedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CnpjFornecedor = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    CodigoFornecedor = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    SkuId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VinculoProdutoFornecedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VinculoProdutoFornecedor_Sku_SkuId",
                        column: x => x.SkuId,
                        principalTable: "Sku",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemEntradaFiscal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntradaFiscalId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NumeroItem = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoFornecedor = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Gtin = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "TEXT", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SkuId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemEntradaFiscal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemEntradaFiscal_EntradaFiscal_EntradaFiscalId",
                        column: x => x.EntradaFiscalId,
                        principalTable: "EntradaFiscal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemEntradaFiscal_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemEntradaFiscal_Sku_SkuId",
                        column: x => x.SkuId,
                        principalTable: "Sku",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntradaFiscal_ChaveAcesso",
                table: "EntradaFiscal",
                column: "ChaveAcesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntradaFiscal_UsuarioId",
                table: "EntradaFiscal",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEntradaFiscal_EntradaFiscalId",
                table: "ItemEntradaFiscal",
                column: "EntradaFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEntradaFiscal_ProdutoId",
                table: "ItemEntradaFiscal",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemEntradaFiscal_SkuId",
                table: "ItemEntradaFiscal",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_VinculoProdutoFornecedor_CnpjFornecedor_CodigoFornecedor",
                table: "VinculoProdutoFornecedor",
                columns: new[] { "CnpjFornecedor", "CodigoFornecedor" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VinculoProdutoFornecedor_SkuId",
                table: "VinculoProdutoFornecedor",
                column: "SkuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemEntradaFiscal");

            migrationBuilder.DropTable(
                name: "VinculoProdutoFornecedor");

            migrationBuilder.DropTable(
                name: "EntradaFiscal");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Produto");
        }
    }
}
