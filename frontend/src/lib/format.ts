/** Utilidades de formato en español (euros y fechas), compartidas por toda la SPA. */

/**
 * Formatea un importe en euros con separador de miles «.» y decimal «,»: 1234.5 → "1.234,50 €".
 * Se hace a mano (no vía Intl) para dar un resultado idéntico en el navegador y en las pruebas,
 * con independencia de los datos de ICU disponibles en cada entorno.
 */
export function eur(n: number | null | undefined): string {
  const valor = Number(n) || 0;
  const negativo = valor < 0;
  const [enteraCruda, decimal] = Math.abs(valor).toFixed(2).split(".");
  const entera = enteraCruda.replace(/\B(?=(\d{3})+(?!\d))/g, ".");
  // Espacio duro (NBSP) antes del símbolo, como marca la convención española.
  return `${negativo ? "−" : ""}${entera},${decimal} €`;
}

/** Formatea una fecha ISO (YYYY-MM-DD o completa) como DD/MM/AAAA. Cadena vacía si no hay fecha. */
export function fecha(iso: string | null | undefined): string {
  if (!iso) return "";
  const partes = String(iso).slice(0, 10).split("-");
  return partes.length === 3 ? `${partes[2]}/${partes[1]}/${partes[0]}` : String(iso);
}

/** Formatea una cantidad sin decimales innecesarios (2 → "2", 1.5 → "1,5"). */
export function cantidad(n: number): string {
  return new Intl.NumberFormat("es-ES", { maximumFractionDigits: 6 }).format(Number(n) || 0);
}
