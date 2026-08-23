import { useCallback, useEffect, useState } from "react";
import { api } from "../lib/cliente";
import { useToast } from "../lib/toast";
import { DataTable, type Columna } from "../components/DataTable";
import { Modal } from "../components/Modal";
import type { Actividad } from "../lib/tipos";

interface Formulario {
  nombre: string;
  activa: boolean;
}

export function Actividades() {
  const toast = useToast();
  const [actividades, setActividades] = useState<Actividad[] | null>(null);
  const [error, setError] = useState("");
  const [editando, setEditando] = useState<Actividad | null>(null);
  const [creando, setCreando] = useState(false);
  const [form, setForm] = useState<Formulario>({ nombre: "", activa: true });

  const cargar = useCallback(() => {
    api.get<Actividad[]>("/actividades").then(setActividades).catch((e: Error) => setError(e.message));
  }, []);

  useEffect(cargar, [cargar]);

  function abrirNueva() {
    setForm({ nombre: "", activa: true });
    setEditando(null);
    setCreando(true);
  }

  function abrirEdicion(a: Actividad) {
    setForm({ nombre: a.nombre, activa: a.activa });
    setEditando(a);
    setCreando(true);
  }

  async function guardar() {
    if (editando) await api.put(`/actividades/${editando.id}`, { nombre: form.nombre, activa: form.activa });
    else await api.post("/actividades", { nombre: form.nombre });
    setCreando(false);
    toast("Actividad guardada.", "ok");
    cargar();
  }

  const columnas: Columna<Actividad>[] = [
    { clave: "nombre", titulo: "Actividad de negocio", render: (a) => <strong>{a.nombre}</strong> },
    {
      clave: "estado",
      titulo: "Estado",
      render: (a) => (a.activa ? <span className="pill">Activa</span> : <span className="muted">Inactiva</span>),
    },
  ];

  return (
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
        <h1 style={{ margin: 0 }}>Actividades de negocio</h1>
        <button className="btn small" onClick={abrirNueva}>+ Nueva actividad</button>
      </div>

      <p className="muted" style={{ marginTop: 0 }}>
        Las actividades clasifican los datos maestros (clientes, proveedores…) y son compartidas por todo el grupo.
        Con ellas se controla qué ve cada usuario en cada pantalla.
      </p>

      {error && <p style={{ color: "var(--neg)" }}>{error}</p>}
      {actividades === null && !error && <p className="muted">Cargando…</p>}

      {actividades && (
        <div style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", boxShadow: "var(--shadow)", overflow: "hidden" }}>
          <DataTable columnas={columnas} filas={actividades} claveFila={(a) => a.id} onFila={abrirEdicion} vacio="Aún no hay actividades. Crea la primera." />
        </div>
      )}

      {creando && (
        <Modal titulo={editando ? "Editar actividad" : "Nueva actividad"} onGuardar={guardar} onCerrar={() => setCreando(false)}>
          <label htmlFor="a_nombre">Nombre</label>
          <input id="a_nombre" value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} />
          {editando && (
            <label style={{ display: "flex", alignItems: "center", gap: 8, marginTop: 12 }}>
              <input type="checkbox" checked={form.activa} onChange={(e) => setForm({ ...form, activa: e.target.checked })} />
              Activa
            </label>
          )}
        </Modal>
      )}
    </div>
  );
}
