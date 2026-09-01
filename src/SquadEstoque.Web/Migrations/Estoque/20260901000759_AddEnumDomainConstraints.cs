using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SquadEstoque.Web.Migrations.Estoque
{
    /// <inheritdoc />
    public partial class AddEnumDomainConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "chk_usuario_perfil",
                table: "Usuario",
                sql: "Perfil IN (0, 1)");

            migrationBuilder.AddCheckConstraint(
                name: "chk_movimentacao_tipo",
                table: "Movimentacao",
                sql: "Tipo IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_usuario_perfil",
                table: "Usuario");

            migrationBuilder.DropCheckConstraint(
                name: "chk_movimentacao_tipo",
                table: "Movimentacao");
        }
    }
}
