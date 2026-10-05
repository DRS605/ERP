using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SubastaEnLiquidacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "precio_id",
                schema: "agro",
                table: "linea_liquidacion",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "sesion_subasta_id",
                schema: "agro",
                table: "linea_liquidacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_sesion_subasta",
                schema: "agro",
                table: "linea_liquidacion",
                column: "sesion_subasta_id");

            // Cada línea se valora con un precio de la campaña o con el de una sesión de subasta (de la misma empresa).
            migrationBuilder.Sql("""
                ALTER TABLE agro.linea_liquidacion ADD CONSTRAINT fk_linea_liquidacion_sesion_subasta
                    FOREIGN KEY (sesion_subasta_id) REFERENCES agro.sesion_subasta (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_liquidacion ADD CONSTRAINT ck_linea_liquidacion_origen_precio
                    CHECK ((precio_id IS NULL) <> (sesion_subasta_id IS NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agro.linea_liquidacion DROP CONSTRAINT ck_linea_liquidacion_origen_precio;
                ALTER TABLE agro.linea_liquidacion DROP CONSTRAINT fk_linea_liquidacion_sesion_subasta;
                DELETE FROM agro.linea_liquidacion WHERE precio_id IS NULL;
                """);
            migrationBuilder.DropIndex(
                name: "ix_linea_liquidacion_sesion_subasta",
                schema: "agro",
                table: "linea_liquidacion");

            migrationBuilder.DropColumn(
                name: "sesion_subasta_id",
                schema: "agro",
                table: "linea_liquidacion");

            migrationBuilder.AlterColumn<Guid>(
                name: "precio_id",
                schema: "agro",
                table: "linea_liquidacion",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
