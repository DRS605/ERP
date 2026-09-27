/** Contexto del módulo: API con el token de la interfaz, avisos y navegación entre pantallas. */
import { createContext, useContext } from "react";
import { crearApi, type Api } from "../lib/api";

export interface Anfitrion {
  /** Token de la sesión de la interfaz que monta el módulo. */
  token: () => string | null;
  /** Aviso breve (usa el de la interfaz). */
  aviso: (mensaje: string, tipo?: "ok" | "err" | "") => void;
  /** Abre una pantalla de la interfaz anfitriona (p. ej. «cobros»). */
  irA?: (vista: string) => void;
}

export type TipoDocumento = "presupuesto" | "pedido" | "factura" | "compra" | "gasto";

export interface Ruta {
  tipo: TipoDocumento;
  pantalla: "lista" | "editor" | "vista";
  id?: string | null;
  /** Datos para abrir el editor (duplicar, rectificar, desde otro documento). */
  semilla?: unknown;
}

export interface ContextoDocs {
  api: Api;
  anfitrion: Anfitrion;
  ruta: Ruta;
  navegar: (r: Ruta) => void;
}

export const Contexto = createContext<ContextoDocs | null>(null);

export function useDocs(): ContextoDocs {
  const c = useContext(Contexto);
  if (!c) throw new Error("Fuera del módulo de documentos");
  return c;
}

export function crearApiAnfitrion(anfitrion: Anfitrion): Api {
  return crearApi((entrada, init) => fetch(entrada, init), anfitrion.token);
}

/** Descarga un fichero protegido (PDF, XML) con el token y lo abre en otra pestaña. */
export async function abrirFichero(anfitrion: Anfitrion, ruta: string) {
  const token = anfitrion.token();
  const r = await fetch(ruta, { headers: token ? { Authorization: "Bearer " + token } : {} });
  if (!r.ok) throw new Error("No se pudo generar el documento");
  const url = URL.createObjectURL(await r.blob());
  window.open(url, "_blank");
  setTimeout(() => URL.revokeObjectURL(url), 60000);
}
