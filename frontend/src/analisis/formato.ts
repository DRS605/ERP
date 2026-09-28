/** Formatos, periodos relativos y utilidades puras del análisis (probadas en formato.test.ts). */
import type { TipoDato } from "./tipos";

const f2 = new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
const f0 = new Intl.NumberFormat("es-ES", { maximumFractionDigits: 3 });

export function valor(v: number | null | undefined, tipo: TipoDato): string {
  if (v === null || v === undefined) return "";
  switch (tipo) {
    case "Moneda": return `${f2.format(v)} €`;
    case "Porcentaje": return `${f2.format(v)} %`;
    default: return f0.format(v);
  }
}

/** Variación porcentual entre el periodo actual y el anterior (null si no hay base). */
export function variacion(actual: number | null | undefined, anterior: number | null | undefined): number | null {
  if (actual == null || anterior == null || anterior === 0) return null;
  return Math.round(((actual - anterior) / Math.abs(anterior)) * 10000) / 100;
}

const MESES = ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];
const DIAS = ["", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado", "domingo"];

/** Texto de un valor de dimensión: fechas y periodos legibles («2026-03» → «marzo 2026»). */
export function clave(v: string | null | undefined, dimension?: string): string {
  if (v === null || v === undefined || v === "") return "(sin valor)";
  if (dimension === "mes" && /^\d{4}-\d{2}$/.test(v)) return `${MESES[Number(v.slice(5)) - 1]} ${v.slice(0, 4)}`;
  if (dimension === "mes_anio" && /^\d{2}$/.test(v)) return MESES[Number(v) - 1];
  if (dimension === "dia_semana" && /^\d$/.test(v)) return DIAS[Number(v)];
  if (dimension === "trimestre") return v.replace("-T", " · T");
  if (dimension === "semana") return v.replace("-S", " · semana ");
  if (/^\d{4}-\d{2}-\d{2}$/.test(v)) return `${v.slice(8, 10)}/${v.slice(5, 7)}/${v.slice(0, 4)}`;
  if (dimension === "mes_vencimiento" && /^\d{4}-\d{2}$/.test(v)) return `${MESES[Number(v.slice(5)) - 1]} ${v.slice(0, 4)}`;
  return v;
}

const iso = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

export const PERIODOS: [string, string][] = [
  ["este_mes", "Este mes"], ["mes_anterior", "Mes anterior"], ["este_trimestre", "Este trimestre"], ["trimestre_anterior", "Trimestre anterior"],
  ["este_anio", "Este año"], ["anio_anterior", "Año anterior"], ["ultimos_12_meses", "Últimos 12 meses"], ["hasta_hoy", "Este año hasta hoy"],
  ["todo", "Todo"], ["personalizado", "Personalizado…"],
];

/** Fechas de un periodo relativo a hoy (el que se guarda en los informes, así siempre están al día). */
export function rango(periodo: string, hoy = new Date()): { desde: string | null; hasta: string | null } {
  const y = hoy.getFullYear(), m = hoy.getMonth();
  const d = (a: number, mm: number, dd: number) => iso(new Date(a, mm, dd));
  const q = Math.floor(m / 3) * 3;
  switch (periodo) {
    case "este_mes": return { desde: d(y, m, 1), hasta: d(y, m + 1, 0) };
    case "mes_anterior": return { desde: d(y, m - 1, 1), hasta: d(y, m, 0) };
    case "este_trimestre": return { desde: d(y, q, 1), hasta: d(y, q + 3, 0) };
    case "trimestre_anterior": return { desde: d(y, q - 3, 1), hasta: d(y, q, 0) };
    case "este_anio": return { desde: d(y, 0, 1), hasta: d(y, 11, 31) };
    case "anio_anterior": return { desde: d(y - 1, 0, 1), hasta: d(y - 1, 11, 31) };
    case "ultimos_12_meses": return { desde: d(y, m - 11, 1), hasta: d(y, m + 1, 0) };
    case "hasta_hoy": return { desde: d(y, 0, 1), hasta: iso(hoy) };
    default: return { desde: null, hasta: null };
  }
}

/** Línea CSV (separador «;», como espera Excel en español). */
export function csv(celdas: (string | number | null | undefined)[]): string {
  return celdas.map((c) => {
    const s = c === null || c === undefined ? "" : typeof c === "number" ? String(c).replace(".", ",") : c;
    return /[";\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
  }).join(";");
}
