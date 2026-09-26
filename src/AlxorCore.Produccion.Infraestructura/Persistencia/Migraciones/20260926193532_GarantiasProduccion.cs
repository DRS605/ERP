using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Produccion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("produccion", "componente_plan", "orden_id", "produccion", "orden_fabricacion"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("produccion", "componente_plan"));
        }
    }
}
