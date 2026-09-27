/** Utilidades de formato y cálculo del módulo de documentos. */
import { useEffect, useRef, useState } from "react";

const formatoEur = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const formatoCant = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 });

export const eur = (n: number | null | undefined) => `${formatoEur.format(Number(n) || 0)} €`;
export const num2 = (n: number | null | undefined) => formatoEur.format(Number(n) || 0);
export const cant = (n: number | null | undefined) => formatoCant.format(Number(n) || 0);
export const fecha = (s: string | null | undefined) => {
  if (!s) return "";
  const p = String(s).slice(0, 10).split("-");
  return p.length === 3 ? `${p[2]}/${p[1]}/${p[0]}` : String(s);
};
export const hoyIso = () => new Date().toISOString().slice(0, 10);
export const redondear2 = (n: number) => Math.round((n + Number.EPSILON) * 100) / 100;

let secuencia = 0;
export const nuevaClave = () => `l${Date.now().toString(36)}${(++secuencia).toString(36)}`;

/** Valor que solo cambia cuando `valor` lleva `ms` quieto (para no llamar al servidor en cada tecla). */
export function useRetardado<T>(valor: T, ms: number): T {
  const [retardado, setRetardado] = useState(valor);
  useEffect(() => {
    const t = setTimeout(() => setRetardado(valor), ms);
    return () => clearTimeout(t);
  }, [valor, ms]);
  return retardado;
}

/** Evita que una respuesta antigua pise a una más reciente. */
export function useUltimaPeticion() {
  const n = useRef(0);
  return () => {
    const mia = ++n.current;
    return () => mia === n.current;
  };
}

/** Pill de estado con los colores de la interfaz. */
export function clasePill(estado: string): string {
  switch (estado) {
    case "Emitida":
    case "Aceptado":
    case "Facturado":
    case "Servido":
    case "Recibido":
      return "pill ok";
    case "Anulada":
    case "Rechazado":
    case "Cancelado":
      return "pill neg";
    case "Borrador":
    case "Rectificada":
      return "pill wait";
    default:
      return "pill part";
  }
}
