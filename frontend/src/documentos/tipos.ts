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
}

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
}

export interface Pagina<T> {
  elementos: T[];
  total: number;
  pagina: number;
  tamanoPagina: number;
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
