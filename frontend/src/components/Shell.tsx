import { NavLink, Outlet } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "../lib/auth";

const NAV: { a: string; etiqueta: string }[] = [
  { a: "/inicio", etiqueta: "Inicio" },
  { a: "/clientes", etiqueta: "Clientes" },
  { a: "/actividades", etiqueta: "Actividades" },
  { a: "/facturas", etiqueta: "Facturas" },
  { a: "/cartas-porte", etiqueta: "Cartas de porte" },
  { a: "/tipos-iva", etiqueta: "Tipos de IVA" },
];

function alternarTema() {
  const raiz = document.documentElement;
  const oscuro = raiz.getAttribute("data-tema") === "oscuro";
  raiz.setAttribute("data-tema", oscuro ? "claro" : "oscuro");
  try {
    localStorage.setItem("alxor.tema", oscuro ? "claro" : "oscuro");
  } catch {
    /* ignore */
  }
}

export function Shell() {
  const { usuario, empresa, salir } = useAuth();
  const [abierto, setAbierto] = useState(false);

  return (
    <div style={{ display: "grid", gridTemplateColumns: abierto ? "240px 1fr" : "72px 1fr", minHeight: "100vh", transition: "grid-template-columns .15s" }}>
      <aside style={{ background: "var(--surface)", borderRight: "1px solid var(--line)", padding: "18px 12px", position: "sticky", top: 0, height: "100vh" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 10, padding: "4px 8px 18px", cursor: "pointer" }} onClick={() => setAbierto((v) => !v)}>
          <div style={{ width: 34, height: 34, borderRadius: 10, background: "linear-gradient(120deg, var(--brand1), var(--brand2))", display: "grid", placeItems: "center", color: "#fff", fontWeight: 800, flex: "none" }}>A</div>
          {abierto && <strong style={{ color: "var(--navy)" }}>ALXOR Core</strong>}
        </div>
        <nav style={{ display: "flex", flexDirection: "column", gap: 4 }}>
          {NAV.map((n) => (
            <NavLink
              key={n.a}
              to={n.a}
              style={({ isActive }) => ({
                display: "flex",
                alignItems: "center",
                gap: 10,
                padding: "10px 12px",
                borderRadius: 10,
                fontWeight: 600,
                color: isActive ? "var(--accent)" : "var(--muted)",
                background: isActive ? "var(--accent-soft)" : "transparent",
                whiteSpace: "nowrap",
              })}
            >
              <span style={{ width: 8, height: 8, borderRadius: 999, background: "currentColor", flex: "none" }} />
              {abierto && n.etiqueta}
            </NavLink>
          ))}
        </nav>
        {abierto && (
          <a href="/" style={{ display: "block", marginTop: 18, padding: "10px 12px", fontSize: 12.5, color: "var(--muted)" }}>
            ← Interfaz clásica
          </a>
        )}
      </aside>

      <div>
        <header style={{ height: 64, borderBottom: "1px solid var(--line)", background: "var(--surface)", display: "flex", alignItems: "center", justifyContent: "space-between", padding: "0 24px", position: "sticky", top: 0, zIndex: 20 }}>
          <span className="pill" title="Vista previa de la nueva interfaz">Nueva interfaz · vista previa</span>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            <span className="pill">{empresa?.razonSocial}</span>
            <button className="btn small secondary" onClick={alternarTema} aria-label="Cambiar tema">◐</button>
            <span className="muted" style={{ fontWeight: 600 }}>{usuario?.nombre}</span>
            <button className="btn small secondary" onClick={salir}>Salir</button>
          </div>
        </header>
        <main style={{ padding: 24, maxWidth: 1100, margin: "0 auto" }}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}
