using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GerenciadorTarefas.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class RemoverFksNotificacaoTarefaProjeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_notificacoes_projetos_projeto_id",
                table: "notificacoes");

            migrationBuilder.DropForeignKey(
                name: "fk_notificacoes_tarefas_tarefa_id",
                table: "notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_notificacoes_projeto_id",
                table: "notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_notificacoes_tarefa_id",
                table: "notificacoes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_projeto_id",
                table: "notificacoes",
                column: "projeto_id");

            migrationBuilder.CreateIndex(
                name: "IX_notificacoes_tarefa_id",
                table: "notificacoes",
                column: "tarefa_id");

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
        }
    }
}
