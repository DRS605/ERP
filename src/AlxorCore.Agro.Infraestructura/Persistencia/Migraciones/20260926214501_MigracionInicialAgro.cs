using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialAgro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "agro");

            migrationBuilder.CreateTable(
                name: "agricultor",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    regimen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    porcentaje_retencion = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    autofacturacion_desde = table.Column<DateOnly>(type: "date", nullable: true),
                    motivo_bloqueo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    codigo_impuesto = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agricultor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "articulo_campana",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metodo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articulo_campana", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "campana",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campana", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    es_destrio = table.Column<bool>(type: "boolean", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clasificacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clasificacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "concepto_liquidacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concepto_liquidacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefijo_gs1 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    digito_extension = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genealogia",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kilos_origen = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genealogia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "liquidacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    regimen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    codigo_impuesto = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    porcentaje_impuesto = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    porcentaje_retencion = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    bruto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    total_descuentos = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    cuota_impuesto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    retencion = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    total_factura = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    a_pagar = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    emitida_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_envase",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_envase", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_partida",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    documento_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_partida", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pale",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sscc = table.Column<string>(type: "character(18)", fixedLength: true, maxLength: 18, nullable: false),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_expedicion = table.Column<DateOnly>(type: "date", nullable: true),
                    referencia_expedicion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pale", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parcela",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    referencia_sigpac = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    superficie_ha = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variedad = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    centro_analitico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcela", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parte_confeccion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    centro_analitico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    porcentaje_indirectos = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    reparto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    coste_fruta = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    coste_materiales = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    coste_mano_obra = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    coste_maquinaria = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    coste_indirectos = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    coste_total = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    validado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parte_confeccion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "partida",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    kilos_iniciales = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linea_recepcion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: true),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: true),
                    calibre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    parte_confeccion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coste_kg = table.Column<decimal>(type: "numeric(14,6)", nullable: true),
                    anulada = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partida", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "precio_liquidacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precio_liquidacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recepcion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    matricula = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    conductor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    confirmada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recepcion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tarifa_coste",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recurso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo_hora = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    coste_unitario = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifa_coste", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_clasificacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kg_muestra = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    clasificacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_clasificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_clasificacion_clasificacion_clasificacion_id",
                        column: x => x.clasificacion_id,
                        principalSchema: "agro",
                        principalTable: "clasificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "descuento_liquidacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    concepto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    @base = table.Column<decimal>(name: "base", type: "numeric(14,2)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_descuento_liquidacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_descuento_liquidacion_liquidacion_liquidacion_id",
                        column: x => x.liquidacion_id,
                        principalSchema: "agro",
                        principalTable: "liquidacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_liquidacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    precio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_recepcion = table.Column<DateOnly>(type: "date", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_liquidacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_liquidacion_liquidacion_liquidacion_id",
                        column: x => x.liquidacion_id,
                        principalSchema: "agro",
                        principalTable: "liquidacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consumo_parte",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    coste_kg = table.Column<decimal>(type: "numeric(14,6)", nullable: false),
                    coste = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumo_parte", x => x.id);
                    table.ForeignKey(
                        name: "FK_consumo_parte_parte_confeccion_parte_id",
                        column: x => x.parte_id,
                        principalSchema: "agro",
                        principalTable: "parte_confeccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mano_obra_parte",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tipo_hora = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    horas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    piezas = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    tarifa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coste_unitario = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    coste = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mano_obra_parte", x => x.id);
                    table.ForeignKey(
                        name: "FK_mano_obra_parte_parte_confeccion_parte_id",
                        column: x => x.parte_id,
                        principalSchema: "agro",
                        principalTable: "parte_confeccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maquina_parte",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    horas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    tarifa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coste_unitario = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    coste = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maquina_parte", x => x.id);
                    table.ForeignKey(
                        name: "FK_maquina_parte_parte_confeccion_parte_id",
                        column: x => x.parte_id,
                        principalSchema: "agro",
                        principalTable: "parte_confeccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "material_parte",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    coste_unitario = table.Column<decimal>(type: "numeric(12,4)", nullable: false),
                    coste = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_material_parte", x => x.id);
                    table.ForeignKey(
                        name: "FK_material_parte_parte_confeccion_parte_id",
                        column: x => x.parte_id,
                        principalSchema: "agro",
                        principalTable: "parte_confeccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "salida_parte",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_linea = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    factor = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    calibre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coste = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    parte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salida_parte", x => x.id);
                    table.ForeignKey(
                        name: "FK_salida_parte_parte_confeccion_parte_id",
                        column: x => x.parte_id,
                        principalSchema: "agro",
                        principalTable: "parte_confeccion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_recepcion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_linea = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_recoleccion = table.Column<DateOnly>(type: "date", nullable: true),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    precio_estimado_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: true),
                    calibre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    neto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    envases = table.Column<int>(type: "integer", nullable: true),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_recepcion", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_recepcion_recepcion_recepcion_id",
                        column: x => x.recepcion_id,
                        principalSchema: "agro",
                        principalTable: "recepcion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pesada",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    secuencia = table.Column<int>(type: "integer", nullable: false),
                    bruto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    tara_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    envases = table.Column<int>(type: "integer", nullable: false),
                    bascula = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pesada", x => x.id);
                    table.ForeignKey(
                        name: "FK_pesada_recepcion_recepcion_id",
                        column: x => x.recepcion_id,
                        principalSchema: "agro",
                        principalTable: "recepcion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_agricultor_proveedor",
                schema: "agro",
                table: "agricultor",
                columns: new[] { "empresa_id", "proveedor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_articulo_campana",
                schema: "agro",
                table: "articulo_campana",
                columns: new[] { "campana_id", "producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_campana_codigo",
                schema: "agro",
                table: "campana",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_categoria_codigo",
                schema: "agro",
                table: "categoria",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_clasificacion_definitiva",
                schema: "agro",
                table: "clasificacion",
                column: "partida_id",
                unique: true,
                filter: "estado = 'Definitiva'");

            migrationBuilder.CreateIndex(
                name: "ux_concepto_liquidacion_codigo",
                schema: "agro",
                table: "concepto_liquidacion",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_empresa",
                schema: "agro",
                table: "configuracion",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_consumo_parte_pale",
                schema: "agro",
                table: "consumo_parte",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_consumo_parte_parte",
                schema: "agro",
                table: "consumo_parte",
                column: "parte_id");

            migrationBuilder.CreateIndex(
                name: "ix_consumo_parte_partida",
                schema: "agro",
                table: "consumo_parte",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_descuento_liquidacion_concepto",
                schema: "agro",
                table: "descuento_liquidacion",
                column: "concepto_id");

            migrationBuilder.CreateIndex(
                name: "ix_descuento_liquidacion_liquidacion",
                schema: "agro",
                table: "descuento_liquidacion",
                column: "liquidacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_genealogia_destino",
                schema: "agro",
                table: "genealogia",
                column: "destino_id");

            migrationBuilder.CreateIndex(
                name: "ix_genealogia_parte",
                schema: "agro",
                table: "genealogia",
                column: "parte_id");

            migrationBuilder.CreateIndex(
                name: "ux_genealogia_origen_destino",
                schema: "agro",
                table: "genealogia",
                columns: new[] { "origen_id", "destino_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_clasificacion_categoria",
                schema: "agro",
                table: "linea_clasificacion",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_clasificacion_categoria",
                schema: "agro",
                table: "linea_clasificacion",
                columns: new[] { "clasificacion_id", "categoria_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_categoria",
                schema: "agro",
                table: "linea_liquidacion",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_linea_recepcion",
                schema: "agro",
                table: "linea_liquidacion",
                column: "linea_recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_liquidacion",
                schema: "agro",
                table: "linea_liquidacion",
                column: "liquidacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_partida",
                schema: "agro",
                table: "linea_liquidacion",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_precio",
                schema: "agro",
                table: "linea_liquidacion",
                column: "precio_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_recepcion",
                schema: "agro",
                table: "linea_liquidacion",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_recepcion_parcela",
                schema: "agro",
                table: "linea_recepcion",
                column: "parcela_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_recepcion_partida",
                schema: "agro",
                table: "linea_recepcion",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_recepcion_numero",
                schema: "agro",
                table: "linea_recepcion",
                columns: new[] { "recepcion_id", "numero_linea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_agricultor",
                schema: "agro",
                table: "liquidacion",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_campana",
                schema: "agro",
                table: "liquidacion",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "ux_liquidacion_numero",
                schema: "agro",
                table: "liquidacion",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mano_obra_parte_parte",
                schema: "agro",
                table: "mano_obra_parte",
                column: "parte_id");

            migrationBuilder.CreateIndex(
                name: "ix_mano_obra_parte_tarifa",
                schema: "agro",
                table: "mano_obra_parte",
                column: "tarifa_id");

            migrationBuilder.CreateIndex(
                name: "ix_maquina_parte_parte",
                schema: "agro",
                table: "maquina_parte",
                column: "parte_id");

            migrationBuilder.CreateIndex(
                name: "ix_maquina_parte_tarifa",
                schema: "agro",
                table: "maquina_parte",
                column: "tarifa_id");

            migrationBuilder.CreateIndex(
                name: "ix_material_parte_parte",
                schema: "agro",
                table: "material_parte",
                column: "parte_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_envase_agricultor",
                schema: "agro",
                table: "movimiento_envase",
                columns: new[] { "agricultor_id", "envase_producto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_envase_recepcion",
                schema: "agro",
                table: "movimiento_envase",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_partida_pale",
                schema: "agro",
                table: "movimiento_partida",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_partida_partida",
                schema: "agro",
                table: "movimiento_partida",
                columns: new[] { "partida_id", "pale_id" });

            migrationBuilder.CreateIndex(
                name: "ux_pale_sscc",
                schema: "agro",
                table: "pale",
                columns: new[] { "empresa_id", "sscc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parcela_agricultor",
                schema: "agro",
                table: "parcela",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ux_parcela_codigo",
                schema: "agro",
                table: "parcela",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parte_confeccion_campana",
                schema: "agro",
                table: "parte_confeccion",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "ix_parte_confeccion_fecha",
                schema: "agro",
                table: "parte_confeccion",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_parte_confeccion_numero",
                schema: "agro",
                table: "parte_confeccion",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_partida_agricultor",
                schema: "agro",
                table: "partida",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_partida_campana",
                schema: "agro",
                table: "partida",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "ix_partida_parcela",
                schema: "agro",
                table: "partida",
                column: "parcela_id");

            migrationBuilder.CreateIndex(
                name: "ix_partida_parte",
                schema: "agro",
                table: "partida",
                column: "parte_confeccion_id");

            migrationBuilder.CreateIndex(
                name: "ix_partida_recepcion",
                schema: "agro",
                table: "partida",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ux_partida_codigo",
                schema: "agro",
                table: "partida",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_partida_linea_recepcion",
                schema: "agro",
                table: "partida",
                column: "linea_recepcion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pesada_recepcion",
                schema: "agro",
                table: "pesada",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ux_pesada_secuencia",
                schema: "agro",
                table: "pesada",
                columns: new[] { "linea_id", "secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_precio_liquidacion_campana",
                schema: "agro",
                table: "precio_liquidacion",
                columns: new[] { "campana_id", "producto_id", "desde" });

            migrationBuilder.CreateIndex(
                name: "ix_precio_liquidacion_categoria",
                schema: "agro",
                table: "precio_liquidacion",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_recepcion_agricultor",
                schema: "agro",
                table: "recepcion",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_recepcion_campana",
                schema: "agro",
                table: "recepcion",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "ix_recepcion_fecha",
                schema: "agro",
                table: "recepcion",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_recepcion_numero",
                schema: "agro",
                table: "recepcion",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_salida_parte_categoria",
                schema: "agro",
                table: "salida_parte",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_salida_parte_pale",
                schema: "agro",
                table: "salida_parte",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_salida_parte_partida",
                schema: "agro",
                table: "salida_parte",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ux_salida_parte_numero",
                schema: "agro",
                table: "salida_parte",
                columns: new[] { "parte_id", "numero_linea" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tarifa_coste_busqueda",
                schema: "agro",
                table: "tarifa_coste",
                columns: new[] { "empresa_id", "recurso", "categoria", "tipo_hora", "desde" });

            // =============================== Garantías de la base de datos ===============================
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);

            // ----- Aislamiento: RLS por empresa en las raíces y por cabecera en las líneas -----
            foreach (var tabla in new[]
            {
                "campana", "agricultor", "parcela", "categoria", "articulo_campana", "precio_liquidacion", "concepto_liquidacion", "tarifa_coste",
                "configuracion", "recepcion", "partida", "movimiento_partida", "pale", "movimiento_envase", "clasificacion", "liquidacion",
                "parte_confeccion", "genealogia",
            })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            foreach (var (tabla, columna, padre) in new[]
            {
                ("linea_recepcion", "recepcion_id", "recepcion"), ("pesada", "recepcion_id", "recepcion"),
                ("linea_clasificacion", "clasificacion_id", "clasificacion"),
                ("linea_liquidacion", "liquidacion_id", "liquidacion"), ("descuento_liquidacion", "liquidacion_id", "liquidacion"),
                ("consumo_parte", "parte_id", "parte_confeccion"), ("mano_obra_parte", "parte_id", "parte_confeccion"),
                ("maquina_parte", "parte_id", "parte_confeccion"), ("material_parte", "parte_id", "parte_confeccion"),
                ("salida_parte", "parte_id", "parte_confeccion"),
            })
            {
                migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", tabla, columna, "agro", padre));
            }

            // ----- Claves foráneas (diferidas: EF no conoce el orden de estas relaciones entre agregados) -----
            migrationBuilder.Sql("""
                ALTER TABLE agro.parcela ADD CONSTRAINT fk_parcela_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.articulo_campana ADD CONSTRAINT fk_articulo_campana_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.precio_liquidacion
                    ADD CONSTRAINT fk_precio_liquidacion_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_precio_liquidacion_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.recepcion
                    ADD CONSTRAINT fk_recepcion_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_recepcion_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_recepcion
                    ADD CONSTRAINT fk_linea_recepcion_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_recepcion_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.pesada ADD CONSTRAINT fk_pesada_linea FOREIGN KEY (linea_id) REFERENCES agro.linea_recepcion (id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.partida
                    ADD CONSTRAINT fk_partida_recepcion FOREIGN KEY (recepcion_id) REFERENCES agro.recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_partida_linea_recepcion FOREIGN KEY (linea_recepcion_id) REFERENCES agro.linea_recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_partida_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_partida_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_partida_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_partida_parte FOREIGN KEY (parte_confeccion_id) REFERENCES agro.parte_confeccion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.movimiento_partida
                    ADD CONSTRAINT fk_movimiento_partida_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_movimiento_partida_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.movimiento_envase
                    ADD CONSTRAINT fk_movimiento_envase_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_movimiento_envase_recepcion FOREIGN KEY (recepcion_id) REFERENCES agro.recepcion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.clasificacion ADD CONSTRAINT fk_clasificacion_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_clasificacion ADD CONSTRAINT fk_linea_clasificacion_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.liquidacion
                    ADD CONSTRAINT fk_liquidacion_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_liquidacion_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_liquidacion
                    ADD CONSTRAINT fk_linea_liquidacion_linea_recepcion FOREIGN KEY (linea_recepcion_id) REFERENCES agro.linea_recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_liquidacion_recepcion FOREIGN KEY (recepcion_id) REFERENCES agro.recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_liquidacion_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_liquidacion_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_liquidacion_precio FOREIGN KEY (precio_id) REFERENCES agro.precio_liquidacion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.descuento_liquidacion ADD CONSTRAINT fk_descuento_liquidacion_concepto FOREIGN KEY (concepto_id) REFERENCES agro.concepto_liquidacion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.parte_confeccion ADD CONSTRAINT fk_parte_confeccion_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.consumo_parte
                    ADD CONSTRAINT fk_consumo_parte_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_consumo_parte_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.mano_obra_parte ADD CONSTRAINT fk_mano_obra_parte_tarifa FOREIGN KEY (tarifa_id) REFERENCES agro.tarifa_coste (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.maquina_parte ADD CONSTRAINT fk_maquina_parte_tarifa FOREIGN KEY (tarifa_id) REFERENCES agro.tarifa_coste (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.salida_parte
                    ADD CONSTRAINT fk_salida_parte_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_salida_parte_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_salida_parte_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.genealogia
                    ADD CONSTRAINT fk_genealogia_parte FOREIGN KEY (parte_id) REFERENCES agro.parte_confeccion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_genealogia_origen FOREIGN KEY (origen_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_genealogia_destino FOREIGN KEY (destino_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                """);

            // ----- Restricciones de valores -----
            migrationBuilder.Sql("""
                -- Dígito de control GS1 (SSCC, GTIN…): suma ponderada 3-1 desde la derecha.
                CREATE OR REPLACE FUNCTION agro.gs1_valido(codigo text) RETURNS boolean
                LANGUAGE plpgsql IMMUTABLE AS $f$
                DECLARE
                    n int := length(codigo);
                    suma int := 0;
                BEGIN
                    IF codigo !~ '^[0-9]{8,18}$' THEN
                        RETURN false;
                    END IF;
                    FOR i IN 1 .. n - 1 LOOP
                        suma := suma + substr(codigo, i, 1)::int * CASE WHEN (n - i) % 2 = 1 THEN 3 ELSE 1 END;
                    END LOOP;
                    RETURN (10 - suma % 10) % 10 = substr(codigo, n, 1)::int;
                END $f$;

                ALTER TABLE agro.campana ADD CONSTRAINT ck_campana_fechas CHECK (hasta >= desde);
                ALTER TABLE agro.agricultor
                    ADD CONSTRAINT ck_agricultor_regimen CHECK (regimen IN ('Reagp', 'General')),
                    ADD CONSTRAINT ck_agricultor_retencion CHECK (porcentaje_retencion BETWEEN 0 AND 50),
                    ADD CONSTRAINT ck_agricultor_impuesto CHECK ((regimen = 'Reagp') = (codigo_impuesto LIKE 'REAGP%'));
                ALTER TABLE agro.parcela
                    ADD CONSTRAINT ck_parcela_superficie CHECK (superficie_ha IS NULL OR superficie_ha > 0),
                    ADD CONSTRAINT ck_parcela_sigpac CHECK (referencia_sigpac IS NULL OR referencia_sigpac ~ '^[0-9]+(:[0-9]+){6}$');
                ALTER TABLE agro.articulo_campana ADD CONSTRAINT ck_articulo_campana_metodo CHECK (metodo IN ('PorClasificacion', 'PorPeriodo'));
                ALTER TABLE agro.precio_liquidacion
                    ADD CONSTRAINT ck_precio_liquidacion_fechas CHECK (hasta >= desde),
                    ADD CONSTRAINT ck_precio_liquidacion_precio CHECK (precio_kg >= 0);
                ALTER TABLE agro.concepto_liquidacion
                    ADD CONSTRAINT ck_concepto_liquidacion_tipo CHECK (tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo')),
                    ADD CONSTRAINT ck_concepto_liquidacion_valor CHECK (valor >= 0 AND (tipo <> 'PorcentajeBruto' OR valor <= 100));
                ALTER TABLE agro.tarifa_coste
                    ADD CONSTRAINT ck_tarifa_coste_recurso CHECK (recurso IN ('ManoObra', 'Maquina') AND (recurso = 'ManoObra' OR tipo_hora = 'Normal')),
                    ADD CONSTRAINT ck_tarifa_coste_tipo_hora CHECK (tipo_hora IN ('Normal', 'Extra', 'Nocturna', 'Festiva', 'Destajo')),
                    ADD CONSTRAINT ck_tarifa_coste_coste CHECK (coste_unitario >= 0),
                    ADD CONSTRAINT ck_tarifa_coste_fechas CHECK (hasta IS NULL OR hasta >= desde);
                ALTER TABLE agro.configuracion
                    ADD CONSTRAINT ck_configuracion_prefijo CHECK (prefijo_gs1 ~ '^[0-9]{7,10}$'),
                    ADD CONSTRAINT ck_configuracion_extension CHECK (digito_extension BETWEEN 0 AND 9);
                ALTER TABLE agro.recepcion
                    ADD CONSTRAINT ck_recepcion_estado CHECK (estado IN ('Borrador', 'Confirmada', 'Anulada')),
                    ADD CONSTRAINT ck_recepcion_numero CHECK ((numero IS NULL) = (estado = 'Borrador') AND (numero IS NULL OR numero > 0)),
                    ADD CONSTRAINT ck_recepcion_ejercicio CHECK (ejercicio = extract(year FROM fecha)),
                    ADD CONSTRAINT ck_recepcion_anulacion CHECK ((estado = 'Anulada') = (motivo_anulacion IS NOT NULL));
                ALTER TABLE agro.linea_recepcion
                    ADD CONSTRAINT ck_linea_recepcion_precio CHECK (precio_estimado_kg IS NULL OR precio_estimado_kg >= 0),
                    ADD CONSTRAINT ck_linea_recepcion_neto CHECK (neto_kg IS NULL OR neto_kg > 0),
                    ADD CONSTRAINT ck_linea_recepcion_envases CHECK (envases IS NULL OR envases >= 0);
                ALTER TABLE agro.pesada
                    ADD CONSTRAINT ck_pesada_kilos CHECK (tara_kg >= 0 AND bruto_kg > tara_kg),
                    ADD CONSTRAINT ck_pesada_envases CHECK (envases >= 0);
                ALTER TABLE agro.partida
                    ADD CONSTRAINT ck_partida_kilos CHECK (kilos_iniciales > 0),
                    ADD CONSTRAINT ck_partida_origen CHECK (
                        (origen = 'Recepcion' AND recepcion_id IS NOT NULL AND linea_recepcion_id IS NOT NULL AND agricultor_id IS NOT NULL AND parte_confeccion_id IS NULL)
                        OR (origen = 'Confeccion' AND parte_confeccion_id IS NOT NULL AND recepcion_id IS NULL)),
                    ADD CONSTRAINT ck_partida_coste CHECK (coste_kg IS NULL OR coste_kg >= 0);
                ALTER TABLE agro.movimiento_partida
                    ADD CONSTRAINT ck_movimiento_partida_tipo CHECK (tipo IN ('Entrada', 'Consumo', 'Paletizado', 'Expedicion', 'Ajuste', 'Anulacion')),
                    ADD CONSTRAINT ck_movimiento_partida_signo CHECK (kilos <> 0 AND (tipo <> 'Entrada' OR kilos > 0) AND (tipo NOT IN ('Consumo', 'Expedicion') OR kilos < 0));
                ALTER TABLE agro.pale
                    ADD CONSTRAINT ck_pale_sscc CHECK (length(sscc) = 18 AND agro.gs1_valido(sscc)),
                    ADD CONSTRAINT ck_pale_estado CHECK (estado IN ('Abierto', 'Cerrado', 'Expedido')),
                    ADD CONSTRAINT ck_pale_expedicion CHECK ((estado = 'Expedido') = (fecha_expedicion IS NOT NULL));
                ALTER TABLE agro.movimiento_envase ADD CONSTRAINT ck_movimiento_envase_cantidad CHECK (cantidad <> 0);
                ALTER TABLE agro.clasificacion ADD CONSTRAINT ck_clasificacion_estado CHECK (estado IN ('Provisional', 'Definitiva', 'Sustituida'));
                ALTER TABLE agro.linea_clasificacion ADD CONSTRAINT ck_linea_clasificacion_kilos CHECK (kg_muestra > 0);
                ALTER TABLE agro.liquidacion
                    ADD CONSTRAINT ck_liquidacion_estado CHECK (estado IN ('Borrador', 'Emitida', 'Anulada')),
                    ADD CONSTRAINT ck_liquidacion_numero CHECK ((numero IS NULL) = (estado = 'Borrador') AND (gasto_id IS NULL) = (estado = 'Borrador')),
                    ADD CONSTRAINT ck_liquidacion_anulacion CHECK ((estado = 'Anulada') = (motivo_anulacion IS NOT NULL)),
                    ADD CONSTRAINT ck_liquidacion_fechas CHECK (hasta >= desde AND fecha >= hasta AND ejercicio = extract(year FROM fecha)),
                    ADD CONSTRAINT ck_liquidacion_regimen CHECK (regimen IN ('Reagp', 'General') AND (regimen = 'Reagp') = (codigo_impuesto LIKE 'REAGP%')),
                    ADD CONSTRAINT ck_liquidacion_importes CHECK (
                        base_imponible >= 0 AND base_imponible = bruto - total_descuentos
                        AND cuota_impuesto = round(base_imponible * porcentaje_impuesto / 100, 2)
                        AND retencion = round(base_imponible * porcentaje_retencion / 100, 2)
                        AND total_factura = base_imponible + cuota_impuesto AND a_pagar = total_factura - retencion);
                ALTER TABLE agro.linea_liquidacion
                    ADD CONSTRAINT ck_linea_liquidacion_importe CHECK (kilos > 0 AND precio_kg >= 0 AND importe = round(kilos * precio_kg, 2));
                ALTER TABLE agro.descuento_liquidacion
                    ADD CONSTRAINT ck_descuento_liquidacion_importe CHECK (importe >= 0 AND tipo IN ('PorKilo', 'PorcentajeBruto', 'Fijo'));
                ALTER TABLE agro.parte_confeccion
                    ADD CONSTRAINT ck_parte_confeccion_estado CHECK (estado IN ('Borrador', 'Validado', 'Anulado') AND reparto IN ('PorKilos', 'PorFactor')),
                    ADD CONSTRAINT ck_parte_confeccion_numero CHECK ((numero IS NULL) = (estado = 'Borrador') AND ejercicio = extract(year FROM fecha)),
                    ADD CONSTRAINT ck_parte_confeccion_costes CHECK (
                        porcentaje_indirectos >= 0 AND coste_total = coste_fruta + coste_materiales + coste_mano_obra + coste_maquinaria + coste_indirectos);
                ALTER TABLE agro.consumo_parte ADD CONSTRAINT ck_consumo_parte_kilos CHECK (kilos > 0 AND coste >= 0);
                ALTER TABLE agro.mano_obra_parte ADD CONSTRAINT ck_mano_obra_parte_cantidades CHECK (horas >= 0 AND (piezas IS NULL OR piezas >= 0) AND coste >= 0);
                ALTER TABLE agro.maquina_parte ADD CONSTRAINT ck_maquina_parte_horas CHECK (horas > 0 AND coste >= 0);
                ALTER TABLE agro.material_parte ADD CONSTRAINT ck_material_parte_cantidad CHECK (cantidad > 0 AND coste >= 0);
                ALTER TABLE agro.salida_parte ADD CONSTRAINT ck_salida_parte_kilos CHECK (kilos > 0 AND factor >= 0 AND coste >= 0);
                ALTER TABLE agro.genealogia ADD CONSTRAINT ck_genealogia CHECK (origen_id <> destino_id AND kilos_origen > 0);
                """);

            // ----- Libros de solo inserción -----
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "movimiento_partida", "partida.movimiento_inmutable", "Los movimientos de partidas no se modifican ni se borran: se corrigen con otro movimiento."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "movimiento_envase", "envase.movimiento_inmutable", "Los movimientos de envases no se modifican ni se borran: se corrigen con otro movimiento."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "genealogia", "genealogia.inmutable", "La genealogía de las partidas no se modifica."));

            // ----- Documentos: el borrador cambia; los confirmados, solo en su transición (y sus líneas, en esa misma transacción) -----
            migrationBuilder.Sql("""
                ALTER TABLE agro.recepcion ADD COLUMN tx_estado xid8;
                ALTER TABLE agro.liquidacion ADD COLUMN tx_estado xid8;
                ALTER TABLE agro.parte_confeccion ADD COLUMN tx_estado xid8;
                ALTER TABLE agro.clasificacion ADD COLUMN tx_estado xid8;

                -- TG_ARGV: [0] estado de borrador, [1] transiciones permitidas ('A>B,C>D'), [2] columnas que la transición
                -- puede cambiar, [3] código de error, [4] mensaje. tx_estado guarda la transacción del último cambio de estado.
                CREATE OR REPLACE FUNCTION agro.documento_inmutable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    permitidas text[] := string_to_array(TG_ARGV[2], ',') || ARRAY['estado', 'tx_estado'];
                BEGIN
                    IF TG_OP = 'INSERT' THEN
                        NEW.tx_estado := pg_current_xact_id();
                        RETURN NEW;
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.estado <> TG_ARGV[0] AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error(TG_ARGV[3], TG_ARGV[4]);
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.estado IS DISTINCT FROM OLD.estado THEN
                        NEW.tx_estado := pg_current_xact_id();
                    END IF;
                    IF OLD.estado = TG_ARGV[0] THEN
                        RETURN NEW;
                    END IF;
                    IF (OLD.estado || '>' || NEW.estado) = ANY (string_to_array(TG_ARGV[1], ','))
                       AND (to_jsonb(NEW) - permitidas) = (to_jsonb(OLD) - permitidas) THEN
                        RETURN NEW;
                    END IF;
                    PERFORM public.alxor_error(TG_ARGV[3], TG_ARGV[4]);
                    RETURN NULL;
                END $f$;

                -- Línea de un documento: se modifica si la cabecera es borrador o si cambió de estado en esta transacción.
                -- TG_ARGV: [0] cabecera cualificada, [1] columna que la referencia, [2] estado de borrador, [3] código, [4] mensaje.
                CREATE OR REPLACE FUNCTION agro.linea_modificable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila jsonb := CASE WHEN TG_OP = 'DELETE' THEN to_jsonb(OLD) ELSE to_jsonb(NEW) END;
                    est text;
                    tx xid8;
                    empresa uuid;
                BEGIN
                    EXECUTE format('SELECT estado, tx_estado, empresa_id FROM %s WHERE id = $1', TG_ARGV[0])
                        INTO est, tx, empresa USING (fila ->> TG_ARGV[1])::uuid;
                    IF est IS NULL OR est = TG_ARGV[2] OR tx = pg_current_xact_id() OR public.alxor_borrando_empresa(empresa) THEN
                        RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                    END IF;
                    PERFORM public.alxor_error(TG_ARGV[3], TG_ARGV[4]);
                    RETURN NULL;
                END $f$;

                CREATE TRIGGER tg_recepcion_inmutable BEFORE INSERT OR UPDATE OR DELETE ON agro.recepcion FOR EACH ROW
                    EXECUTE FUNCTION agro.documento_inmutable('Borrador', 'Confirmada>Anulada', 'motivo_anulacion,anulada_en',
                        'recepcion.confirmada', 'Una recepción confirmada no se modifica: se anula.');
                CREATE TRIGGER tg_liquidacion_inmutable BEFORE INSERT OR UPDATE OR DELETE ON agro.liquidacion FOR EACH ROW
                    EXECUTE FUNCTION agro.documento_inmutable('Borrador', 'Emitida>Anulada', 'motivo_anulacion',
                        'liquidacion.emitida', 'Una liquidación emitida no se modifica: se anula.');
                CREATE TRIGGER tg_parte_confeccion_inmutable BEFORE INSERT OR UPDATE OR DELETE ON agro.parte_confeccion FOR EACH ROW
                    EXECUTE FUNCTION agro.documento_inmutable('Borrador', 'Validado>Anulado', '',
                        'parte.validado', 'Un parte validado no se modifica: se anula.');
                CREATE TRIGGER tg_clasificacion_inmutable BEFORE INSERT OR UPDATE OR DELETE ON agro.clasificacion FOR EACH ROW
                    EXECUTE FUNCTION agro.documento_inmutable('Provisional', 'Definitiva>Sustituida', '',
                        'clasificacion.definitiva', 'Una clasificación definitiva no se modifica: se sustituye por otra.');

                CREATE TRIGGER tg_linea_recepcion_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.linea_recepcion FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.recepcion', 'recepcion_id', 'Borrador', 'recepcion.confirmada', 'Una recepción confirmada no se modifica: se anula.');
                CREATE TRIGGER tg_pesada_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.pesada FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.recepcion', 'recepcion_id', 'Borrador', 'recepcion.confirmada', 'Una recepción confirmada no se modifica: se anula.');
                CREATE TRIGGER tg_linea_liquidacion_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.linea_liquidacion FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.liquidacion', 'liquidacion_id', 'Borrador', 'liquidacion.emitida', 'Una liquidación emitida no se modifica: se anula.');
                CREATE TRIGGER tg_descuento_liquidacion_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.descuento_liquidacion FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.liquidacion', 'liquidacion_id', 'Borrador', 'liquidacion.emitida', 'Una liquidación emitida no se modifica: se anula.');
                CREATE TRIGGER tg_linea_clasificacion_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.linea_clasificacion FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.clasificacion', 'clasificacion_id', 'Provisional', 'clasificacion.definitiva', 'Una clasificación definitiva no se modifica: se sustituye por otra.');
                """);

            migrationBuilder.Sql(string.Join("\n", new[] { "consumo_parte", "mano_obra_parte", "maquina_parte", "material_parte", "salida_parte" }.Select(t => $"""
                CREATE TRIGGER tg_{t}_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.{t} FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.parte_confeccion', 'parte_id', 'Borrador', 'parte.validado', 'Un parte validado no se modifica: se anula.');
                """)));

            // ----- Numeración sin huecos de recepciones, liquidaciones y partes -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.numeracion_sin_huecos() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    hay boolean;
                BEGIN
                    IF NEW.numero IS NULL OR NEW.numero = 1 THEN
                        RETURN NULL;
                    END IF;
                    EXECUTE format('SELECT EXISTS (SELECT 1 FROM %I.%I WHERE empresa_id = $1 AND ejercicio = $2 AND numero = $3)', TG_TABLE_SCHEMA, TG_TABLE_NAME)
                        INTO hay USING NEW.empresa_id, NEW.ejercicio, NEW.numero - 1;
                    IF NOT hay THEN
                        PERFORM public.alxor_error('numeracion.hueco', format('El número %s deja un hueco en la serie: falta el %s.', NEW.numero, NEW.numero - 1));
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_recepcion_numeracion AFTER INSERT OR UPDATE OF numero ON agro.recepcion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.numeracion_sin_huecos();
                CREATE CONSTRAINT TRIGGER tg_liquidacion_numeracion AFTER INSERT OR UPDATE OF numero ON agro.liquidacion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.numeracion_sin_huecos();
                CREATE CONSTRAINT TRIGGER tg_parte_confeccion_numeracion AFTER INSERT OR UPDATE OF numero ON agro.parte_confeccion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.numeracion_sin_huecos();
                """);

            // ----- Partidas: sus datos de origen no cambian; su saldo (suelto y en cada palé) nunca es negativo -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.partida_inmutable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('partida.inmutable', 'Las partidas no se borran: se anulan.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF (to_jsonb(NEW) - ARRAY['coste_kg', 'anulada']) <> (to_jsonb(OLD) - ARRAY['coste_kg', 'anulada']) OR (OLD.anulada AND NOT NEW.anulada) THEN
                        PERFORM public.alxor_error('partida.inmutable', 'El origen y los kilos de una partida no cambian, y una partida anulada no se recupera.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_partida_inmutable BEFORE UPDATE OR DELETE ON agro.partida FOR EACH ROW EXECUTE FUNCTION agro.partida_inmutable();

                CREATE OR REPLACE FUNCTION agro.movimiento_partida_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    saldo numeric;
                    est text;
                    sscc text;
                    partida_anulada boolean;
                BEGIN
                    SELECT coalesce(sum(kilos), 0) INTO saldo FROM agro.movimiento_partida
                     WHERE partida_id = NEW.partida_id AND pale_id IS NOT DISTINCT FROM NEW.pale_id;
                    IF saldo < 0 THEN
                        PERFORM public.alxor_error('partida.saldo_negativo',
                            'La partida no tiene tantos kilos ' || CASE WHEN NEW.pale_id IS NULL THEN 'sueltos.' ELSE 'en ese palé.' END);
                    END IF;
                    SELECT p.anulada INTO partida_anulada FROM agro.partida p WHERE p.id = NEW.partida_id;
                    IF partida_anulada AND NEW.tipo <> 'Anulacion' THEN
                        PERFORM public.alxor_error('partida.anulada', 'La partida está anulada.');
                    END IF;
                    IF NEW.pale_id IS NOT NULL THEN
                        SELECT p.estado, p.sscc INTO est, sscc FROM agro.pale p WHERE p.id = NEW.pale_id;
                        IF (NEW.tipo = 'Expedicion' AND est <> 'Expedido')
                           OR (NEW.tipo IN ('Entrada', 'Paletizado', 'Consumo', 'Ajuste') AND est <> 'Abierto')
                           OR (NEW.tipo = 'Anulacion' AND NEW.kilos > 0 AND est = 'Expedido') THEN
                            PERFORM public.alxor_error('pale.estado', format('El palé %s está %s: no admite ese movimiento.', sscc, lower(est)));
                        END IF;
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_movimiento_partida_valido AFTER INSERT ON agro.movimiento_partida
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.movimiento_partida_valido();

                -- Palés: el SSCC no cambia; Abierto ⇄ Cerrado → Expedido, y expedido ya no cambia.
                CREATE OR REPLACE FUNCTION agro.pale_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('pale.inmutable', 'Los palés no se borran.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.sscc <> OLD.sscc OR OLD.estado = 'Expedido'
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC no cambia y un palé expedido ya no se toca.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_pale_valido BEFORE UPDATE OR DELETE ON agro.pale FOR EACH ROW EXECUTE FUNCTION agro.pale_valido();
                """);

            // ----- Recepción confirmada: cada línea con su partida, con el neto y los envases de sus pesadas -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.recepcion_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    mal record;
                BEGIN
                    IF NEW.estado <> 'Confirmada' THEN
                        RETURN NULL;
                    END IF;
                    SELECT l.numero_linea INTO mal
                      FROM agro.linea_recepcion l
                      LEFT JOIN LATERAL (SELECT sum(p.bruto_kg - p.tara_kg) AS neto, sum(p.envases) AS envases FROM agro.pesada p WHERE p.linea_id = l.id) s ON true
                      LEFT JOIN agro.partida pa ON pa.id = l.partida_id
                     WHERE l.recepcion_id = NEW.id
                       AND (l.neto_kg IS DISTINCT FROM s.neto OR l.envases IS DISTINCT FROM s.envases::int OR pa.id IS NULL
                            OR pa.kilos_iniciales <> l.neto_kg OR pa.linea_recepcion_id <> l.id OR pa.agricultor_id <> NEW.agricultor_id)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('recepcion.no_cuadra', format('La línea %s de la recepción no cuadra con sus pesadas o su partida.', mal.numero_linea));
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM agro.linea_recepcion WHERE recepcion_id = NEW.id) THEN
                        PERFORM public.alxor_error('recepcion.sin_lineas', 'La recepción no tiene líneas.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_recepcion_cuadra AFTER INSERT OR UPDATE ON agro.recepcion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.recepcion_cuadra();
                """);

            // ----- Precios y tarifas: sin solapes, y los ya aplicados no cambian -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.precio_liquidacion_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion l JOIN agro.liquidacion q ON q.id = l.liquidacion_id
                         WHERE l.precio_id = OLD.id AND q.estado = 'Emitida') THEN
                        PERFORM public.alxor_error('precio.aplicado', 'El precio ya se aplicó en una liquidación emitida: no se cambia ni se borra.');
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.precio_liquidacion p
                                WHERE p.id <> NEW.id AND p.campana_id = NEW.campana_id AND p.producto_id = NEW.producto_id
                                  AND p.categoria_id IS NOT DISTINCT FROM NEW.categoria_id AND p.desde <= NEW.hasta AND NEW.desde <= p.hasta) THEN
                        PERFORM public.alxor_error('precio.solapado', 'Ya hay un precio de ese artículo y categoría en esas fechas.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_precio_liquidacion_valido BEFORE INSERT OR UPDATE OR DELETE ON agro.precio_liquidacion
                    FOR EACH ROW EXECUTE FUNCTION agro.precio_liquidacion_valido();

                CREATE OR REPLACE FUNCTION agro.tarifa_coste_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') AND (
                        EXISTS (SELECT 1 FROM agro.mano_obra_parte WHERE tarifa_id = OLD.id)
                        OR EXISTS (SELECT 1 FROM agro.maquina_parte WHERE tarifa_id = OLD.id)) THEN
                        PERFORM public.alxor_error('tarifa.aplicada', 'La tarifa ya se aplicó en un parte: no se cambia; crea otra con nueva vigencia.');
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.tarifa_coste t
                                WHERE t.id <> NEW.id AND t.empresa_id = NEW.empresa_id AND t.recurso = NEW.recurso AND t.categoria = NEW.categoria
                                  AND t.tipo_hora = NEW.tipo_hora AND t.desde <= coalesce(NEW.hasta, 'infinity') AND NEW.desde <= coalesce(t.hasta, 'infinity')) THEN
                        PERFORM public.alxor_error('tarifa.solapada', 'Ya hay una tarifa de esa categoría y tipo de hora en esas fechas.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_tarifa_coste_valida BEFORE INSERT OR UPDATE OR DELETE ON agro.tarifa_coste
                    FOR EACH ROW EXECUTE FUNCTION agro.tarifa_coste_valida();

                -- Una definitiva no se sustituye si la partida ya está en una liquidación.
                CREATE OR REPLACE FUNCTION agro.clasificacion_sustituible() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.estado = 'Definitiva' AND NEW.estado = 'Sustituida' AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion l JOIN agro.liquidacion q ON q.id = l.liquidacion_id
                         WHERE l.partida_id = OLD.partida_id AND q.estado <> 'Anulada') THEN
                        PERFORM public.alxor_error('clasificacion.liquidada', 'La partida ya está en una liquidación: su clasificación definitiva no se puede sustituir.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_clasificacion_sustituible BEFORE UPDATE ON agro.clasificacion
                    FOR EACH ROW EXECUTE FUNCTION agro.clasificacion_sustituible();
                """);

            // ----- Liquidación: cada entrega una sola vez, coherente con su recepción, su precio y sus totales -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.liquidacion_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    liq agro.liquidacion%ROWTYPE;
                    mal record;
                    id_liq uuid := (to_jsonb(CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END)
                                    ->> CASE WHEN TG_TABLE_NAME = 'liquidacion' THEN 'id' ELSE 'liquidacion_id' END)::uuid;
                BEGIN
                    SELECT * INTO liq FROM agro.liquidacion WHERE id = id_liq;
                    IF NOT FOUND OR liq.estado = 'Anulada' THEN
                        RETURN NULL;
                    END IF;

                    -- Una entrega (línea de recepción) solo está en una liquidación viva.
                    SELECT l.linea_recepcion_id INTO mal FROM agro.linea_liquidacion l
                     WHERE l.liquidacion_id = liq.id AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion o JOIN agro.liquidacion q ON q.id = o.liquidacion_id
                         WHERE o.linea_recepcion_id = l.linea_recepcion_id AND o.liquidacion_id <> liq.id AND q.estado <> 'Anulada')
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.entrega_duplicada', 'Una entrega ya está en otra liquidación.');
                    END IF;

                    -- Coherente con la recepción: confirmada, del agricultor y la campaña, en el periodo, con su partida.
                    SELECT l.id INTO mal FROM agro.linea_liquidacion l
                      JOIN agro.linea_recepcion lr ON lr.id = l.linea_recepcion_id
                      JOIN agro.recepcion r ON r.id = lr.recepcion_id
                     WHERE l.liquidacion_id = liq.id
                       AND (r.id <> l.recepcion_id OR r.estado <> 'Confirmada' OR r.agricultor_id <> liq.agricultor_id OR r.campana_id <> liq.campana_id
                            OR r.fecha NOT BETWEEN liq.desde AND liq.hasta OR r.fecha <> l.fecha_recepcion OR lr.partida_id IS DISTINCT FROM l.partida_id)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.linea_incoherente', 'Una línea de la liquidación no corresponde a una entrega confirmada del agricultor en el periodo.');
                    END IF;

                    -- Se liquidan todos los kilos netos de cada entrega, ni más ni menos.
                    SELECT lr.id INTO mal FROM agro.linea_recepcion lr
                      JOIN (SELECT linea_recepcion_id, sum(kilos) AS kilos FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id GROUP BY linea_recepcion_id) s
                        ON s.linea_recepcion_id = lr.id
                     WHERE s.kilos <> lr.neto_kg
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.kilos', 'Los kilos liquidados de una entrega no coinciden con su peso neto.');
                    END IF;

                    -- El precio aplicado es el vigente del artículo (y la categoría) en la fecha de la entrega.
                    SELECT l.id INTO mal FROM agro.linea_liquidacion l
                      JOIN agro.precio_liquidacion p ON p.id = l.precio_id
                      JOIN agro.linea_recepcion lr ON lr.id = l.linea_recepcion_id
                     WHERE l.liquidacion_id = liq.id
                       AND (p.precio_kg <> l.precio_kg OR p.campana_id <> liq.campana_id OR p.producto_id <> lr.producto_id
                            OR p.categoria_id IS DISTINCT FROM l.categoria_id OR l.fecha_recepcion NOT BETWEEN p.desde AND p.hasta)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.precio', 'Una línea no lleva el precio vigente de su artículo y categoría.');
                    END IF;

                    -- Totales de la cabecera.
                    IF liq.kilos <> (SELECT coalesce(sum(kilos), 0) FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id)
                       OR liq.bruto <> (SELECT coalesce(sum(importe), 0) FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id)
                       OR liq.total_descuentos <> (SELECT coalesce(sum(importe), 0) FROM agro.descuento_liquidacion WHERE liquidacion_id = liq.id) THEN
                        PERFORM public.alxor_error('liquidacion.totales', 'Los totales de la liquidación no cuadran con sus líneas y descuentos.');
                    END IF;

                    -- Emitida: el agricultor autorizó la autofacturación antes de la fecha.
                    IF liq.estado = 'Emitida' AND NOT EXISTS (
                        SELECT 1 FROM agro.agricultor a WHERE a.id = liq.agricultor_id AND a.autofacturacion_desde <= liq.fecha) THEN
                        PERFORM public.alxor_error('liquidacion.sin_autofacturacion', 'El agricultor no ha autorizado la autofacturación en esa fecha.');
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_liquidacion_cuadra AFTER INSERT OR UPDATE ON agro.liquidacion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.liquidacion_cuadra();
                CREATE CONSTRAINT TRIGGER tg_linea_liquidacion_cuadra AFTER INSERT OR UPDATE OR DELETE ON agro.linea_liquidacion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.liquidacion_cuadra();
                CREATE CONSTRAINT TRIGGER tg_descuento_liquidacion_cuadra AFTER INSERT OR UPDATE OR DELETE ON agro.descuento_liquidacion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.liquidacion_cuadra();
                """);

            // ----- Parte validado: coste repartido entero entre sus salidas, cada una con su partida; genealogía coherente -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.parte_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.estado <> 'Validado' THEN
                        RETURN NULL;
                    END IF;
                    IF NEW.coste_total <> (SELECT coalesce(sum(coste), 0) FROM agro.salida_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_fruta <> (SELECT coalesce(sum(coste), 0) FROM agro.consumo_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_materiales <> (SELECT coalesce(sum(coste), 0) FROM agro.material_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_mano_obra <> (SELECT coalesce(sum(coste), 0) FROM agro.mano_obra_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_maquinaria <> (SELECT coalesce(sum(coste), 0) FROM agro.maquina_parte WHERE parte_id = NEW.id) THEN
                        PERFORM public.alxor_error('parte.no_cuadra', 'El coste del parte no cuadra con sus líneas o no se ha repartido entero entre las salidas.');
                    END IF;
                    IF (SELECT coalesce(sum(kilos), 0) FROM agro.salida_parte WHERE parte_id = NEW.id) > (SELECT coalesce(sum(kilos), 0) FROM agro.consumo_parte WHERE parte_id = NEW.id)
                       OR EXISTS (SELECT 1 FROM agro.salida_parte s LEFT JOIN agro.partida p ON p.id = s.partida_id
                                   WHERE s.parte_id = NEW.id AND (p.id IS NULL OR p.parte_confeccion_id <> NEW.id OR p.kilos_iniciales <> s.kilos)) THEN
                        PERFORM public.alxor_error('parte.salidas', 'Las salidas del parte no cuadran con sus partidas o superan los kilos consumidos.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_parte_confeccion_cuadra AFTER INSERT OR UPDATE ON agro.parte_confeccion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.parte_cuadra();

                CREATE OR REPLACE FUNCTION agro.genealogia_coherente() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM agro.partida d WHERE d.id = NEW.destino_id AND d.parte_confeccion_id = NEW.parte_id)
                       OR NOT EXISTS (SELECT 1 FROM agro.consumo_parte c WHERE c.parte_id = NEW.parte_id AND c.partida_id = NEW.origen_id) THEN
                        PERFORM public.alxor_error('genealogia.incoherente', 'La genealogía debe unir una partida consumida en el parte con una obtenida en él.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_genealogia_coherente AFTER INSERT ON agro.genealogia
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.genealogia_coherente();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS agro.documento_inmutable() CASCADE;
                DROP FUNCTION IF EXISTS agro.linea_modificable() CASCADE;
                DROP FUNCTION IF EXISTS agro.numeracion_sin_huecos() CASCADE;
                DROP FUNCTION IF EXISTS agro.partida_inmutable() CASCADE;
                DROP FUNCTION IF EXISTS agro.movimiento_partida_valido() CASCADE;
                DROP FUNCTION IF EXISTS agro.pale_valido() CASCADE;
                DROP FUNCTION IF EXISTS agro.recepcion_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS agro.precio_liquidacion_valido() CASCADE;
                DROP FUNCTION IF EXISTS agro.tarifa_coste_valida() CASCADE;
                DROP FUNCTION IF EXISTS agro.clasificacion_sustituible() CASCADE;
                DROP FUNCTION IF EXISTS agro.liquidacion_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS agro.parte_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS agro.genealogia_coherente() CASCADE;
                DROP FUNCTION IF EXISTS agro.gs1_valido(text) CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "agricultor",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "articulo_campana",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "campana",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "categoria",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "concepto_liquidacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "configuracion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "consumo_parte",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "descuento_liquidacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "genealogia",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "linea_clasificacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "linea_liquidacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "linea_recepcion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "mano_obra_parte",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "maquina_parte",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "material_parte",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "movimiento_envase",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "movimiento_partida",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "pale",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "parcela",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "partida",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "pesada",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "precio_liquidacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "salida_parte",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "tarifa_coste",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "clasificacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "liquidacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "recepcion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "parte_confeccion",
                schema: "agro");
        }
    }
}
