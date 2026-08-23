import { useCallback, useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { useToast } from "../lib/toast";
import { eur, fecha, cantidad } from "../lib/format";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";
import type { Actividad, Cliente, FacturaResumen } from "../lib/tipos";

interface LineaForm {
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
  codigoIva: string;
}
const lineaVacia: LineaForm = { descripcion: "", cantidad: 1, precioUnitario: 0, codigoIva: "IVA21" };

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
  const toast = useToast();
  const [facturas, setFacturas] = useState<FacturaResumen[] | null>(null);
  const [error, setError] = useState("");
  const [detalle, setDetalle] = useState<FacturaDetalle | null>(null);

  const [creando, setCreando] = useState(false);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [actividades, setActividades] = useState<Actividad[]>([]);
  const [clienteId, setClienteId] = useState("");
  const [actividadId, setActividadId] = useState("");
  const [lineas, setLineas] = useState<LineaForm[]>([{ ...lineaVacia }]);

  const cargar = useCallback(() => {
    api.get<FacturaResumen[]>("/facturas").then(setFacturas).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(cargar, [cargar]);

  function abrirNueva() {
    setClienteId("");
    setActividadId("");
    setLineas([{ ...lineaVacia }]);
    // Clientes y actividades que el usuario puede elegir en Ventas (según su visibilidad).
    api.get<Cliente[]>("/clientes").then(setClientes).catch(() => setClientes([]));
    api.get<Actividad[]>("/actividades/visibles?area=Ventas").then(setActividades).catch(() => setActividades([]));
    setCreando(true);
  }

  async function emitir() {
    if (!clienteId) {
      toast("Selecciona un cliente.", "err");
      return;
    }
    try {
      await api.post("/facturas", {
        clienteId,
        actividadNegocioId: actividadId || null,
        lineas: lineas
          .filter((l) => l.descripcion.trim() !== "")
          .map((l) => ({ descripcion: l.descripcion, cantidad: Number(l.cantidad) || 0, precioUnitario: Number(l.precioUnitario) || 0, codigoIva: l.codigoIva })),
      });
      setCreando(false);
      toast("Factura emitida.", "ok");
      cargar();
    } catch (e) {
      toast(e instanceof Error ? e.message : "No se pudo emitir la factura.", "err");
    }
  }

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
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 8 }}>
        <h1 style={{ margin: 0 }}>Facturas</h1>
        <button className="btn small" onClick={abrirNueva}>+ Nueva factura</button>
      </div>
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

      {creando && (
        <Modal titulo="Nueva factura" onGuardar={emitir} onCerrar={() => setCreando(false)}>
          <label htmlFor="f_cliente">Cliente</label>
          <select id="f_cliente" value={clienteId} onChange={(e) => setClienteId(e.target.value)}>
            <option value="">— Selecciona un cliente —</option>
            {clientes.map((c) => (
              <option key={c.id} value={c.id}>{c.nombre}</option>
            ))}
          </select>

          <label htmlFor="f_actividad">Actividad de negocio (opcional)</label>
          <select id="f_actividad" value={actividadId} onChange={(e) => setActividadId(e.target.value)}>
            <option value="">Según el cliente / sin actividad</option>
            {actividades.map((a) => (
              <option key={a.id} value={a.id}>{a.nombre}</option>
            ))}
          </select>
          <p className="muted" style={{ fontSize: 12, marginTop: 4 }}>Solo se muestran las actividades a las que tienes acceso.</p>

          <div style={{ marginTop: 8 }}>
            <div className="muted" style={{ fontSize: 12, marginBottom: 4 }}>Líneas</div>
            {lineas.map((l, i) => (
              <div key={i} style={{ display: "grid", gridTemplateColumns: "1fr 70px 90px 90px 28px", gap: 6, marginBottom: 6 }}>
                <input placeholder="Descripción" value={l.descripcion} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, descripcion: e.target.value } : x)))} />
                <input type="number" placeholder="Cant." value={l.cantidad} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, cantidad: Number(e.target.value) } : x)))} />
                <input type="number" placeholder="Precio" value={l.precioUnitario} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, precioUnitario: Number(e.target.value) } : x)))} />
                <select value={l.codigoIva} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, codigoIva: e.target.value } : x)))}>
                  <option value="IVA21">21%</option>
                  <option value="IVA10">10%</option>
                  <option value="IVA4">4%</option>
                  <option value="IVA0">0%</option>
                </select>
                <button className="btn small secondary" type="button" aria-label="Quitar línea" onClick={() => setLineas(lineas.length > 1 ? lineas.filter((_, j) => j !== i) : lineas)}>×</button>
              </div>
            ))}
            <button className="btn small ghost" type="button" onClick={() => setLineas([...lineas, { ...lineaVacia }])}>+ Añadir línea</button>
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
