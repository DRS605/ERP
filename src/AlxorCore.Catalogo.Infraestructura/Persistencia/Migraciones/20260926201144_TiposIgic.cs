using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TiposIgic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "impuesto",
                schema: "catalogo",
                table: "tipo_iva",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Iva");

            migrationBuilder.Sql("""
                ALTER TABLE catalogo.tipo_iva
                    ADD CONSTRAINT ck_tipo_iva_impuesto CHECK (impuesto IN ('Iva', 'Igic')),
                    ADD CONSTRAINT ck_tipo_iva_igic_sin_recargo CHECK (impuesto <> 'Igic' OR recargo_equivalencia = 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "impuesto",
                schema: "catalogo",
                table: "tipo_iva");
        }
    }
}
