using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RectificativasRecibidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_rectificativa",
                schema: "gastos",
                table: "gasto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_rectificada",
                schema: "gastos",
                table: "gasto",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_rectificacion",
                schema: "gastos",
                table: "gasto",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_rectificado",
                schema: "gastos",
                table: "gasto",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "rectifica_gasto_id",
                schema: "gastos",
                table: "gasto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_gasto_rectifica",
                schema: "gastos",
                table: "gasto",
                column: "rectifica_gasto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_gasto_rectifica",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "es_rectificativa",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "fecha_rectificada",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "motivo_rectificacion",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "numero_rectificado",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "rectifica_gasto_id",
                schema: "gastos",
                table: "gasto");
        }
    }
}
