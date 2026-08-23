import { useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { eur, fecha, cantidad } from "../lib/format";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";
import type { FacturaResumen } from "../lib/tipos";

interface LineaDetalle {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  porcentajeIva: number;
  base: number;
}
interface FacturaDetalle extends FacturaResumen {
  porcentajeIrpf: number;
  recargoTotal: number;
  lineas: LineaDetalle[];
}

function estadoPill(estado: string) {
  const tipo = estado === "Emitida" ? "ok" : estado === "Anulada" ? "err" : "warn";
  return <span className={`pill ${tipo}`}>{estado}</span>;
}

export function Facturas() {
  const [facturas, setFacturas] = useState<FacturaResumen[] | null>(null);
  const [error, setError] = useState("");
  const [detalle, setDetalle] = useState<FacturaDetalle | null>(null);

  useEffect(() => {
    api.get<FacturaResumen[]>("/facturas").then(setFacturas).catch((e: Error) => setError(e.message));
  }, []);

  async function abrir(f: FacturaResumen) {
    try {
      setDetalle(await api.get<FacturaDetalle>(`/facturas/${f.id}`));
    } catch (e) {
      setError(e instanceof Error ? e.message : "No se pudo abrir la factura.");
    }
  }

  const columnas: Columna<FacturaResumen>[] = [
    { clave: "num", titulo: "Número", render: (f) => <strong className="mono">{f.numeroCompleto}</strong> },
    { clave: "cliente", titulo: "Cliente", render: (f) => f.clienteNombre },
    { clave: "fecha", titulo: "Emisión", render: (f) => fecha(f.fechaEmision) },
    { clave: "estado", titulo: "Estado", render: (f) => estadoPill(f.estado) },
    { clave: "total", titulo: "Total", numerica: true, render: (f) => <strong>{eur(f.total)}</strong> },
  ];

  return (
    <div>
      <h1 style={{ marginTop: 0 }}>Facturas</h1>
      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {facturas === null && !error && <p className="muted">Cargando…</p>}

      {facturas && (
        <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", boxShadow: "var(--shadow)", overflow: "hidden" }}>
          <DataTable columnas={columnas} filas={facturas} claveFila={(f) => f.id} onFila={abrir} vacio="Aún no hay facturas." />
        </div>
      )}

      {detalle && (
        <Modal titulo={`Factura ${detalle.numeroCompleto}`} onCerrar={() => setDetalle(null)}>
          <div style={{ display: "flex", justifyContent: "space-between", flexWrap: "wrap", gap: 8 }}>
            <div>
              <div className="muted" style={{ fontSize: 12 }}>Cliente</div>
              <strong>{detalle.clienteNombre}</strong>
              {detalle.clienteNif && <div className="mono muted">{detalle.clienteNif}</div>}
            </div>
            <div style={{ textAlign: "right" }}>
              <div className="muted" style={{ fontSize: 12 }}>Emisión</div>
              <strong>{fecha(detalle.fechaEmision)}</strong>
              <div style={{ marginTop: 4 }}>{estadoPill(detalle.estado)}</div>
            </div>
          </div>

          <table style={{ width: "100%", borderCollapse: "collapse", marginTop: 16, fontSize: 13 }}>
            <thead>
              <tr>
                <th style={{ textAlign: "left", padding: "8px 6px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontSize: 12 }}>Descripción</th>
                <th className="num" style={{ padding: "8px 6px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontSize: 12 }}>Cant.</th>
                <th className="num" style={{ padding: "8px 6px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontSize: 12 }}>Precio</th>
                <th className="num" style={{ padding: "8px 6px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontSize: 12 }}>IVA</th>
                <th className="num" style={{ padding: "8px 6px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontSize: 12 }}>Base</th>
              </tr>
            </thead>
            <tbody>
              {detalle.lineas.map((l, i) => (
                <tr key={i} style={{ borderBottom: "1px solid var(--line)" }}>
                  <td style={{ padding: "8px 6px" }}>{l.descripcion}</td>
                  <td className="num" style={{ padding: "8px 6px" }}>{cantidad(l.cantidad)}</td>
                  <td className="num" style={{ padding: "8px 6px" }}>{eur(l.precioUnitario)}</td>
                  <td className="num" style={{ padding: "8px 6px" }}>{l.porcentajeIva}%</td>
                  <td className="num" style={{ padding: "8px 6px" }}>{eur(l.base)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <div style={{ maxWidth: 260, marginLeft: "auto", marginTop: 14 }}>
            <Tot etiqueta="Base imponible" valor={eur(detalle.baseImponible)} />
            <Tot etiqueta="IVA" valor={eur(detalle.cuotaIva)} />
            {detalle.retencionIrpf > 0 && <Tot etiqueta={`Retención IRPF (${detalle.porcentajeIrpf}%)`} valor={`−${eur(detalle.retencionIrpf)}`} />}
            <Tot etiqueta="Total" valor={eur(detalle.total)} fuerte />
          </div>

          <div style={{ display: "flex", gap: 8, marginTop: 18, flexWrap: "wrap" }}>
            <a className="btn small ghost" href={`/facturas/${detalle.id}/pdf`} target="_blank" rel="noreferrer">Ver PDF</a>
            {detalle.tipo !== "Simplificada" && (detalle.clienteNif || detalle.clienteNombre) && (
              <a className="btn small ghost" href={`/facturas/${detalle.id}/facturae.xml`} target="_blank" rel="noreferrer">Facturae (XML)</a>
            )}
          </div>
        </Modal>
      )}
    </div>
  );
}

function Tot({ etiqueta, valor, fuerte }: { etiqueta: string; valor: string; fuerte?: boolean }) {
  return (
    <div style={{ display: "flex", justifyContent: "space-between", padding: "6px 0", borderTop: fuerte ? "1px solid var(--line)" : undefined, fontWeight: fuerte ? 750 : 400 }}>
      <span className={fuerte ? undefined : "muted"}>{etiqueta}</span>
      <span>{valor}</span>
    </div>
  );
}
