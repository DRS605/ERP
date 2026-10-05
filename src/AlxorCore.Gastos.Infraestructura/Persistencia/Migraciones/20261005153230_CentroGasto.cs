using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CentroGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "centro_id",
                schema: "gastos",
                table: "gasto",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "centro_id",
                schema: "gastos",
                table: "gasto");
        }
    }
}
