using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RendimientosYCosteConfeccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cajas",
                schema: "agro",
                table: "salida_parte",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "coste_confeccion",
                schema: "agro",
                table: "salida_parte",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "agro",
                table: "salida_parte",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "segundos_teoricos",
                schema: "agro",
                table: "salida_parte",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "confeccion",
                schema: "agro",
                table: "mano_obra_parte",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "rendimiento_confeccion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cajas_hora = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rendimiento_confeccion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_rendimiento_confeccion",
                schema: "agro",
                table: "rendimiento_confeccion",
                columns: new[] { "empresa_id", "producto_id", "envase_producto_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "rendimiento_confeccion"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.rendimiento_confeccion ADD CONSTRAINT ck_rendimiento_confeccion_cajas CHECK (cajas_hora > 0);
                ALTER TABLE agro.salida_parte ADD CONSTRAINT ck_salida_parte_cajas CHECK (cajas IS NULL OR cajas >= 0);
                ALTER TABLE agro.parte_confeccion DROP CONSTRAINT ck_parte_confeccion_estado;
                ALTER TABLE agro.parte_confeccion ADD CONSTRAINT ck_parte_confeccion_estado
                    CHECK (estado IN ('Borrador', 'Validado', 'Anulado') AND reparto IN ('PorKilos', 'PorFactor', 'PorTiempoTeorico'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agro.parte_confeccion DROP CONSTRAINT ck_parte_confeccion_estado;
                ALTER TABLE agro.parte_confeccion ADD CONSTRAINT ck_parte_confeccion_estado
                    CHECK (estado IN ('Borrador', 'Validado', 'Anulado') AND reparto IN ('PorKilos', 'PorFactor'));
                ALTER TABLE agro.salida_parte DROP CONSTRAINT IF EXISTS ck_salida_parte_cajas;
                """);

            migrationBuilder.DropTable(
                name: "rendimiento_confeccion",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "cajas",
                schema: "agro",
                table: "salida_parte");

            migrationBuilder.DropColumn(
                name: "coste_confeccion",
                schema: "agro",
                table: "salida_parte");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "agro",
                table: "salida_parte");

            migrationBuilder.DropColumn(
                name: "segundos_teoricos",
                schema: "agro",
                table: "salida_parte");

            migrationBuilder.DropColumn(
                name: "confeccion",
                schema: "agro",
                table: "mano_obra_parte");
        }
    }
}
