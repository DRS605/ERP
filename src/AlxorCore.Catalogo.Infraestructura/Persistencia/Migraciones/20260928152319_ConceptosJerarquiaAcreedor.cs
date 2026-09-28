using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosJerarquiaAcreedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "acreedor_id",
                schema: "catalogo",
                table: "concepto_linea",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "base_porcentaje",
                schema: "catalogo",
                table: "concepto_linea",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Linea");

            migrationBuilder.AddColumn<string>(
                name: "cuenta_contable",
                schema: "catalogo",
                table: "concepto_linea",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "catalogo",
                table: "concepto_linea",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "acreedor_id",
                schema: "catalogo",
                table: "asignacion_concepto",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "desde",
                schema: "catalogo",
                table: "asignacion_concepto",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "hasta",
                schema: "catalogo",
                table: "asignacion_concepto",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_tercero",
                schema: "catalogo",
                table: "asignacion_concepto",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea
                    ADD CONSTRAINT ck_concepto_linea_base_porcentaje CHECK (base_porcentaje IN ('Linea', 'Cascada')),
                    ADD CONSTRAINT ck_concepto_linea_cuenta CHECK (cuenta_contable IS NULL OR cuenta_contable ~ '^[0-9]{1,20}$');
                ALTER TABLE catalogo.asignacion_concepto
                    ADD CONSTRAINT ck_asignacion_concepto_vigencia CHECK (desde IS NULL OR hasta IS NULL OR hasta >= desde),
                    ADD CONSTRAINT ck_asignacion_concepto_tercero CHECK (tercero_id IS NULL OR tipo_tercero IS NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acreedor_id",
                schema: "catalogo",
                table: "concepto_linea");

            migrationBuilder.DropColumn(
                name: "base_porcentaje",
                schema: "catalogo",
                table: "concepto_linea");

            migrationBuilder.DropColumn(
                name: "cuenta_contable",
                schema: "catalogo",
                table: "concepto_linea");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "catalogo",
                table: "concepto_linea");

            migrationBuilder.DropColumn(
                name: "acreedor_id",
                schema: "catalogo",
                table: "asignacion_concepto");

            migrationBuilder.DropColumn(
                name: "desde",
                schema: "catalogo",
                table: "asignacion_concepto");

            migrationBuilder.DropColumn(
                name: "hasta",
                schema: "catalogo",
                table: "asignacion_concepto");

            migrationBuilder.DropColumn(
                name: "tipo_tercero",
                schema: "catalogo",
                table: "asignacion_concepto");
        }
    }
}
