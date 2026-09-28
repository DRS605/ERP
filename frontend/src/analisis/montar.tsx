/** Punto de entrada del análisis para la interfaz clásica: `montar(elemento, anfitrion, inicial)`; devuelve el desmontaje. */
import { createRoot } from "react-dom/client";
import { AnalisisApp, type AnfitrionAnalisis } from "./AnalisisApp";
import { ESTILOS_ANALISIS } from "./estilos";

export function montar(elemento: HTMLElement, anfitrion: AnfitrionAnalisis, inicial?: { informeId?: string; plantilla?: string }): () => void {
  if (!document.getElementById("ax-estilos")) {
    const s = document.createElement("style");
    s.id = "ax-estilos";
    s.textContent = ESTILOS_ANALISIS;
    document.head.appendChild(s);
  }
  const raiz = createRoot(elemento);
  raiz.render(<AnalisisApp anfitrion={anfitrion} inicial={inicial} />);
  return () => raiz.unmount();
}
