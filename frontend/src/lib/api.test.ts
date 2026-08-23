import { describe, it, expect, vi } from "vitest";
import { crearApi, ApiError } from "./api";

function respuestaJson(datos: unknown, ok = true, status = 200): Response {
  return {
    ok,
    status,
    headers: new Headers({ "content-type": "application/json" }),
    json: async () => datos,
    text: async () => JSON.stringify(datos),
  } as unknown as Response;
}

describe("cliente API", () => {
  it("añade el token Bearer y serializa el cuerpo en POST", async () => {
    const fetchFn = vi.fn().mockResolvedValue(respuestaJson({ id: 1 }));
    const api = crearApi(fetchFn, () => "tok123");

    const r = await api.post<{ id: number }>("/clientes", { nombre: "X" });

    expect(r.id).toBe(1);
    const [ruta, init] = fetchFn.mock.calls[0];
    expect(ruta).toBe("/clientes");
    expect(init.method).toBe("POST");
    expect(init.headers["Authorization"]).toBe("Bearer tok123");
    expect(init.headers["Content-Type"]).toBe("application/json");
    expect(init.body).toBe(JSON.stringify({ nombre: "X" }));
  });

  it("no añade Authorization si no hay token", async () => {
    const fetchFn = vi.fn().mockResolvedValue(respuestaJson([]));
    const api = crearApi(fetchFn, () => null);
    await api.get("/facturas");
    expect(fetchFn.mock.calls[0][1].headers["Authorization"]).toBeUndefined();
  });

  it("lanza ApiError con el title del problema y el status", async () => {
    const fetchFn = vi.fn().mockResolvedValue(respuestaJson({ title: "No autorizado" }, false, 401));
    const api = crearApi(fetchFn, () => null);
    await expect(api.get("/facturas")).rejects.toMatchObject({ message: "No autorizado", status: 401 });
    await expect(api.get("/facturas")).rejects.toBeInstanceOf(ApiError);
  });

  it("devuelve undefined en 204 sin intentar parsear", async () => {
    const fetchFn = vi.fn().mockResolvedValue({
      ok: true,
      status: 204,
      headers: new Headers(),
      json: async () => {
        throw new Error("no debería llamarse");
      },
    } as unknown as Response);
    const api = crearApi(fetchFn, () => "t");
    await expect(api.del("/x/1")).resolves.toBeUndefined();
  });
});
