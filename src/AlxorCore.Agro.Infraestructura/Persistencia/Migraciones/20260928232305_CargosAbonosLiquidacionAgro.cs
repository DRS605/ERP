using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CargosAbonosLiquidacionAgro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "abono",
                schema: "agro",
                table: "concepto_liquidacion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "agricultor_id",
                schema: "agro",
                table: "concepto_liquidacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "agro",
                table: "concepto_liquidacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "producto_id",
                schema: "agro",
                table: "concepto_liquidacion",
                type: "uuid",
                nullable: true);
            migrationBuilder.Sql("""
                ALTER TABLE agro.concepto_liquidacion DROP CONSTRAINT ck_concepto_liquidacion_tipo;
                ALTER TABLE agro.concepto_liquidacion ADD CONSTRAINT ck_concepto_liquidacion_tipo CHECK (tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo', 'PorEnvase'));
                ALTER TABLE agro.descuento_liquidacion DROP CONSTRAINT ck_descuento_liquidacion_importe;
                ALTER TABLE agro.descuento_liquidacion ADD CONSTRAINT ck_descuento_liquidacion_importe CHECK (tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo', 'PorEnvase'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agro.descuento_liquidacion DROP CONSTRAINT ck_descuento_liquidacion_importe;
                ALTER TABLE agro.descuento_liquidacion ADD CONSTRAINT ck_descuento_liquidacion_importe CHECK (importe >= 0 AND tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo'));
                ALTER TABLE agro.concepto_liquidacion DROP CONSTRAINT ck_concepto_liquidacion_tipo;
                ALTER TABLE agro.concepto_liquidacion ADD CONSTRAINT ck_concepto_liquidacion_tipo CHECK (tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo'));
                """);
            migrationBuilder.DropColumn(
                name: "abono",
                schema: "agro",
                table: "concepto_liquidacion");

            migrationBuilder.DropColumn(
                name: "agricultor_id",
                schema: "agro",
                table: "concepto_liquidacion");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "agro",
                table: "concepto_liquidacion");

            migrationBuilder.DropColumn(
                name: "producto_id",
                schema: "agro",
                table: "concepto_liquidacion");
        }
    }
}
