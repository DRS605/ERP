/** Tipos de datos que devuelve la API de ALXOR Core (los usados por la SPA). */

export interface Usuario {
  id: string;
  nombre: string;
  email?: string;
}

export interface Empresa {
  id: string;
  nif: string;
  razonSocial: string;
  rolCodigo?: string;
}

export interface LoginRespuesta {
  token: string;
  usuario: Usuario;
  requiere2fa?: boolean;
}

export interface SeleccionRespuesta {
  token: string;
}

export interface Cliente {
  id: string;
  nombre: string;
  nifFiscal?: string | null;
  email?: string | null;
  poblacion: string;
  provincia: string;
  activo: boolean;
  porcentajeIrpfDefecto: number;
  actividadNegocioId?: string | null;
}

/** Actividad de negocio (clasificación transversal compartida por el grupo). */
export interface Actividad {
  id: string;
  nombre: string;
  activa: boolean;
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
