using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InteresesRetencionPagares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "intereses",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "pagare_id",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_retencion",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "retencion",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_interes",
                schema: "tesoreria",
                table: "entrega_cuenta_proveedor",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555', '4311', '4310', '650', '769', '4751', '401')
                        AND cuenta_bancaria_id IS NULL));
                ALTER TABLE tesoreria.liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_liquidacion_pagos_forma;
                ALTER TABLE tesoreria.liquidacion_pagos ADD CONSTRAINT ck_liquidacion_pagos_forma CHECK (forma_pago IN ('Pendiente', 'Directo', 'Remesa', 'Pagare'));
                ALTER TABLE tesoreria.liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_liquidacion_pagos_importes;
                ALTER TABLE tesoreria.liquidacion_pagos ADD CONSTRAINT ck_liquidacion_pagos_importes CHECK (a_pagar >= 0 AND entregas_canceladas >= 0 AND compensado >= 0
                    AND intereses >= 0 AND retencion >= 0 AND liquido >= 0 AND liquido = a_pagar - entregas_canceladas - compensado - intereses - retencion);
                ALTER TABLE tesoreria.linea_liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_linea_liquidacion_pagos_tipo;
                ALTER TABLE tesoreria.linea_liquidacion_pagos ADD CONSTRAINT ck_linea_liquidacion_pagos_tipo
                    CHECK (tipo IN ('EntregaCuenta', 'Compensacion', 'CobroCompensado', 'Pago', 'Intereses', 'Retencion', 'Pagare'));
                ALTER TABLE tesoreria.entrega_cuenta_proveedor ADD CONSTRAINT ck_entrega_cuenta_interes CHECK (porcentaje_interes BETWEEN 0 AND 100);
                ALTER TABLE tesoreria.liquidacion_pagos ADD CONSTRAINT ck_liquidacion_pagos_retencion CHECK (porcentaje_retencion BETWEEN 0 AND 50);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555', '4311', '4310', '650') AND cuenta_bancaria_id IS NULL));
                ALTER TABLE tesoreria.liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_liquidacion_pagos_forma;
                ALTER TABLE tesoreria.liquidacion_pagos ADD CONSTRAINT ck_liquidacion_pagos_forma CHECK (forma_pago IN ('Pendiente', 'Directo', 'Remesa'));
                ALTER TABLE tesoreria.liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_liquidacion_pagos_importes;
                ALTER TABLE tesoreria.liquidacion_pagos ADD CONSTRAINT ck_liquidacion_pagos_importes CHECK (a_pagar >= 0 AND entregas_canceladas >= 0 AND compensado >= 0
                    AND liquido >= 0 AND liquido = a_pagar - entregas_canceladas - compensado);
                ALTER TABLE tesoreria.linea_liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_linea_liquidacion_pagos_tipo;
                ALTER TABLE tesoreria.linea_liquidacion_pagos ADD CONSTRAINT ck_linea_liquidacion_pagos_tipo CHECK (tipo IN ('EntregaCuenta', 'Compensacion', 'CobroCompensado', 'Pago'));
                ALTER TABLE tesoreria.liquidacion_pagos DROP CONSTRAINT IF EXISTS ck_liquidacion_pagos_retencion;
                """);
            migrationBuilder.DropColumn(
                name: "intereses",
                schema: "tesoreria",
                table: "liquidacion_pagos");

            migrationBuilder.DropColumn(
                name: "pagare_id",
                schema: "tesoreria",
                table: "liquidacion_pagos");

            migrationBuilder.DropColumn(
                name: "porcentaje_retencion",
                schema: "tesoreria",
                table: "liquidacion_pagos");

            migrationBuilder.DropColumn(
                name: "retencion",
                schema: "tesoreria",
                table: "liquidacion_pagos");

            migrationBuilder.DropColumn(
                name: "porcentaje_interes",
                schema: "tesoreria",
                table: "entrega_cuenta_proveedor");
        }
    }
}
