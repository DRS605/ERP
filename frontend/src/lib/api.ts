/** Cliente HTTP tipado de la API de ALXOR Core. Añade el token Bearer y normaliza los errores. */

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export interface OpcionesPeticion {
  method?: string;
  body?: unknown;
  headers?: Record<string, string>;
}

export type ObtenerToken = () => string | null;

/** Crea un cliente API con una función `fetch` y un proveedor de token inyectables (para pruebas). */
export function crearApi(fetchFn: typeof fetch, obtenerToken: ObtenerToken) {
  async function peticion<T>(ruta: string, opciones: OpcionesPeticion = {}): Promise<T> {
    const headers: Record<string, string> = { ...(opciones.headers ?? {}) };
    const token = obtenerToken();
    if (token) headers["Authorization"] = "Bearer " + token;

    let body: string | undefined;
    if (opciones.body !== undefined) {
      headers["Content-Type"] = "application/json";
      body = JSON.stringify(opciones.body);
    }

    const respuesta = await fetchFn(ruta, { method: opciones.method ?? "GET", headers, body });
    if (!respuesta.ok) {
      let mensaje = `HTTP ${respuesta.status}`;
      try {
        const problema = (await respuesta.json()) as { title?: string; detail?: string };
        mensaje = problema.detail || problema.title || mensaje;
      } catch {
        /* sin cuerpo JSON */
      }
      throw new ApiError(mensaje, respuesta.status);
    }

    if (respuesta.status === 204) return undefined as T;
    const tipo = respuesta.headers.get("content-type") ?? "";
    return (tipo.includes("json") ? await respuesta.json() : await respuesta.text()) as T;
  }

  return {
    get: <T>(ruta: string) => peticion<T>(ruta),
    post: <T>(ruta: string, body?: unknown) => peticion<T>(ruta, { method: "POST", body }),
    put: <T>(ruta: string, body?: unknown) => peticion<T>(ruta, { method: "PUT", body }),
    del: <T>(ruta: string) => peticion<T>(ruta, { method: "DELETE" }),
  };
}

export type Api = ReturnType<typeof crearApi>;
