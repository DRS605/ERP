using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("inventario", "movimiento_inventario",
                "stock.movimiento_inalterable", "Un movimiento de inventario no se puede modificar ni borrar: registra un ajuste."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("inventario", "movimiento_inventario", "tg_movimiento_inventario_solo_insercion"));
        }
    }
}
