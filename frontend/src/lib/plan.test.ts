import { contratado } from "./plan";

describe("contratado", () => {
  const gestion = { edicion: "gestion", modulos: ["ventas", "compras", "inventario"] };

  it("la base siempre se ve", () => {
    expect(contratado(undefined, gestion)).toBe(true);
  });

  it("un módulo del plan se ve y uno fuera del plan no", () => {
    expect(contratado("ventas", gestion)).toBe(true);
    expect(contratado("contabilidad", gestion)).toBe(false);
  });

  it("sin plan en el token no se oculta nada", () => {
    expect(contratado("contabilidad", null)).toBe(true);
    expect(contratado("contabilidad", { edicion: null, modulos: [] })).toBe(true);
  });
});
