using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadEstoque.Web.Migrations.Estoque
{
    /// <inheritdoc />
    public partial class AddSkuUltimaAtualizacaoSaldo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaAtualizacaoSaldoEm",
                table: "Sku",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimaAtualizacaoSaldoEm",
                table: "Sku");
        }
    }
}
