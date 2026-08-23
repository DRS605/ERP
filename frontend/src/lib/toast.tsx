import { createContext, useCallback, useContext, useRef, useState, type ReactNode } from "react";

type Tipo = "ok" | "err" | "";
interface Aviso { id: number; texto: string; tipo: Tipo; }

const ContextoToast = createContext<(texto: string, tipo?: Tipo) => void>(() => {});

export function ProveedorToast({ children }: { children: ReactNode }) {
  const [avisos, setAvisos] = useState<Aviso[]>([]);
  const contador = useRef(0);

  const toast = useCallback((texto: string, tipo: Tipo = "") => {
    const id = ++contador.current;
    setAvisos((a) => [...a, { id, texto, tipo }]);
    setTimeout(() => setAvisos((a) => a.filter((x) => x.id !== id)), 2800);
  }, []);

  return (
    <ContextoToast.Provider value={toast}>
      {children}
      <div style={{ position: "fixed", bottom: 20, right: 20, display: "flex", flexDirection: "column", gap: 8, zIndex: 1000 }}>
        {avisos.map((a) => (
          <div
            key={a.id}
            role="status"
            style={{
              background: "var(--surface)",
              border: "1px solid var(--line)",
              borderLeft: `4px solid ${a.tipo === "err" ? "var(--neg)" : a.tipo === "ok" ? "var(--pos)" : "var(--accent)"}`,
              borderRadius: 12,
              padding: "12px 16px",
              boxShadow: "var(--shadow)",
              maxWidth: 360,
            }}
          >
            {a.texto}
          </div>
        ))}
      </div>
    </ContextoToast.Provider>
  );
}

export function useToast() {
  return useContext(ContextoToast);
}
