/** Instancia por defecto del cliente API, ligada al token de la sesión activa. */
import { crearApi } from "./api";
import { sesion } from "./sesion";

export const api = crearApi((...args) => fetch(...args), () => sesion.token);
