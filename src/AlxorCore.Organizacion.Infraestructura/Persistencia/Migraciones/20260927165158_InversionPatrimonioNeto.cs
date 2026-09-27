using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InversionPatrimonioNeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "coste_inversion",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuenta_inversion",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "patrimonio_adquisicion",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "titular_id",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_perimetro_titular",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                column: "titular_id");

            migrationBuilder.Sql("""
                ALTER TABLE organizacion.perimetro_consolidacion
                    ADD CONSTRAINT fk_perimetro_titular FOREIGN KEY (titular_id) REFERENCES organizacion.empresa (id) ON DELETE SET NULL,
                    ADD CONSTRAINT ck_perimetro_titular CHECK (titular_id IS NULL OR titular_id <> empresa_id),
                    ADD CONSTRAINT ck_perimetro_inversion CHECK ((titular_id IS NULL) = (cuenta_inversion IS NULL) AND (coste_inversion IS NULL OR coste_inversion >= 0));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.perimetro_consolidacion
                    DROP CONSTRAINT IF EXISTS fk_perimetro_titular,
                    DROP CONSTRAINT IF EXISTS ck_perimetro_titular,
                    DROP CONSTRAINT IF EXISTS ck_perimetro_inversion;
                """);

            migrationBuilder.DropIndex(
                name: "ix_perimetro_titular",
                schema: "organizacion",
                table: "perimetro_consolidacion");

            migrationBuilder.DropColumn(
                name: "coste_inversion",
                schema: "organizacion",
                table: "perimetro_consolidacion");

            migrationBuilder.DropColumn(
                name: "cuenta_inversion",
                schema: "organizacion",
                table: "perimetro_consolidacion");

            migrationBuilder.DropColumn(
                name: "patrimonio_adquisicion",
                schema: "organizacion",
                table: "perimetro_consolidacion");

            migrationBuilder.DropColumn(
                name: "titular_id",
                schema: "organizacion",
                table: "perimetro_consolidacion");
        }
    }
}
