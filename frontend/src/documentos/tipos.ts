/** Tipos del módulo de documentos (ventas y compras), tal como los devuelve la API. */

export type TipoVenta = "presupuesto" | "pedido" | "factura";

export interface Producto {
  id: string;
  referencia?: string | null;
  nombre: string;
  precioUnitario: number;
  codigoIva: string;
  porcentajeIva: number;
  unidad: string;
  precioCompra: number;
  controlarStock: boolean;
  stock: number;
  activo: boolean;
  familia?: string | null;
  pesoKg?: number | null;
  unidadCompra?: string | null;
  factorCompra?: number;
  precioCompraPorUnidadCompra?: number;
}

export interface Tercero {
  id: string;
  nombre: string;
  nifFiscal?: string | null;
  poblacion?: string;
  provincia?: string;
  pais?: string;
  recargoEquivalencia?: boolean;
  formaPagoDefectoId?: string | null;
  limiteRiesgo?: number | null;
  tarifaId?: string | null;
  porcentajeIrpfDefecto?: number;
  activo?: boolean;
}

export interface TipoIva {
  codigo: string;
  nombre: string;
  porcentaje: number;
  activo: boolean;
  impuesto?: "Iva" | "Igic";
}

/** Tipo equivalente al pasar una línea de un impuesto al otro (mismo criterio que el servidor). */
export function tipoEquivalente(codigo: string | null, a: "Iva" | "Igic"): string | null {
  if (!codigo) return codigo;
  const c = codigo.toUpperCase();
  const aIgic: Record<string, string> = { IVA21: "IGIC7", IVA10: "IGIC3", IVA4: "IGIC0", IVA0: "IGIC0", REAGP12: "REAGPIGIC", REAGP105: "REAGPIGIC" };
  const aIva: Record<string, string> = { IGIC7: "IVA21", IGIC3: "IVA10", IGIC0: "IVA0", REAGPIGIC: "REAGP12" };
  return (a === "Igic" ? aIgic[c] : aIva[c]) ?? codigo;
}

export interface FormaPago {
  id: string;
  nombre: string;
  diasVencimiento: number;
  activo: boolean;
}

export interface ConceptoCatalogo {
  id: string;
  codigo: string;
  nombre: string;
  efecto: "Precio" | "Coste";
  sentido: "Suma" | "Resta";
  calculo: "Porcentaje" | "PorUnidad" | "PorKilo" | "Importe";
  valor: number;
  /** Reglas del concepto (solo interesa el envase: las líneas pueden elegir el suyo). */
  asignaciones?: { envaseProductoId?: string | null }[];
}

export interface ConceptoAplicado {
  conceptoId: string;
  codigo: string;
  nombre: string;
  efecto: "Precio" | "Coste";
  sentido: "Suma" | "Resta";
  calculo: "Porcentaje" | "PorUnidad" | "PorKilo" | "Importe";
  valor: number;
  importe: number;
  /** Factura en divisa: el importe en la divisa (el importe es su contravalor en euros). */
  importeDivisa?: number | null;
  repartido: boolean;
}

/** Concepto pedido para una línea o para el documento; sin valor, el de su regla o el del concepto. */
export interface ConceptoSolicitado {
  conceptoId: string;
  valor: number | null;
}

/** Línea en edición. `precio` null: lo pone el servidor (tarifa del cliente o precio del artículo). */
export interface LineaEdicion {
  clave: string;
  productoId: string | null;
  referencia?: string | null;
  descripcion: string;
  cantidad: number;
  precio: number | null;
  dto: number;
  iva: string | null;
  /** Conceptos puestos a mano; undefined = los que se pongan solos. */
  conceptos?: ConceptoSolicitado[];
  /** Envase de la línea (un artículo): decide las reglas de conceptos por envase. */
  envaseProductoId?: string | null;
  unidad?: string;
  stock?: number | null;
  controlarStock?: boolean;
}

export interface LineaFactura {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  porcentajeDescuento: number;
  codigoIva: string;
  porcentajeIva: number;
  base: number;
  cuotaIva: number;
  costeUnitario: number;
  margen: number;
  porcentajeRecargo: number;
  cuotaRecargo: number;
  productoId?: string | null;
  conceptos?: ConceptoAplicado[] | null;
  importeConceptos: number;
  costeConceptos: number;
  /** Cuenta propia de la línea (438 en anticipos). */
  cuentaContable?: string | null;
  /** Anticipo facturado que descuenta esta línea (importes en negativo). */
  anticipoId?: string | null;
  /** Factura en divisa: precio y base en la divisa (los de euros son su contravalor). */
  precioDivisa?: number | null;
  baseDivisa?: number | null;
}

/** Divisas habituales para facturar fuera de la zona euro. */
export const DIVISAS = ["USD", "GBP", "CHF", "JPY", "CNY", "CAD", "MXN", "BRL", "SEK", "NOK", "DKK", "PLN", "MAD"];

export interface Factura {
  id: string;
  numeroCompleto: string;
  fechaEmision: string;
  fechaOperacion: string;
  fechaVencimiento: string;
  clienteId?: string | null;
  clienteNombre: string;
  clienteNif?: string | null;
  clienteCalle?: string;
  clienteCodigoPostal?: string;
  clientePoblacion?: string;
  clienteProvincia?: string;
  baseImponible: number;
  cuotaIva: number;
  porcentajeIrpf: number;
  retencionIrpf: number;
  recargoEquivalencia: boolean;
  recargoTotal: number;
  total: number;
  estado: string;
  tipo: string;
  huella?: string | null;
  rectificaFacturaId?: string | null;
  motivoRectificacion?: string | null;
  motivoAnulacion?: string | null;
  lineas: LineaFactura[];
  avisoRiesgo?: string | null;
  mencionFiscal?: string | null;
  impuesto?: string;
  moneda?: string | null;
  tasaCambio?: number | null;
  baseDivisa?: number | null;
  cuotaDivisa?: number | null;
  totalDivisa?: number | null;
}

export interface FacturaResumen {
  id: string;
  numeroCompleto: string;
  fechaEmision: string;
  fechaVencimiento: string;
  clienteNombre: string;
  clienteNif?: string | null;
  baseImponible: number;
  cuotaIva: number;
  retencionIrpf: number;
  total: number;
  estado: string;
  tipo: string;
  clienteId?: string | null;
}

export interface Pagina<T> {
  elementos: T[];
  total: number;
  pagina: number;
  tamanoPagina: number;
}

/** Totales de todo el resultado filtrado de un listado paginado (sin anulados; pendiente de los documentos vivos). */
export interface TotalesListado {
  documentos: number;
  baseImponible: number;
  impuestos: number;
  retenciones: number;
  total: number;
  pendiente: number;
  vencido: number;
  documentosVencidos: number;
}

/** Página de /facturas/buscar o /gastos/buscar con los totales del filtro y el pendiente de cada documento. */
export interface PaginaConTotales<T> extends Pagina<T> {
  totalPaginas: number;
  totales?: TotalesListado;
  pendientes?: Record<string, number>;
}

export interface LineaPresupuesto {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  porcentajeDescuento: number;
  codigoIva: string;
  porcentajeIva: number;
  base: number;
  cuotaIva: number;
  productoId?: string | null;
  conceptos?: ConceptoAplicado[] | null;
}

export interface Presupuesto {
  id: string;
  /** Divisa (null en euros): los importes están en ella. */
  moneda?: string | null;
  numeroCompleto: string;
  clienteId: string;
  clienteNombre: string;
  fecha: string;
  validez: string;
  estado: string;
  baseImponible: number;
  cuotaIva: number;
  total: number;
  facturaId?: string | null;
  lineas: LineaPresupuesto[];
}

export interface PresupuestoResumen {
  id: string;
  numeroCompleto: string;
  fecha: string;
  validez: string;
  clienteNombre: string;
  total: number;
  estado: string;
  facturaId?: string | null;
  baseImponible?: number;
  cuotaIva?: number;
  clienteId?: string | null;
}

export interface LineaPedidoVenta {
  id: string;
  productoId?: string | null;
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  porcentajeDescuento: number;
  codigoIva: string;
  base: number;
  cantidadServida: number;
  cantidadFacturada: number;
  pendienteServir: number;
  conceptos?: ConceptoAplicado[] | null;
}

export interface PedidoVenta {
  id: string;
  moneda?: string | null;
  estado: string;
  numeroCompleto: string;
  clienteId: string;
  clienteNombre: string;
  fecha: string;
  presupuestoOrigenId?: string | null;
  facturaId?: string | null;
  total: number;
  servidoCompleto: boolean;
  lineas: LineaPedidoVenta[];
}

export interface Albaran {
  id: string;
  moneda?: string | null;
  numeroCompleto: string;
  fecha: string;
  referencia?: string | null;
  lineas: { lineaPedidoId: string; descripcion: string; cantidad: number }[];
  anulado: boolean;
  motivoAnulacion?: string | null;
  almacenId?: string | null;
}

export interface LineaPedidoCompra {
  id: string;
  productoId?: string | null;
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  importe: number;
  cantidadRecibida: number;
  cantidadFacturada: number;
  pendienteRecibir: number;
  conceptos?: ConceptoAplicado[] | null;
  importeConceptos: number;
  costeConceptos: number;
  costeUnitarioEntrada: number;
}

export interface PedidoCompra {
  id: string;
  estado: string;
  numeroCompleto: string;
  proveedorId?: string | null;
  proveedorTexto: string;
  fecha: string;
  solicitudOrigenId?: string | null;
  total: number;
  recibidoCompleto: boolean;
  lineas: LineaPedidoCompra[];
  empresaOrigenId?: string | null;
}

export interface Saldo {
  total: number;
  liquidado: number;
  pendiente: number;
  estado: string;
}

export interface Almacen {
  id: string;
  codigo: string;
  nombre: string;
}

export interface TipoIvaCompleto extends TipoIva {
  clase: string;
  recargoEquivalencia: number;
}

export interface DesgloseGasto {
  codigoIva: string;
  porcentajeIva: number;
  base: number;
  cuota: number;
  cuotaDeducible: number;
  cuotaRecargo: number;
  autoliquidada: boolean;
}

export interface LineaGasto {
  descripcion?: string | null;
  cuentaGasto?: string | null;
  base: number;
  codigoIva: string;
  porcentajeIva: number;
  cuota: number;
  autoliquidada: boolean;
  porcentajeRecargo: number;
  cuotaRecargo: number;
  porcentajeDeducible: number;
  cuotaDeducible: number;
  suplido?: boolean;
}

/** Anticipo (entrega a cuenta) de un cliente: lo disponible se aplica a sus facturas (asiento 438 a 430). */
export interface Anticipo {
  id: string;
  clienteId: string;
  fecha: string;
  importe: number;
  aplicado: number;
  disponible: number;
  estado: string;
  concepto: string;
  /** Factura del anticipo (con su IVA): se descuenta en la factura final con una línea negativa. */
  facturaId?: string | null;
  facturaNumero?: string | null;
  baseFacturada?: number | null;
  disponibleBase?: number;
  codigoIva?: string | null;
}

/** Anticipos con algo por aplicar, del más antiguo al más reciente. */
export const anticiposDisponibles = (lista: Anticipo[]) =>
  lista.filter((a) => a.disponible > 0 && a.estado !== "Anulado" && !a.facturaId).sort((a, b) => a.fecha.localeCompare(b.fecha));

/** Anticipos facturados con base por descontar en la factura final, del más antiguo al más reciente. */
export const anticiposFacturados = (lista: Anticipo[]) =>
  lista.filter((a) => !!a.facturaId && (a.disponibleBase ?? 0) > 0 && a.estado !== "Anulado").sort((a, b) => a.fecha.localeCompare(b.fecha));

/** Reparto de un importe entre los anticipos, el más antiguo primero (lo que se aplica de cada uno). */
export function repartoAnticipos(anticipos: Anticipo[], importe: number): { id: string; importe: number }[] {
  let resto = Math.round(importe * 100);
  const reparto: { id: string; importe: number }[] = [];
  for (const a of anticiposDisponibles(anticipos)) {
    if (resto <= 0) break;
    const parte = Math.min(resto, Math.round(a.disponible * 100));
    if (parte > 0) reparto.push({ id: a.id, importe: parte / 100 });
    resto -= parte;
  }
  return reparto;
}

export interface Gasto {
  id: string;
  proveedorId?: string | null;
  proveedorTexto?: string | null;
  concepto: string;
  fecha: string;
  numeroFactura?: string | null;
  fechaFactura?: string | null;
  baseImponible: number;
  cuotaIva: number;
  recargoTotal: number;
  porcentajeIrpf: number;
  retencionIrpf: number;
  total: number;
  estado: string;
  avisoRiesgo?: string | null;
  afectacion?: string;
  lineas?: LineaGasto[] | null;
  vencimientos?: { fecha: string; importe: number }[] | null;
  desglose?: DesgloseGasto[] | null;
  esRectificativa?: boolean;
  rectificaGastoId?: string | null;
  numeroRectificado?: string | null;
  fechaRectificada?: string | null;
  motivoRectificacion?: string | null;
  moneda?: string | null;
  tasaCambio?: number | null;
  totalDivisa?: number | null;
  /** Suplidos (fuera de la base, sin impuesto): suman al total a pagar. */
  suplidos?: number;
}

export interface Cuenta {
  codigo: string;
  nombre: string;
}
