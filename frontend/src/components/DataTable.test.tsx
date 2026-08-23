import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { DataTable, type Columna } from "./DataTable";

interface Fila { id: string; nombre: string; }
const columnas: Columna<Fila>[] = [{ clave: "nombre", titulo: "Nombre", render: (f) => f.nombre }];

describe("DataTable", () => {
  it("muestra el mensaje de vacío sin filas", () => {
    render(<DataTable columnas={columnas} filas={[]} claveFila={(f) => f.id} vacio="Nada aquí" />);
    expect(screen.getByText("Nada aquí")).toBeInTheDocument();
  });

  it("renderiza las filas y responde al clic", () => {
    const onFila = vi.fn();
    render(
      <DataTable
        columnas={columnas}
        filas={[{ id: "1", nombre: "Ana" }, { id: "2", nombre: "Luis" }]}
        claveFila={(f) => f.id}
        onFila={onFila}
      />,
    );
    expect(screen.getByText("Ana")).toBeInTheDocument();
    fireEvent.click(screen.getByText("Luis"));
    expect(onFila).toHaveBeenCalledWith({ id: "2", nombre: "Luis" });
  });
});
