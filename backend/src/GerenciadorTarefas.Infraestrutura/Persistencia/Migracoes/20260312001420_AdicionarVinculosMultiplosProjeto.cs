using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class AdicionarVinculosMultiplosProjeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projetos_areas",
                columns: table => new
                {
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projetos_areas", x => new { x.projeto_id, x.area_id });
                    table.ForeignKey(
                        name: "fk_projetos_areas_vinculo_area",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_projetos_areas_vinculo_projeto",
                        column: x => x.projeto_id,
                        principalTable: "projetos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projetos_usuarios_vinculados",
                columns: table => new
                {
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projetos_usuarios_vinculados", x => new { x.projeto_id, x.usuario_id });
                    table.ForeignKey(
                        name: "fk_projetos_usuarios_vinculo_projeto",
                        column: x => x.projeto_id,
                        principalTable: "projetos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_projetos_usuarios_vinculo_usuario",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_projetos_areas_area_id",
                table: "projetos_areas",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "ix_projetos_usuarios_vinculados_usuario_id",
                table: "projetos_usuarios_vinculados",
                column: "usuario_id");

            migrationBuilder.Sql(
                """
                INSERT INTO projetos_areas (projeto_id, area_id)
                SELECT id, area_id
                FROM projetos
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO projetos_usuarios_vinculados (projeto_id, usuario_id)
                SELECT p.id, p.gestor_usuario_id
                FROM projetos p
                INNER JOIN usuarios u ON u.id = p.gestor_usuario_id
                WHERE p.gestor_usuario_id IS NOT NULL
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO projetos_usuarios_vinculados (projeto_id, usuario_id)
                SELECT p.id, p.criado_por_usuario_id
                FROM projetos p
                INNER JOIN usuarios u ON u.id = p.criado_por_usuario_id
                WHERE p.criado_por_usuario_id IS NOT NULL
                ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projetos_areas");

            migrationBuilder.DropTable(
                name: "projetos_usuarios_vinculados");
        }
    }
}
