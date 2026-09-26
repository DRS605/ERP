using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasTesoreria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("tesoreria", "aplicacion_anticipo", "anticipo_id", "tesoreria", "anticipo"));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "movimiento",
                "tesoreria.movimiento_inalterable", "Un cobro o pago registrado no se puede modificar ni borrar: registra el movimiento contrario."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "aplicacion_anticipo",
                "anticipo.aplicacion_inalterable", "La aplicación de un anticipo no se puede modificar ni borrar."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "reclamacion",
                "reclamacion.inalterable", "Una reclamación registrada es histórica: no se puede modificar ni borrar."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "reclamacion", "tg_reclamacion_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "aplicacion_anticipo", "tg_aplicacion_anticipo_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "movimiento", "tg_movimiento_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("tesoreria", "aplicacion_anticipo"));
        }
    }
}
