import { describe, expect, it } from "vitest";
import { nombreFichero, textoCsv } from "./exportar";
import { ordenar, totalesLocales, type Fila } from "./Listados";

const fila = (id: string, total: number, estado = "Emitida", extra: Partial<Fila> = {}): Fila => ({
  id, numero: "F-" + id, fecha: "2026-0" + id + "-01", tercero: "Cliente " + id, base: total / 1.21, impuestos: total - total / 1.21, total, estado, ...extra,
});

describe("totales de los listados", () => {
  it("suman sin anulados ni cancelados y separan lo vencido", () => {
    const t = totalesLocales(
      [fila("1", 121, "Emitida", { pendiente: 121, vencimiento: "2026-01-15" }), fila("2", 242, "Emitida", { pendiente: 0 }), fila("3", 1000, "Anulada", { pendiente: 1000 }), fila("4", 50, "Cancelado")],
      "2026-06-01",
    );
    expect(t.documentos).toBe(2);
    expect(t.total).toBe(363);
    expect(t.baseImponible).toBe(300);
    expect(t.pendiente).toBe(121);
    expect(t.vencido).toBe(121);
    expect(t.documentosVencidos).toBe(1);
  });

  it("ordenan por importe, texto o fecha en los dos sentidos", () => {
    const filas = [fila("2", 50), fila("1", 300), fila("3", 10)];
    expect(ordenar(filas, "total", false).map((f) => f.id)).toEqual(["3", "2", "1"]);
    expect(ordenar(filas, "total", true).map((f) => f.id)).toEqual(["1", "2", "3"]);
    expect(ordenar(filas, "fecha", true).map((f) => f.id)).toEqual(["3", "2", "1"]);
    expect(ordenar(filas, "tercero", false).map((f) => f.id)).toEqual(["1", "2", "3"]);
  });
});

describe("exportación", () => {
  it("genera CSV con «;», decimales con coma y comillas cuando hace falta", () => {
    const csv = textoCsv({
      titulo: "Facturas",
      columnas: [{ titulo: "Cliente", tipo: "texto" }, { titulo: "Total", tipo: "moneda" }],
      filas: [["Frutas; Levante", 1234.5], ["Ana \"la del puesto\"", null]],
    });
    expect(csv.split("\r\n")).toEqual(["Cliente;Total", "\"Frutas; Levante\";1234,50", "\"Ana \"\"la del puesto\"\"\";"]);
  });

  it("nombra el fichero sin acentos ni símbolos", () => {
    expect(nombreFichero("Facturas de proveedor y gastos", "xlsx")).toMatch(/^facturas-de-proveedor-y-gastos-\d{4}-\d{2}-\d{2}\.xlsx$/);
    expect(nombreFichero("Año · Pedidos", "csv")).toMatch(/^ano-pedidos-/);
  });
});
