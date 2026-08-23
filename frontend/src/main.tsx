import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { App } from "./App";
import { ProveedorAuth } from "./lib/auth";
import { ProveedorToast } from "./lib/toast";
import "./styles/tokens.css";

// Restaura el tema elegido por la persona (si lo hubiera).
try {
  const tema = localStorage.getItem("alxor.tema");
  if (tema) document.documentElement.setAttribute("data-tema", tema);
} catch {
  /* almacenamiento no disponible */
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <BrowserRouter basename="/app">
      <ProveedorToast>
        <ProveedorAuth>
          <App />
        </ProveedorAuth>
      </ProveedorToast>
    </BrowserRouter>
  </StrictMode>,
);
