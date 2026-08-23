import { useCallback, useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { useToast } from "../lib/toast";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";

type Clase = "Ordinario" | "Exento" | "NoSujeto" | "InversionSujetoPasivo" | "Importacion" | "Intracomunitario";

const CLASES: { valor: Clase; etiqueta: string }[] = [
  { valor: "Ordinario", etiqueta: "Ordinario (repercute IVA)" },
  { valor: "Exento", etiqueta: "Exento (art. 20)" },
  { valor: "NoSujeto", etiqueta: "No sujeto (art. 7)" },
  { valor: "InversionSujetoPasivo", etiqueta: "Inversión del sujeto pasivo (art. 84)" },
  { valor: "Importacion", etiqueta: "Importación" },
  { valor: "Intracomunitario", etiqueta: "Intracomunitario (art. 25)" },
];

interface TipoIva {
  id: string;
  codigo: string;
  nombre: string;
  porcentaje: number;
  recargoEquivalencia: number;
  clase: Clase;
  mencionFactura?: string | null;
  activo: boolean;
  repercute: boolean;
}

interface Form {
  codigo: string;
  nombre: string;
  porcentaje: number;
  recargoEquivalencia: number;
  clase: Clase;
  mencionFactura: string;
  activo: boolean;
}
const vacio: Form = { codigo: "", nombre: "", porcentaje: 0, recargoEquivalencia: 0, clase: "Ordinario", mencionFactura: "", activo: true };

function repercute(clase: Clase) {
  return clase === "Ordinario" || clase === "Importacion";
}

export function TiposIva() {
  const toast = useToast();
  const [tipos, setTipos] = useState<TipoIva[] | null>(null);
  const [error, setError] = useState("");
  const [editando, setEditando] = useState<TipoIva | null>(null);
  const [creando, setCreando] = useState(false);
  const [form, setForm] = useState<Form>(vacio);

  const cargar = useCallback(() => {
    api.get<TipoIva[]>("/tipos-iva").then(setTipos).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(cargar, [cargar]);

  function abrirNuevo() {
    setForm(vacio);
    setEditando(null);
    setCreando(true);
  }

  function abrirEdicion(t: TipoIva) {
    setForm({
      codigo: t.codigo,
      nombre: t.nombre,
      porcentaje: t.porcentaje,
      recargoEquivalencia: t.recargoEquivalencia,
      clase: t.clase,
      mencionFactura: t.mencionFactura ?? "",
      activo: t.activo,
    });
    setEditando(t);
    setCreando(true);
  }

  async function guardar() {
    // Las clases sin repercusión no llevan porcentaje.
    const porcentaje = repercute(form.clase) ? Number(form.porcentaje) || 0 : 0;
    const recargo = repercute(form.clase) ? Number(form.recargoEquivalencia) || 0 : 0;
    const cuerpo = {
      codigo: form.codigo,
      nombre: form.nombre,
      porcentaje,
      recargoEquivalencia: recargo,
      clase: form.clase,
      mencionFactura: form.mencionFactura || null,
      activo: form.activo,
    };
    try {
      if (editando) await api.put(`/tipos-iva/${editando.id}`, cuerpo);
      else await api.post("/tipos-iva", cuerpo);
      setCreando(false);
      toast("Tipo de IVA guardado.", "ok");
      cargar();
    } catch (e) {
      toast(e instanceof Error ? e.message : "No se pudo guardar.", "err");
    }
  }

  const columnas: Columna<TipoIva>[] = [
    { clave: "codigo", titulo: "Código", render: (t) => <strong className="mono">{t.codigo}</strong> },
    { clave: "nombre", titulo: "Nombre", render: (t) => t.nombre },
    { clave: "pct", titulo: "%", numerica: true, render: (t) => (t.repercute ? `${t.porcentaje}%` : "—") },
    { clave: "clase", titulo: "Clase", render: (t) => CLASES.find((c) => c.valor === t.clase)?.etiqueta ?? t.clase },
    { clave: "estado", titulo: "Estado", render: (t) => (t.activo ? <span className="pill">Activo</span> : <span className="muted">Inactivo</span>) },
  ];

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 12 }}>
        <h1 style={{ margin: 0 }}>Tipos de IVA</h1>
        <button className="btn small" onClick={abrirNuevo}>+ Nuevo tipo</button>
      </div>
      <p className="muted" style={{ marginTop: 0 }}>Configura los tipos de IVA de la empresa y sus casuísticas (exención, no sujeto, inversión del sujeto pasivo, importación, intracomunitario).</p>

      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {tipos === null && !error && <p className="muted">Cargando…</p>}

      {tipos && (
        <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", boxShadow: "var(--shadow)", overflow: "hidden" }}>
          <DataTable columnas={columnas} filas={tipos} claveFila={(t) => t.id} onFila={abrirEdicion} vacio="Sin tipos de IVA." />
        </div>
      )}

      {creando && (
        <Modal titulo={editando ? `Editar ${editando.codigo}` : "Nuevo tipo de IVA"} onGuardar={guardar} onCerrar={() => setCreando(false)}>
          <label htmlFor="t_codigo">Código</label>
          <input id="t_codigo" value={form.codigo} disabled={!!editando} onChange={(e) => setForm({ ...form, codigo: e.target.value })} />
          <label htmlFor="t_nombre">Nombre</label>
          <input id="t_nombre" value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} />
          <label htmlFor="t_clase">Clase</label>
          <select id="t_clase" value={form.clase} onChange={(e) => setForm({ ...form, clase: e.target.value as Clase })}>
            {CLASES.map((c) => (
              <option key={c.valor} value={c.valor}>{c.etiqueta}</option>
            ))}
          </select>
          {repercute(form.clase) && (
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <div>
                <label htmlFor="t_pct">Porcentaje (%)</label>
                <input id="t_pct" type="number" value={form.porcentaje} onChange={(e) => setForm({ ...form, porcentaje: Number(e.target.value) })} />
              </div>
              <div>
                <label htmlFor="t_rec">Recargo equiv. (%)</label>
                <input id="t_rec" type="number" value={form.recargoEquivalencia} onChange={(e) => setForm({ ...form, recargoEquivalencia: Number(e.target.value) })} />
              </div>
            </div>
          )}
          {!repercute(form.clase) && (
            <>
              <label htmlFor="t_mencion">Mención legal en factura</label>
              <input id="t_mencion" value={form.mencionFactura} onChange={(e) => setForm({ ...form, mencionFactura: e.target.value })} placeholder="p. ej. Inversión del sujeto pasivo (art. 84…)" />
            </>
          )}
          {editando && (
            <label style={{ display: "flex", alignItems: "center", gap: 8, marginTop: 12 }}>
              <input type="checkbox" checked={form.activo} onChange={(e) => setForm({ ...form, activo: e.target.checked })} />
              Activo
            </label>
          )}
        </Modal>
      )}
    </div>
  );
}
