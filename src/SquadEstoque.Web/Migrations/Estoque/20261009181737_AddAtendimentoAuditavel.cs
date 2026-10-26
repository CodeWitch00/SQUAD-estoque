using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadEstoque.Web.Migrations.Estoque
{
    /// <inheritdoc />
    public partial class AddAtendimentoAuditavel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AtendimentoId",
                table: "Ruptura",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AtendimentoId",
                table: "Movimentacao",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Atendimento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SkuId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Resultado = table.Column<int>(type: "INTEGER", nullable: true),
                    SaldoConsultado = table.Column<int>(type: "INTEGER", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atendimento", x => x.Id);
                    table.CheckConstraint("chk_atendimento_conclusao", "(Status = 0 AND Resultado IS NULL AND ConcluidoEm IS NULL) OR (Status = 1 AND Resultado IS NOT NULL AND ConcluidoEm IS NOT NULL)");
                    table.CheckConstraint("chk_atendimento_resultado", "Resultado IS NULL OR Resultado IN (0, 1, 2)");
                    table.CheckConstraint("chk_atendimento_saldo", "SaldoConsultado IS NULL OR SaldoConsultado >= 0");
                    table.CheckConstraint("chk_atendimento_sku_resultado", "Resultado IS NULL OR Resultado = 2 OR SkuId IS NOT NULL");
                    table.CheckConstraint("chk_atendimento_status", "Status IN (0, 1)");
                    table.ForeignKey(
                        name: "FK_Atendimento_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Atendimento_Sku_SkuId",
                        column: x => x.SkuId,
                        principalTable: "Sku",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Atendimento_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ruptura_AtendimentoId",
                table: "Ruptura",
                column: "AtendimentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movimentacao_AtendimentoId",
                table: "Movimentacao",
                column: "AtendimentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Atendimento_ProdutoId",
                table: "Atendimento",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimento_SkuId",
                table: "Atendimento",
                column: "SkuId");

            migrationBuilder.CreateIndex(
                name: "IX_Atendimento_UsuarioId_Status_CriadoEm",
                table: "Atendimento",
                columns: new[] { "UsuarioId", "Status", "CriadoEm" });

            migrationBuilder.AddForeignKey(
                name: "FK_Movimentacao_Atendimento_AtendimentoId",
                table: "Movimentacao",
                column: "AtendimentoId",
                principalTable: "Atendimento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ruptura_Atendimento_AtendimentoId",
                table: "Ruptura",
                column: "AtendimentoId",
                principalTable: "Atendimento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimentacao_Atendimento_AtendimentoId",
                table: "Movimentacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Ruptura_Atendimento_AtendimentoId",
                table: "Ruptura");

            migrationBuilder.DropTable(
                name: "Atendimento");

            migrationBuilder.DropIndex(
                name: "IX_Ruptura_AtendimentoId",
                table: "Ruptura");

            migrationBuilder.DropIndex(
                name: "IX_Movimentacao_AtendimentoId",
                table: "Movimentacao");

            migrationBuilder.DropColumn(
                name: "AtendimentoId",
                table: "Ruptura");

            migrationBuilder.DropColumn(
                name: "AtendimentoId",
                table: "Movimentacao");
        }
    }
}
