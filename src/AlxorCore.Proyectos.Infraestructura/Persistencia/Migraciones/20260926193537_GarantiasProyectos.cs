using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Proyectos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasProyectos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("proyectos", "imputacion", "proyecto_id", "proyectos", "proyecto"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("proyectos", "imputacion"));
        }
    }
}
