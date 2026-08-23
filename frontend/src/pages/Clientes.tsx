import { useCallback, useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { useToast } from "../lib/toast";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";
import type { Cliente } from "../lib/tipos";

interface Formulario {
  nombre: string;
  nifFiscal: string;
  email: string;
  poblacion: string;
  provincia: string;
  porcentajeIrpfDefecto: number;
}

const vacio: Formulario = { nombre: "", nifFiscal: "", email: "", poblacion: "", provincia: "", porcentajeIrpfDefecto: 0 };

export function Clientes() {
  const toast = useToast();
  const [clientes, setClientes] = useState<Cliente[] | null>(null);
  const [error, setError] = useState("");
  const [editando, setEditando] = useState<Cliente | null>(null);
  const [creando, setCreando] = useState(false);
  const [form, setForm] = useState<Formulario>(vacio);

  const cargar = useCallback(() => {
    api.get<Cliente[]>("/clientes").then(setClientes).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(cargar, [cargar]);

  function abrirNuevo() {
    setForm(vacio);
    setEditando(null);
    setCreando(true);
  }

  function abrirEdicion(c: Cliente) {
    setForm({
      nombre: c.nombre,
      nifFiscal: c.nifFiscal ?? "",
      email: c.email ?? "",
      poblacion: c.poblacion,
      provincia: c.provincia,
      porcentajeIrpfDefecto: c.porcentajeIrpfDefecto,
    });
    setEditando(c);
    setCreando(true);
  }

  async function guardar() {
    const cuerpo = {
      nombre: form.nombre,
      nifFiscal: form.nifFiscal || null,
      email: form.email || null,
      poblacion: form.poblacion || null,
      provincia: form.provincia || null,
      porcentajeIrpfDefecto: Number(form.porcentajeIrpfDefecto) || 0,
    };
    if (editando) await api.put(`/clientes/${editando.id}`, cuerpo);
    else await api.post("/clientes", cuerpo);
    setCreando(false);
    toast("Cliente guardado.", "ok");
    cargar();
  }

  const columnas: Columna<Cliente>[] = [
    { clave: "nombre", titulo: "Nombre", render: (c) => <strong>{c.nombre}</strong> },
    { clave: "nif", titulo: "NIF", render: (c) => <span className="mono muted">{c.nifFiscal ?? "—"}</span> },
    { clave: "poblacion", titulo: "Población", render: (c) => c.poblacion || "—" },
    { clave: "irpf", titulo: "IRPF", numerica: true, render: (c) => (c.porcentajeIrpfDefecto ? `${c.porcentajeIrpfDefecto}%` : "—") },
  ];

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <h1 style={{ margin: 0 }}>Clientes</h1>
        <button className="btn small" onClick={abrirNuevo}>+ Nuevo cliente</button>
      </div>

      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {clientes === null && !error && <p className="muted">Cargando…</p>}

      {clientes && (
        <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", boxShadow: "var(--shadow)", overflow: "hidden" }}>
          <DataTable columnas={columnas} filas={clientes} claveFila={(c) => c.id} onFila={abrirEdicion} vacio="Aún no hay clientes. Crea el primero." />
        </div>
      )}

      {creando && (
        <Modal titulo={editando ? "Editar cliente" : "Nuevo cliente"} onGuardar={guardar} onCerrar={() => setCreando(false)}>
          <label htmlFor="c_nombre">Nombre o razón social</label>
          <input id="c_nombre" value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} />
          <label htmlFor="c_nif">NIF (opcional)</label>
          <input id="c_nif" value={form.nifFiscal} onChange={(e) => setForm({ ...form, nifFiscal: e.target.value })} />
          <label htmlFor="c_email">Email (opcional)</label>
          <input id="c_email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div>
              <label htmlFor="c_pob">Población</label>
              <input id="c_pob" value={form.poblacion} onChange={(e) => setForm({ ...form, poblacion: e.target.value })} />
            </div>
            <div>
              <label htmlFor="c_prov">Provincia</label>
              <input id="c_prov" value={form.provincia} onChange={(e) => setForm({ ...form, provincia: e.target.value })} />
            </div>
          </div>
          <label htmlFor="c_irpf">IRPF por defecto (%)</label>
          <input id="c_irpf" type="number" value={form.porcentajeIrpfDefecto} onChange={(e) => setForm({ ...form, porcentajeIrpfDefecto: Number(e.target.value) })} />
        </Modal>
      )}
    </div>
  );
}
