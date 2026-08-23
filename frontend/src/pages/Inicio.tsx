import { useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { eur } from "../lib/format";
import { useAuth } from "../lib/auth";
import type { FacturaResumen } from "../lib/tipos";

function Kpi({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", padding: 18, boxShadow: "var(--shadow)" }}>
      <div style={{ fontSize: 24, fontWeight: 750, color: "var(--navy)" }}>{valor}</div>
      <div className="muted" style={{ fontSize: 12.5, marginTop: 6 }}>{etiqueta}</div>
    </div>
  );
}

export function Inicio() {
  const { usuario } = useAuth();
  const [facturas, setFacturas] = useState<FacturaResumen[] | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    api.get<FacturaResumen[]>("/facturas").then(setFacturas).catch((e: Error) => setError(e.message));
  }, []);

  const total = (facturas ?? []).reduce((s, f) => s + f.total, 0);
  const nombre = usuario?.nombre?.split(" ")[0] ?? "";

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Hola{nombre ? `, ${nombre}` : ""} 👋</h1>
      <p className="muted" style={{ marginTop: -6 }}>Resumen de tu actividad de facturación.</p>

      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {facturas === null && !error && <p className="muted">Cargando…</p>}

      {facturas && (
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))", gap: 14, marginTop: 18 }}>
          <Kpi etiqueta="Facturas emitidas" valor={String(facturas.length)} />
          <Kpi etiqueta="Total facturado" valor={eur(total)} />
          <Kpi etiqueta="Última factura" valor={facturas[0]?.numeroCompleto ?? "—"} />
        </div>
      )}
    </div>
  );
}
