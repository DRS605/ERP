import { describe, expect, it } from "vitest";
import { anticiposDisponibles, repartoAnticipos, type Anticipo } from "./tipos";

const a = (id: string, fecha: string, disponible: number, estado = "Disponible"): Anticipo =>
  ({ id, clienteId: "c", fecha, importe: disponible, aplicado: 0, disponible, estado, concepto: "" });

describe("anticipos en facturas", () => {
  it("aplica primero los anticipos más antiguos hasta cubrir la factura", () => {
    const lista = [a("nuevo", "2026-05-01", 300), a("viejo", "2026-01-10", 200), a("anulado", "2025-01-01", 999, "Anulado")];
    expect(repartoAnticipos(lista, 450)).toEqual([{ id: "viejo", importe: 200 }, { id: "nuevo", importe: 250 }]);
  });

  it("no aplica más de lo disponible ni anticipos agotados", () => {
    expect(repartoAnticipos([a("x", "2026-01-01", 100), a("y", "2026-02-01", 0)], 1000)).toEqual([{ id: "x", importe: 100 }]);
    expect(anticiposDisponibles([a("y", "2026-02-01", 0)])).toEqual([]);
  });

  it("reparte al céntimo", () => {
    expect(repartoAnticipos([a("x", "2026-01-01", 0.1), a("y", "2026-01-02", 0.2)], 0.3)).toEqual([{ id: "x", importe: 0.1 }, { id: "y", importe: 0.2 }]);
  });
});
