using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Tarifas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tarifa",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_tarifa",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    familia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cantidad_minima = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(15,6)", nullable: true),
                    porcentaje_descuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: true),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    tarifa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_tarifa", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_tarifa_tarifa_tarifa_id",
                        column: x => x.tarifa_id,
                        principalSchema: "catalogo",
                        principalTable: "tarifa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_linea_tarifa_tarifa",
                schema: "catalogo",
                table: "linea_tarifa",
                column: "tarifa_id");

            migrationBuilder.CreateIndex(
                name: "ux_tarifa_grupo_codigo",
                schema: "catalogo",
                table: "tarifa",
                columns: new[] { "grupo_id", "codigo" },
                unique: true);

            // Aislamiento por grupo (RLS) y reglas de las líneas también en la base de datos.
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("catalogo", "tarifa"));
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.linea_tarifa
                    ADD CONSTRAINT ck_linea_tarifa_ambito CHECK (producto_id IS NULL OR familia_id IS NULL),
                    ADD CONSTRAINT ck_linea_tarifa_cantidad CHECK (cantidad_minima >= 0),
                    ADD CONSTRAINT ck_linea_tarifa_precio CHECK (precio IS NULL OR precio >= 0),
                    ADD CONSTRAINT ck_linea_tarifa_descuento CHECK (porcentaje_descuento BETWEEN 0 AND 100),
                    ADD CONSTRAINT ck_linea_tarifa_efecto CHECK (precio IS NOT NULL OR porcentaje_descuento > 0),
                    ADD CONSTRAINT ck_linea_tarifa_fechas CHECK (desde IS NULL OR hasta IS NULL OR hasta >= desde);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "linea_tarifa",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "tarifa",
                schema: "catalogo");
        }
    }
}
