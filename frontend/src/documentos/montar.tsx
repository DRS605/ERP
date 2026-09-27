/**
 * Punto de entrada del módulo de documentos para la interfaz clásica: `montar(elemento, opciones)` pinta la pantalla
 * pedida dentro del elemento y devuelve la función para desmontarla. Comparte la sesión y los avisos de la interfaz.
 */
import { createRoot } from "react-dom/client";
import { DocumentosApp } from "./DocumentosApp";
import type { Anfitrion, Ruta } from "./contexto";
import { ESTILOS } from "./estilos";

function estilos() {
  if (document.getElementById("dx-estilos")) return;
  const s = document.createElement("style");
  s.id = "dx-estilos";
  s.textContent = ESTILOS;
  document.head.appendChild(s);
}

export function montar(elemento: HTMLElement, anfitrion: Anfitrion, inicial: Ruta): () => void {
  estilos();
  const raiz = createRoot(elemento);
  raiz.render(<DocumentosApp anfitrion={anfitrion} inicial={inicial} />);
  return () => raiz.unmount();
}
