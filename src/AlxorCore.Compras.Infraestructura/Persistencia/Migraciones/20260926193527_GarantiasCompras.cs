using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("compras", "linea_pedido", "pedido_id", "compras", "pedido_compra"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("compras", "linea_albaran", "albaran_id", "compras", "albaran_compra"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("compras", "linea_solicitud", "solicitud_id", "compras", "solicitud_compra"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("compras", "linea_solicitud"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("compras", "linea_albaran"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("compras", "linea_pedido"));
        }
    }
}
