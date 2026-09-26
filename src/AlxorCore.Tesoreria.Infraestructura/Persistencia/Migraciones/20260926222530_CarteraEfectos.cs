using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CarteraEfectos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "efecto_cartera",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_documento = table.Column<DateOnly>(type: "date", nullable: true),
                    vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    origen = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    origen_referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_efecto_cartera", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_efecto_cartera_vencimiento",
                schema: "tesoreria",
                table: "efecto_cartera",
                columns: new[] { "empresa_id", "sentido", "vencimiento" });

            migrationBuilder.CreateIndex(
                name: "ux_efecto_cartera_origen",
                schema: "tesoreria",
                table: "efecto_cartera",
                columns: new[] { "empresa_id", "origen", "origen_referencia" },
                unique: true);

            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(RlsSql.Activar("tesoreria", "efecto_cartera"));
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.efecto_cartera
                    ADD CONSTRAINT ck_efecto_cartera_sentido CHECK (sentido IN ('Cobro', 'Pago')),
                    ADD CONSTRAINT ck_efecto_cartera_importe CHECK (importe > 0),
                    ADD CONSTRAINT ck_efecto_cartera_origen CHECK ((origen IS NULL) = (origen_referencia IS NULL));
                """);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "efecto_cartera", "cartera.inalterable",
                "Un efecto de cartera no se modifica ni se borra: se cobra, se paga o se compensa con un movimiento."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "efecto_cartera",
                schema: "tesoreria");
        }
    }
}
