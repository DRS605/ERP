import { describe, expect, it } from "vitest";
import { clave, csv, rango, valor, variacion } from "./formato";

describe("formato del análisis", () => {
  it("formatea importes, porcentajes y números", () => {
    expect(valor(1234.5, "Moneda")).toMatch(/^1\.?234,50 €$/);
    expect(valor(12.5, "Porcentaje")).toMatch(/12,50 %/);
    expect(valor(null, "Moneda")).toBe("");
  });

  it("calcula la variación contra el periodo anterior", () => {
    expect(variacion(150, 100)).toBe(50);
    expect(variacion(50, -100)).toBe(150);
    expect(variacion(10, 0)).toBeNull();
    expect(variacion(10, null)).toBeNull();
  });

  it("hace legibles los periodos", () => {
    expect(clave("2026-03", "mes")).toBe("marzo 2026");
    expect(clave("2026-T2", "trimestre")).toBe("2026 · T2");
    expect(clave("2026-03-09", "dia")).toBe("09/03/2026");
    expect(clave(null)).toBe("(sin valor)");
  });

  it("calcula los periodos relativos", () => {
    const hoy = new Date(2026, 8, 28);
    expect(rango("este_mes", hoy)).toEqual({ desde: "2026-09-01", hasta: "2026-09-30" });
    expect(rango("mes_anterior", hoy)).toEqual({ desde: "2026-08-01", hasta: "2026-08-31" });
    expect(rango("este_trimestre", hoy)).toEqual({ desde: "2026-07-01", hasta: "2026-09-30" });
    expect(rango("trimestre_anterior", hoy)).toEqual({ desde: "2026-04-01", hasta: "2026-06-30" });
    expect(rango("ultimos_12_meses", hoy)).toEqual({ desde: "2025-10-01", hasta: "2026-09-30" });
    expect(rango("anio_anterior", hoy)).toEqual({ desde: "2025-01-01", hasta: "2025-12-31" });
    expect(rango("todo", hoy)).toEqual({ desde: null, hasta: null });
  });

  it("escribe CSV con punto y coma y coma decimal", () => {
    expect(csv(["a;b", 1.5, null])).toBe('"a;b";1,5;');
  });
});
