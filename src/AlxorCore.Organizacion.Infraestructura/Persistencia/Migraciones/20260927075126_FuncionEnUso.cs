using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FuncionEnUso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Comprobación de uso de los maestros en todas las empresas del grupo (ver ComprobadorUso en la API).
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionEnUso);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.alxor_en_uso(text[], uuid);");
        }
    }
}
