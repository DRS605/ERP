/** Exportación de listados a Excel (.xlsx, generado por POST /exportar/xlsx) y a CSV. */

export type TipoColumnaExportacion = "texto" | "numero" | "moneda" | "fecha" | "porcentaje";

export interface ColumnaExportacion {
  titulo: string;
  tipo: TipoColumnaExportacion;
  /** Total de la columna en el Excel: suma, media o ninguno (entonces vale el valor de `totales`). */
  total?: "suma" | "media" | "no";
}

export interface PeticionExportacion {
  titulo: string;
  columnas: ColumnaExportacion[];
  filas: (string | number | null)[][];
  /** Fila de totales: sus valores rotulan o fijan las columnas sin fórmula. */
  totales?: (string | number | null)[];
}

/** Nombre de fichero sin acentos ni símbolos, con la fecha del día. */
export function nombreFichero(titulo: string, extension: string): string {
  const base = titulo.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "listado";
  return `${base}-${new Date().toISOString().slice(0, 10)}.${extension}`;
}

function descargar(blob: Blob, nombre: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = nombre;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 60000);
}

/** Pide el Excel al servidor y lo descarga. */
export async function exportarXlsx(token: string | null, peticion: PeticionExportacion): Promise<void> {
  const r = await fetch("/exportar/xlsx", {
    method: "POST",
    headers: { "Content-Type": "application/json", ...(token ? { Authorization: "Bearer " + token } : {}) },
    body: JSON.stringify(peticion),
  });
  if (!r.ok) {
    let mensaje = `HTTP ${r.status}`;
    try {
      const p = (await r.json()) as { title?: string };
      mensaje = p.title || mensaje;
    } catch {
      /* sin cuerpo JSON */
    }
    throw new Error(mensaje);
  }
  descargar(await r.blob(), nombreFichero(peticion.titulo, "xlsx"));
}

/** Texto CSV (separador «;», formato español) de la exportación. */
export function textoCsv(peticion: PeticionExportacion): string {
  const celda = (v: string | number | null | undefined, tipo: TipoColumnaExportacion) => {
    if (v === null || v === undefined) return "";
    const s = typeof v === "number" ? (tipo === "texto" ? String(v) : v.toFixed(2).replace(".", ",")) : v;
    return /[";\n]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
  };
  const lineas = [peticion.columnas.map((c) => celda(c.titulo, "texto")).join(";")];
  for (const f of peticion.filas) lineas.push(f.map((v, j) => celda(v, peticion.columnas[j]?.tipo ?? "texto")).join(";"));
  return lineas.join("\r\n");
}

export function exportarCsv(peticion: PeticionExportacion) {
  descargar(new Blob(["﻿" + textoCsv(peticion)], { type: "text/csv;charset=utf-8" }), nombreFichero(peticion.titulo, "csv"));
}
