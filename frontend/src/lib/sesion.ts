/** Estado de sesión persistido en el navegador (token y empresa activa). */
import type { Empresa, Usuario } from "./tipos";

const CLAVE = "alxor.sesion";

interface SesionGuardada {
  token: string;
  usuario: Usuario;
  empresa: Empresa;
  /** Plan de la empresa activa (edición y módulos). Sin edición no se restringe nada. */
  plan?: { edicion: string | null; modulos: string[] };
}

let actual: SesionGuardada | null = leer();

function leer(): SesionGuardada | null {
  try {
    const bruto = localStorage.getItem(CLAVE);
    return bruto ? (JSON.parse(bruto) as SesionGuardada) : null;
  } catch {
    return null;
  }
}

export const sesion = {
  get token(): string | null {
    return actual?.token ?? null;
  },
  get usuario(): Usuario | null {
    return actual?.usuario ?? null;
  },
  get empresa(): Empresa | null {
    return actual?.empresa ?? null;
  },
  get plan(): { edicion: string | null; modulos: string[] } | null {
    return actual?.plan ?? null;
  },
  guardar(datos: SesionGuardada) {
    actual = datos;
    try {
      localStorage.setItem(CLAVE, JSON.stringify(datos));
    } catch {
      /* almacenamiento no disponible: la sesión vive solo en memoria */
    }
  },
  limpiar() {
    actual = null;
    try {
      localStorage.removeItem(CLAVE);
    } catch {
      /* ignore */
    }
  },
};
