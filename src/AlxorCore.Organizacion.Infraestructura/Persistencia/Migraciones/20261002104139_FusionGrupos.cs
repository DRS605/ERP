using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FusionGrupos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las políticas por grupo admiten escribir en el grupo de destino de una fusión (app.grupo_fusion).
            migrationBuilder.Sql(RlsSql.ActualizarPoliticasPorGrupo(conFusion: true));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsSql.ActualizarPoliticasPorGrupo(conFusion: false));
        }
    }
}
