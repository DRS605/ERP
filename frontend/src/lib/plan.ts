/** Plan contratado: qué partes de la interfaz puede ver la empresa activa. */

export interface Plan {
  edicion: string | null;
  modulos: string[];
}

/**
 * ¿Está contratado el módulo? Sin módulo (base) o sin plan en el token (tokens antiguos),
 * no se restringe nada: la API sigue siendo la que decide.
 */
export function contratado(modulo: string | undefined, plan: Plan | null): boolean {
  if (!modulo || !plan || !plan.edicion) return true;
  return plan.modulos.includes(modulo);
}
