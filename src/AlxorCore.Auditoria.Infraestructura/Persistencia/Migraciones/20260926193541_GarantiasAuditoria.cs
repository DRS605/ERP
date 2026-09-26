using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Auditoria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("auditoria", "registro_auditoria",
                "auditoria.inalterable", "El registro de auditoría no se puede modificar ni borrar."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("auditoria", "registro_auditoria", "tg_registro_auditoria_solo_insercion"));
        }
    }
}
