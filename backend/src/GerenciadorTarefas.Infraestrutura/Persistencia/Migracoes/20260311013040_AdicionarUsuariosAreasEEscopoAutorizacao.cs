using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class AdicionarUsuariosAreasEEscopoAutorizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "responsavel_id",
                table: "tarefas",
                newName: "responsavel_usuario_id");

            migrationBuilder.RenameIndex(
                name: "ix_tarefas_responsavel_id",
                table: "tarefas",
                newName: "ix_tarefas_responsavel_usuario_id");

            migrationBuilder.RenameColumn(
                name: "responsavel_id",
                table: "notificacoes",
                newName: "responsavel_usuario_id");

            migrationBuilder.RenameIndex(
                name: "ix_notificacoes_responsavel_id",
                table: "notificacoes",
                newName: "ix_notificacoes_responsavel_usuario_id");

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    perfil_global = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ultimo_acesso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_areas",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_areas", x => new { x.usuario_id, x.area_id });
                    table.ForeignKey(
                        name: "fk_usuarios_areas_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_usuarios_areas_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            var areaGeralId = new Guid("a5f46885-6b01-4f72-9f4e-c11c00da8df0");

            migrationBuilder.InsertData(
                table: "areas",
                columns: new[] { "id", "nome", "codigo", "ativa" },
                values: new object[] { areaGeralId, "Geral", "GERAL", true });

            migrationBuilder.AddColumn<Guid>(
                name: "area_id",
                table: "projetos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "criado_por_usuario_id",
                table: "projetos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "gestor_usuario_id",
                table: "projetos",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                $"UPDATE projetos SET area_id = '{areaGeralId}' WHERE area_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "area_id",
                table: "projetos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO usuarios (id, nome, email, senha_hash, perfil_global, ativo, data_criacao)
                SELECT ids.id,
                       'Usuario legado',
                       'legado-' || replace(ids.id::text, '-', '') || '@gerenciadortarefas.local',
                       '',
                       3,
                       true,
                       NOW()
                FROM (
                    SELECT DISTINCT responsavel_usuario_id AS id FROM tarefas
                    UNION
                    SELECT DISTINCT responsavel_usuario_id AS id FROM notificacoes
                ) ids
                WHERE ids.id IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1 FROM usuarios u WHERE u.id = ids.id
                  );
                """);

            migrationBuilder.Sql(
                $"""
                INSERT INTO usuarios_areas (usuario_id, area_id)
                SELECT u.id, '{areaGeralId}'
                FROM usuarios u
                WHERE u.email LIKE 'legado-%@gerenciadortarefas.local'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM usuarios_areas ua
                      WHERE ua.usuario_id = u.id
                        AND ua.area_id = '{areaGeralId}'
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "ix_projetos_area_id",
                table: "projetos",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_criado_por_usuario_id",
                table: "projetos",
                column: "criado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_gestor_usuario_id",
                table: "projetos",
                column: "gestor_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_projeto_id",
                table: "notificacoes",
                column: "projeto_id");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_tarefa_id",
                table: "notificacoes",
                column: "tarefa_id");

            migrationBuilder.CreateIndex(
                name: "ux_areas_codigo",
                table: "areas",
                column: "codigo",
                unique: true,
                filter: "codigo IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_areas_nome",
                table: "areas",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_areas_area_id",
                table: "usuarios_areas",
                column: "area_id");

            migrationBuilder.AddForeignKey(
                name: "fk_notificacoes_projetos_projeto_id",
                table: "notificacoes",
                column: "projeto_id",
                principalTable: "projetos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notificacoes_tarefas_tarefa_id",
                table: "notificacoes",
                column: "tarefa_id",
                principalTable: "tarefas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_notificacoes_usuarios_responsavel_usuario_id",
                table: "notificacoes",
                column: "responsavel_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_projetos_areas_area_id",
                table: "projetos",
                column: "area_id",
                principalTable: "areas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_projetos_usuarios_criado_por",
                table: "projetos",
                column: "criado_por_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_projetos_usuarios_gestor",
                table: "projetos",
                column: "gestor_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_tarefas_usuarios_responsavel_usuario_id",
                table: "tarefas",
                column: "responsavel_usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notificacoes_projetos_projeto_id",
                table: "notificacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_notificacoes_tarefas_tarefa_id",
                table: "notificacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_notificacoes_usuarios_responsavel_usuario_id",
                table: "notificacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_projetos_areas_area_id",
                table: "projetos");

            migrationBuilder.DropForeignKey(
                name: "fk_projetos_usuarios_criado_por",
                table: "projetos");

            migrationBuilder.DropForeignKey(
                name: "fk_projetos_usuarios_gestor",
                table: "projetos");

            migrationBuilder.DropForeignKey(
                name: "fk_tarefas_usuarios_responsavel_usuario_id",
                table: "tarefas");

            migrationBuilder.DropTable(
                name: "usuarios_areas");

            migrationBuilder.DropTable(
                name: "areas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropIndex(
                name: "ix_projetos_area_id",
                table: "projetos");

            migrationBuilder.DropIndex(
                name: "IX_projetos_criado_por_usuario_id",
                table: "projetos");

            migrationBuilder.DropIndex(
                name: "IX_projetos_gestor_usuario_id",
                table: "projetos");

            migrationBuilder.DropIndex(
                name: "IX_notificacoes_projeto_id",
                table: "notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_notificacoes_tarefa_id",
                table: "notificacoes");

            migrationBuilder.DropColumn(
                name: "area_id",
                table: "projetos");

            migrationBuilder.DropColumn(
                name: "criado_por_usuario_id",
                table: "projetos");

            migrationBuilder.DropColumn(
                name: "gestor_usuario_id",
                table: "projetos");

            migrationBuilder.RenameColumn(
                name: "responsavel_usuario_id",
                table: "tarefas",
                newName: "responsavel_id");

            migrationBuilder.RenameIndex(
                name: "ix_tarefas_responsavel_usuario_id",
                table: "tarefas",
                newName: "ix_tarefas_responsavel_id");

            migrationBuilder.RenameColumn(
                name: "responsavel_usuario_id",
                table: "notificacoes",
                newName: "responsavel_id");

            migrationBuilder.RenameIndex(
                name: "ix_notificacoes_responsavel_usuario_id",
                table: "notificacoes",
                newName: "ix_notificacoes_responsavel_id");
        }
    }
}
