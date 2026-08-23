import type { ReactNode } from "react";

export interface Columna<T> {
  clave: string;
  titulo: string;
  render: (fila: T) => ReactNode;
  numerica?: boolean;
}

interface Props<T> {
  columnas: Columna<T>[];
  filas: T[];
  claveFila: (fila: T) => string;
  vacio?: string;
  onFila?: (fila: T) => void;
}

/** Tabla de datos reutilizable, con estado vacío y filas clicables opcionales. */
export function DataTable<T>({ columnas, filas, claveFila, vacio = "Sin datos.", onFila }: Props<T>) {
  return (
    <div style={{ overflowX: "auto" }}>
      <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 13.5 }}>
        <thead>
          <tr>
            {columnas.map((c) => (
              <th
                key={c.clave}
                className={c.numerica ? "num" : undefined}
                style={{ textAlign: c.numerica ? "right" : "left", padding: "10px 12px", borderBottom: "1px solid var(--line)", color: "var(--muted)", fontWeight: 600, fontSize: 12 }}
              >
                {c.titulo}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filas.length === 0 ? (
            <tr>
              <td colSpan={columnas.length} style={{ padding: "18px 12px", color: "var(--muted)" }}>{vacio}</td>
            </tr>
          ) : (
            filas.map((fila) => (
              <tr
                key={claveFila(fila)}
                onClick={onFila ? () => onFila(fila) : undefined}
                style={{ cursor: onFila ? "pointer" : "default", borderBottom: "1px solid var(--line)" }}
              >
                {columnas.map((c) => (
                  <td key={c.clave} className={c.numerica ? "num" : undefined} style={{ padding: "10px 12px" }}>
                    {c.render(fila)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
