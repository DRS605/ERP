using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ComercioExteriorCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "eori",
                schema: "terceros",
                table: "cliente",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "incoterm",
                schema: "terceros",
                table: "cliente",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lugar_incoterm",
                schema: "terceros",
                table: "cliente",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE terceros.cliente
                    ADD CONSTRAINT ck_cliente_incoterm CHECK (incoterm IS NULL OR incoterm IN ('EXW','FCA','CPT','CIP','DAP','DPU','DDP','FAS','FOB','CFR','CIF')),
                    ADD CONSTRAINT ck_cliente_eori CHECK (eori IS NULL OR eori ~ '^[A-Z]{2}[A-Z0-9]{1,15}$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE terceros.cliente DROP CONSTRAINT IF EXISTS ck_cliente_incoterm, DROP CONSTRAINT IF EXISTS ck_cliente_eori;");

            migrationBuilder.DropColumn(
                name: "eori",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "incoterm",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "lugar_incoterm",
                schema: "terceros",
                table: "cliente");
        }
    }
}
