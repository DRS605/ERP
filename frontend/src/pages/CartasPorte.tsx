import { useCallback, useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { useToast } from "../lib/toast";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";
import type { Cliente } from "../lib/tipos";

interface CartaResumen {
  id: string;
  numeroCompleto: string;
  fechaExpedicion: string;
  destinatarioNombre: string;
  lugarDestino: string;
  totalBultos: number;
  totalPesoKg: number;
}

interface LineaForm {
  descripcion: string;
  bultos: number;
  pesoKg: number;
}
const lineaVacia: LineaForm = { descripcion: "", bultos: 1, pesoKg: 0 };

export function CartasPorte() {
  const toast = useToast();
  const [cartas, setCartas] = useState<CartaResumen[] | null>(null);
  const [error, setError] = useState("");
  const [creando, setCreando] = useState(false);
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [clienteId, setClienteId] = useState("");
  const [transportista, setTransportista] = useState("");
  const [matricula, setMatricula] = useState("");
  const [origen, setOrigen] = useState("");
  const [destino, setDestino] = useState("");
  const [lineas, setLineas] = useState<LineaForm[]>([{ ...lineaVacia }]);

  const cargar = useCallback(() => {
    api.get<CartaResumen[]>("/cartas-porte").then(setCartas).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(cargar, [cargar]);

  function abrirNueva() {
    setClienteId("");
    setTransportista("");
    setMatricula("");
    setOrigen("");
    setDestino("");
    setLineas([{ ...lineaVacia }]);
    api.get<Cliente[]>("/clientes").then(setClientes).catch(() => setClientes([]));
    setCreando(true);
  }

  async function guardar() {
    const mercancias = lineas.filter((l) => l.descripcion.trim() !== "");
    if (mercancias.length === 0) {
      toast("Añade al menos una mercancía.", "err");
      return;
    }
    try {
      await api.post("/cartas-porte", {
        destinatarioClienteId: clienteId || null,
        transportistaNombre: transportista || null,
        matricula: matricula || null,
        lugarOrigen: origen || null,
        lugarDestino: destino || null,
        lineas: mercancias.map((l) => ({ descripcion: l.descripcion, bultos: Number(l.bultos) || 0, pesoKg: Number(l.pesoKg) || 0 })),
      });
      setCreando(false);
      toast("Carta de porte creada.", "ok");
      cargar();
    } catch (e) {
      toast(e instanceof Error ? e.message : "No se pudo crear la carta de porte.", "err");
    }
  }

  const columnas: Columna<CartaResumen>[] = [
    { clave: "num", titulo: "Número", render: (c) => <strong className="mono">{c.numeroCompleto}</strong> },
    { clave: "dest", titulo: "Destinatario", render: (c) => c.destinatarioNombre },
    { clave: "destino", titulo: "Destino", render: (c) => c.lugarDestino || "—" },
    { clave: "bultos", titulo: "Bultos", numerica: true, render: (c) => c.totalBultos },
    { clave: "peso", titulo: "Peso (kg)", numerica: true, render: (c) => c.totalPesoKg },
    { clave: "pdf", titulo: "", render: (c) => <a className="btn small ghost" href={`/cartas-porte/${c.id}/pdf`} target="_blank" rel="noreferrer" onClick={(e) => e.stopPropagation()}>PDF</a> },
  ];

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
        <h1 style={{ margin: 0 }}>Cartas de porte</h1>
        <button className="btn small" onClick={abrirNueva}>+ Nueva carta de porte</button>
      </div>
      <p className="muted" style={{ marginTop: 0 }}>Documento de control del transporte de mercancías por carretera.</p>

      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {cartas === null && !error && <p className="muted">Cargando…</p>}

      {cartas && (
        <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", boxShadow: "var(--shadow)", overflow: "hidden" }}>
          <DataTable columnas={columnas} filas={cartas} claveFila={(c) => c.id} vacio="Aún no hay cartas de porte." />
        </div>
      )}

      {creando && (
        <Modal titulo="Nueva carta de porte" onGuardar={guardar} onCerrar={() => setCreando(false)}>
          <label htmlFor="cp_cliente">Destinatario (cliente)</label>
          <select id="cp_cliente" value={clienteId} onChange={(e) => setClienteId(e.target.value)}>
            <option value="">— Selecciona un cliente —</option>
            {clientes.map((c) => (
              <option key={c.id} value={c.id}>{c.nombre}</option>
            ))}
          </select>

          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div>
              <label htmlFor="cp_transp">Transportista</label>
              <input id="cp_transp" value={transportista} onChange={(e) => setTransportista(e.target.value)} />
            </div>
            <div>
              <label htmlFor="cp_matricula">Matrícula</label>
              <input id="cp_matricula" value={matricula} onChange={(e) => setMatricula(e.target.value)} />
            </div>
            <div>
              <label htmlFor="cp_origen">Lugar de origen</label>
              <input id="cp_origen" value={origen} onChange={(e) => setOrigen(e.target.value)} placeholder="Por defecto, la empresa" />
            </div>
            <div>
              <label htmlFor="cp_destino">Lugar de destino</label>
              <input id="cp_destino" value={destino} onChange={(e) => setDestino(e.target.value)} placeholder="Por defecto, el cliente" />
            </div>
          </div>

          <div style={{ marginTop: 8 }}>
            <div className="muted" style={{ fontSize: 12, marginBottom: 4 }}>Mercancías</div>
            {lineas.map((l, i) => (
              <div key={i} style={{ display: "grid", gridTemplateColumns: "1fr 70px 90px 28px", gap: 6, marginBottom: 6 }}>
                <input placeholder="Descripción" value={l.descripcion} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, descripcion: e.target.value } : x)))} />
                <input type="number" placeholder="Bultos" value={l.bultos} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, bultos: Number(e.target.value) } : x)))} />
                <input type="number" placeholder="Peso kg" value={l.pesoKg} onChange={(e) => setLineas(lineas.map((x, j) => (j === i ? { ...x, pesoKg: Number(e.target.value) } : x)))} />
                <button className="btn small secondary" type="button" aria-label="Quitar" onClick={() => setLineas(lineas.length > 1 ? lineas.filter((_, j) => j !== i) : lineas)}>×</button>
              </div>
            ))}
            <button className="btn small ghost" type="button" onClick={() => setLineas([...lineas, { ...lineaVacia }])}>+ Añadir mercancía</button>
          </div>
        </Modal>
      )}
    </div>
  );
}
