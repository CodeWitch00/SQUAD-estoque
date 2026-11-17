using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadEstoque.Web.Migrations.Estoque
{
    /// <inheritdoc />
    public partial class AddConsultaIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ruptura_SkuId",
                table: "Ruptura");

            migrationBuilder.CreateIndex(
                name: "IX_Sku_ProdutoId_SaldoZero",
                table: "Sku",
                column: "ProdutoId",
                filter: "SaldoAtual = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Ruptura_SkuId_CriadoEm",
                table: "Ruptura",
                columns: new[] { "SkuId", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Produto_Categoria",
                table: "Produto",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_Marca",
                table: "Produto",
                column: "Marca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sku_ProdutoId_SaldoZero",
                table: "Sku");

            migrationBuilder.DropIndex(
                name: "IX_Ruptura_SkuId_CriadoEm",
                table: "Ruptura");

            migrationBuilder.DropIndex(
                name: "IX_Produto_Categoria",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_Marca",
                table: "Produto");

            migrationBuilder.CreateIndex(
                name: "IX_Ruptura_SkuId",
                table: "Ruptura",
                column: "SkuId");
        }
    }
}
