import { describe, it, expect } from "vitest";
import { eur, fecha, cantidad } from "./format";

describe("formato", () => {
  it("formatea euros en es-ES", () => {
    expect(eur(1234.5)).toBe("1.234,50 €");
    expect(eur(0)).toBe("0,00 €");
    expect(eur(null)).toBe("0,00 €");
  });

  it("formatea fechas ISO como DD/MM/AAAA", () => {
    expect(fecha("2026-08-23")).toBe("23/08/2026");
    expect(fecha("2026-08-23T10:00:00Z")).toBe("23/08/2026");
    expect(fecha(null)).toBe("");
    expect(fecha("")).toBe("");
  });

  it("formatea cantidades sin decimales innecesarios", () => {
    expect(cantidad(2)).toBe("2");
    expect(cantidad(1.5)).toBe("1,5");
  });
});
