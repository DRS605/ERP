using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionEfectosCartera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anulacion_efecto_cartera",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    efecto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anulacion_efecto_cartera", x => x.id);
                    table.ForeignKey(
                        name: "FK_anulacion_efecto_cartera_efecto_cartera_efecto_id",
                        column: x => x.efecto_id,
                        principalSchema: "tesoreria",
                        principalTable: "efecto_cartera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_anulacion_efecto_cartera",
                schema: "tesoreria",
                table: "anulacion_efecto_cartera",
                column: "efecto_id",
                unique: true);

            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(RlsSql.Activar("tesoreria", "anulacion_efecto_cartera"));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "anulacion_efecto_cartera", "cartera.inalterable",
                "La anulación de un efecto no se modifica ni se borra."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anulacion_efecto_cartera",
                schema: "tesoreria");
        }
    }
}
