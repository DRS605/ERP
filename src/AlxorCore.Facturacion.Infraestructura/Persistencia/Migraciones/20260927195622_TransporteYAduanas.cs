using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TransporteYAduanas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_arancelario",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "embalaje",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "marcas",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_neto_kg",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "numeric(14,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "volumen_m3",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "numeric(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "awb",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "buque",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "compania_aerea",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conductor",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conductor2",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contenedor",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "documentos_anexos",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "incoterm",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "instrucciones",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lugar_incoterm",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "matricula_remolque",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modo",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Carretera");

            migrationBuilder.AddColumn<string>(
                name: "naviera",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pais_destino",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pais_origen",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "portes",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "precinto",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "puerto_carga",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "puerto_destino",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reserva",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "temperatura_consigna",
                schema: "facturacion",
                table: "carta_porte",
                type: "numeric(5,1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "termografo",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Nacional");

            migrationBuilder.AddColumn<Guid>(
                name: "transportista_id",
                schema: "facturacion",
                table: "carta_porte",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "vehiculo_id",
                schema: "facturacion",
                table: "carta_porte",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vuelo",
                schema: "facturacion",
                table: "carta_porte",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "despacho_aduanero",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mrn = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    fecha_despacho = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_salida = table.Column<DateOnly>(type: "date", nullable: true),
                    aduana = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_despacho_aduanero", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transportista",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    pais = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transportista", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehiculo",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    matricula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    matricula_remolque = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    descripcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tara_kg = table.Column<decimal>(type: "numeric(10,1)", nullable: true),
                    frigorifico = table.Column<bool>(type: "boolean", nullable: false),
                    transportista_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehiculo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_carta_porte_albaran",
                schema: "facturacion",
                table: "carta_porte",
                column: "albaran_id");

            migrationBuilder.CreateIndex(
                name: "ix_carta_porte_transportista",
                schema: "facturacion",
                table: "carta_porte",
                column: "transportista_id");

            migrationBuilder.CreateIndex(
                name: "ix_carta_porte_vehiculo",
                schema: "facturacion",
                table: "carta_porte",
                column: "vehiculo_id");

            migrationBuilder.CreateIndex(
                name: "ix_despacho_factura",
                schema: "facturacion",
                table: "despacho_aduanero",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "ux_despacho_empresa_mrn",
                schema: "facturacion",
                table: "despacho_aduanero",
                columns: new[] { "empresa_id", "mrn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportista_empresa_nombre",
                schema: "facturacion",
                table: "transportista",
                columns: new[] { "empresa_id", "nombre" });

            migrationBuilder.CreateIndex(
                name: "ix_vehiculo_transportista",
                schema: "facturacion",
                table: "vehiculo",
                column: "transportista_id");

            migrationBuilder.CreateIndex(
                name: "ux_vehiculo_empresa_matricula",
                schema: "facturacion",
                table: "vehiculo",
                columns: new[] { "empresa_id", "matricula" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "transportista"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "vehiculo"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "despacho_aduanero"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.carta_porte
                    ADD CONSTRAINT fk_carta_porte_transportista FOREIGN KEY (transportista_id) REFERENCES facturacion.transportista (id),
                    ADD CONSTRAINT fk_carta_porte_vehiculo FOREIGN KEY (vehiculo_id) REFERENCES facturacion.vehiculo (id),
                    ADD CONSTRAINT ck_carta_porte_temperatura CHECK (temperatura_consigna IS NULL OR temperatura_consigna BETWEEN -60 AND 60),
                    ADD CONSTRAINT ck_carta_porte_paises CHECK ((pais_origen IS NULL OR pais_origen ~ '^[A-Z]{2}$') AND (pais_destino IS NULL OR pais_destino ~ '^[A-Z]{2}$'));
                ALTER TABLE facturacion.linea_carta_porte
                    ADD CONSTRAINT ck_linea_carta_porte_arancel CHECK (codigo_arancelario IS NULL OR codigo_arancelario ~ '^([0-9]{8}|[0-9]{10})$'),
                    ADD CONSTRAINT ck_linea_carta_porte_pesos CHECK ((peso_neto_kg IS NULL OR peso_neto_kg >= 0) AND (volumen_m3 IS NULL OR volumen_m3 >= 0));
                ALTER TABLE facturacion.vehiculo
                    ADD CONSTRAINT fk_vehiculo_transportista FOREIGN KEY (transportista_id) REFERENCES facturacion.transportista (id);
                ALTER TABLE facturacion.despacho_aduanero
                    ADD CONSTRAINT fk_despacho_factura FOREIGN KEY (factura_id) REFERENCES facturacion.factura (id),
                    ADD CONSTRAINT ck_despacho_mrn CHECK (mrn ~ '^[0-9]{2}[A-Z]{2}[A-Z0-9]{14}$'),
                    ADD CONSTRAINT ck_despacho_fechas CHECK (fecha_salida IS NULL OR fecha_salida >= fecha_despacho);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.carta_porte
                    DROP CONSTRAINT IF EXISTS fk_carta_porte_transportista, DROP CONSTRAINT IF EXISTS fk_carta_porte_vehiculo,
                    DROP CONSTRAINT IF EXISTS ck_carta_porte_temperatura, DROP CONSTRAINT IF EXISTS ck_carta_porte_paises;
                ALTER TABLE facturacion.linea_carta_porte DROP CONSTRAINT IF EXISTS ck_linea_carta_porte_arancel, DROP CONSTRAINT IF EXISTS ck_linea_carta_porte_pesos;
                """);

            migrationBuilder.DropTable(
                name: "despacho_aduanero",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "transportista",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "vehiculo",
                schema: "facturacion");

            migrationBuilder.DropIndex(
                name: "ix_carta_porte_albaran",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropIndex(
                name: "ix_carta_porte_transportista",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropIndex(
                name: "ix_carta_porte_vehiculo",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "codigo_arancelario",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "embalaje",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "marcas",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "peso_neto_kg",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "volumen_m3",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "awb",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "buque",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "compania_aerea",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "conductor",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "conductor2",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "contenedor",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "documentos_anexos",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "incoterm",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "instrucciones",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "lugar_incoterm",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "matricula_remolque",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "modo",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "naviera",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "pais_destino",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "pais_origen",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "portes",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "precinto",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "puerto_carga",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "puerto_destino",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "reserva",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "temperatura_consigna",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "termografo",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "transportista_id",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "vehiculo_id",
                schema: "facturacion",
                table: "carta_porte");

            migrationBuilder.DropColumn(
                name: "vuelo",
                schema: "facturacion",
                table: "carta_porte");
        }
    }
}
