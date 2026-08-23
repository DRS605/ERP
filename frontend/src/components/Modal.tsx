import { useState, type ReactNode } from "react";

interface Props {
  titulo: string;
  children: ReactNode;
  onGuardar?: () => Promise<void> | void;
  onCerrar: () => void;
  textoGuardar?: string;
}

/** Diálogo modal reutilizable con botón de guardar que muestra el error si la acción falla. */
export function Modal({ titulo, children, onGuardar, onCerrar, textoGuardar = "Guardar" }: Props) {
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function guardar() {
    if (!onGuardar) return;
    setGuardando(true);
    setError(null);
    try {
      await onGuardar();
    } catch (e) {
      setError(e instanceof Error ? e.message : "No se pudo guardar.");
      setGuardando(false);
    }
  }

  return (
    <div
      onClick={onCerrar}
      style={{ position: "fixed", inset: 0, background: "rgba(11,37,64,.45)", display: "grid", placeItems: "center", zIndex: 500, padding: 16 }}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        role="dialog"
        aria-label={titulo}
        style={{ background: "var(--surface)", border: "1px solid var(--line)", borderRadius: "var(--radius)", width: "min(520px, 100%)", maxHeight: "90vh", overflow: "auto", boxShadow: "var(--shadow)" }}
      >
        <div style={{ padding: "16px 20px", borderBottom: "1px solid var(--line)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
          <h2 style={{ margin: 0, fontSize: 17 }}>{titulo}</h2>
          <button className="btn small secondary" onClick={onCerrar} aria-label="Cerrar">✕</button>
        </div>
        <div style={{ padding: 20 }}>
          {children}
          {error && <p style={{ color: "var(--neg)", fontSize: 12.5, marginTop: 12 }}>{error}</p>}
        </div>
        {onGuardar && (
          <div style={{ padding: "0 20px 20px", display: "flex", justifyContent: "flex-end", gap: 8 }}>
            <button className="btn secondary small" onClick={onCerrar}>Cancelar</button>
            <button className="btn small" onClick={guardar} disabled={guardando}>{guardando ? "Guardando…" : textoGuardar}</button>
          </div>
        )}
      </div>
    </div>
  );
}
