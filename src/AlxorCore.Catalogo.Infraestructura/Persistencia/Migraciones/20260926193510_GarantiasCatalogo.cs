using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("catalogo", "atributo_variante", "producto_id", "catalogo", "producto"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("catalogo", "componente_articulo", "producto_id", "catalogo", "producto"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("catalogo", "linea_tarifa", "tarifa_id", "catalogo", "tarifa"));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("catalogo", "movimiento_stock",
                "stock.movimiento_inalterable", "Un movimiento de stock no se puede modificar ni borrar: registra un ajuste."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("catalogo", "movimiento_stock", "tg_movimiento_stock_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("catalogo", "linea_tarifa"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("catalogo", "componente_articulo"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("catalogo", "atributo_variante"));
        }
    }
}
