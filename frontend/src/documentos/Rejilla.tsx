/**
 * Rejilla de líneas de un documento. Cada línea: artículo (buscador), descripción, cantidad, precio, descuento, IVA,
 * importe calculado por el servidor, margen o coste, y sus conceptos. Teclado: Intro pasa a la siguiente casilla (y en
 * la última, a una línea nueva), flechas arriba y abajo cambian de línea, F2 abre el buscador de artículos.
 */
import { useRef, useState, type KeyboardEvent } from "react";
import { BuscadorArticulo, EditorConceptos } from "./Componentes";
import type { ConceptoAplicado, ConceptoCatalogo, ConceptoSolicitado, LineaEdicion, Producto, TipoIva } from "./tipos";
import { cant, eur, nuevaClave, num2 } from "./util";

/** Lo que el servidor ha calculado para una línea. */
export interface CalculoLinea {
  precio: number;
  dto?: number;
  iva?: string;
  importe: number;
  margen?: number;
  costeUnitarioEntrada?: number;
  conceptos?: ConceptoAplicado[] | null;
}

export const lineaVacia = (): LineaEdicion => ({ clave: nuevaClave(), productoId: null, descripcion: "", cantidad: 1, precio: null, dto: 0, iva: null });

export function Rejilla(props: {
  modo: "venta" | "compra";
  lineas: LineaEdicion[];
  alCambiar: (l: LineaEdicion[]) => void;
  calculos: (CalculoLinea | undefined)[];
  ivas: TipoIva[];
  catalogo: ConceptoCatalogo[];
  sugeridos: Record<string, ConceptoSolicitado[]>;
  alElegirArticulo: (clave: string, p: Producto) => void;
  soloLectura?: boolean;
}) {
  const contenedor = useRef<HTMLDivElement>(null);
  const [abiertas, setAbiertas] = useState<Set<string>>(new Set());
  const venta = props.modo === "venta";
  const columnas = venta ? 7 : 5; // casillas editables por línea (para moverse con Intro)

  const cambiar = (clave: string, c: Partial<LineaEdicion>) => props.alCambiar(props.lineas.map((l) => (l.clave === clave ? { ...l, ...c } : l)));
  const quitar = (clave: string) => {
    const resto = props.lineas.filter((l) => l.clave !== clave);
    props.alCambiar(resto.length ? resto : [lineaVacia()]);
  };

  function enfocar(f: number, c: number) {
    const el = contenedor.current?.querySelector<HTMLElement>(`[data-f="${f}"][data-c="${c}"]`);
    el?.focus();
    if (el instanceof HTMLInputElement) el.select();
  }

  function tecla(e: KeyboardEvent<HTMLElement>) {
    const el = e.target as HTMLElement;
    const f = Number(el.dataset.f), c = Number(el.dataset.c);
    if (Number.isNaN(f) || Number.isNaN(c)) return;
    if (e.key === "Enter") {
      e.preventDefault();
      if (c < columnas - 1) return enfocar(f, c + 1);
      if (f === props.lineas.length - 1) {
        props.alCambiar([...props.lineas, lineaVacia()]);
        setTimeout(() => enfocar(f + 1, 0), 30);
      } else enfocar(f + 1, 0);
    } else if (e.key === "ArrowDown" && el.tagName !== "SELECT") {
      e.preventDefault();
      enfocar(Math.min(f + 1, props.lineas.length - 1), c);
    } else if (e.key === "ArrowUp" && el.tagName !== "SELECT") {
      e.preventDefault();
      enfocar(Math.max(f - 1, 0), c);
    }
  }

  const alternar = (clave: string) =>
    setAbiertas((s) => {
      const n = new Set(s);
      if (n.has(clave)) n.delete(clave);
      else n.add(clave);
      return n;
    });

  return (
    <div ref={contenedor} className="dx-rejilla" onKeyDown={tecla}>
      <table>
        <thead>
          <tr>
            <th style={{ width: "22%" }}>Artículo</th>
            <th>Descripción</th>
            <th className="num" style={{ width: 90 }}>Cantidad</th>
            <th className="num" style={{ width: 110 }}>Precio</th>
            {venta && <th className="num" style={{ width: 74 }}>Dto %</th>}
            {venta && <th style={{ width: 150 }}>Impuesto</th>}
            <th className="num" style={{ width: 110 }}>Importe</th>
            <th className="num" style={{ width: 100 }}>{venta ? "Margen" : "Coste entrada"}</th>
            <th style={{ width: 70 }} />
          </tr>
        </thead>
        <tbody>
          {props.lineas.map((l, f) => {
            const calc = props.calculos[f];
            const aplicados = calc?.conceptos ?? [];
            const sinStock = venta && l.controlarStock && l.stock != null && l.cantidad > l.stock;
            const margenPct = calc && calc.margen != null && calc.importe ? (calc.margen / calc.importe) * 100 : null;
            return [
              <tr key={l.clave} className={f % 2 ? "dx-par" : ""}>
                <td>
                  {props.soloLectura ? (
                    <span className="mono">{l.referencia ?? ""}</span>
                  ) : (
                    <BuscadorArticulo
                      texto={l.referencia ?? (l.productoId ? l.descripcion : "")}
                      alCambiarTexto={(t) => cambiar(l.clave, { referencia: t, ...(t === "" ? { productoId: null } : {}) })}
                      alElegir={(p) => (props.alElegirArticulo(l.clave, p), enfocar(f, 2))}
                      precioDe={venta ? undefined : (p) => p.precioCompraPorUnidadCompra ?? p.precioCompra}
                      datos={{ f, c: 0 }}
                    />
                  )}
                  {l.productoId && (
                    <div className="dx-sub">
                      {l.unidad && <span>{l.unidad}</span>}
                      {l.controlarStock && <span className={sinStock ? "dx-rojo" : ""}> · stock {cant(l.stock)}</span>}
                    </div>
                  )}
                </td>
                <td>
                  <input data-f={f} data-c={1} value={l.descripcion} placeholder={l.productoId ? "" : "Descripción (línea libre)"} disabled={props.soloLectura}
                    onChange={(e) => cambiar(l.clave, { descripcion: e.target.value })} />
                  {aplicados.length > 0 && !abiertas.has(l.clave) && (
                    <button type="button" className="dx-resumen-conc" onClick={() => alternar(l.clave)} title="Ver y cambiar los conceptos">
                      {aplicados.map((c) => `${c.importe < 0 ? "−" : "+"} ${c.codigo.toLowerCase()} ${num2(Math.abs(c.importe))}${c.efecto === "Coste" ? " (coste)" : ""}`).join(" · ")}
                    </button>
                  )}
                </td>
                <td>
                  <input data-f={f} data-c={2} className="num" type="number" step="0.001" value={l.cantidad} disabled={props.soloLectura}
                    onChange={(e) => cambiar(l.clave, { cantidad: Number(e.target.value) })} />
                </td>
                <td>
                  <input data-f={f} data-c={3} className={"num" + (l.precio == null ? " dx-auto" : "")} type="number" step="0.0001" disabled={props.soloLectura}
                    value={l.precio ?? ""} placeholder={calc ? num2(calc.precio) : ""}
                    title={l.precio == null ? "Precio de la tarifa del cliente o del artículo (escribe uno para fijarlo)" : ""}
                    onChange={(e) => cambiar(l.clave, { precio: e.target.value === "" ? null : Number(e.target.value) })} />
                </td>
                {venta && (
                  <td>
                    <input data-f={f} data-c={4} className="num" type="number" step="0.01" value={l.dto || (l.precio == null && calc?.dto ? calc.dto : 0)} disabled={props.soloLectura}
                      onChange={(e) => cambiar(l.clave, { dto: Number(e.target.value), precio: l.precio ?? calc?.precio ?? null })} />
                  </td>
                )}
                {venta && (
                  <td>
                    <select data-f={f} data-c={5} value={l.iva ?? calc?.iva ?? ""} disabled={props.soloLectura} onChange={(e) => cambiar(l.clave, { iva: e.target.value || null })}>
                      {!l.iva && !calc?.iva && <option value="">Del artículo</option>}
                      {props.ivas.map((t) => (
                        <option key={t.codigo} value={t.codigo}>{t.nombre}</option>
                      ))}
                    </select>
                  </td>
                )}
                <td className="num"><strong>{calc ? eur(calc.importe) : "—"}</strong></td>
                <td className="num">
                  {venta
                    ? calc?.margen != null && (
                        <span className={calc.margen < 0 ? "dx-rojo" : "muted"}>
                          {eur(calc.margen)}
                          {margenPct != null && <div className="dx-sub">{num2(margenPct)} %</div>}
                        </span>
                      )
                    : calc?.costeUnitarioEntrada != null && <span className="muted">{eur(calc.costeUnitarioEntrada)}/ud</span>}
                </td>
                <td className="right" style={{ whiteSpace: "nowrap" }}>
                  {!props.soloLectura && props.catalogo.length > 0 && (
                    <button type="button" className={"dx-icono" + (abiertas.has(l.clave) ? " activo" : "")} title="Conceptos de la línea" onClick={() => alternar(l.clave)} data-f={f} data-c={venta ? 6 : 4}>±</button>
                  )}
                  {!props.soloLectura && (
                    <button type="button" className="dx-icono" title="Quitar la línea" onClick={() => quitar(l.clave)}>✕</button>
                  )}
                </td>
              </tr>,
              abiertas.has(l.clave) && (
                <tr key={l.clave + "c"} className="dx-fila-conc">
                  <td colSpan={venta ? 9 : 7}>
                    <EditorConceptos catalogo={props.catalogo} lista={l.conceptos} sugeridos={props.sugeridos[l.clave]} alCambiar={(c) => cambiar(l.clave, { conceptos: c })} />
                  </td>
                </tr>
              ),
            ];
          })}
        </tbody>
      </table>
      {!props.soloLectura && (
        <button type="button" className="btn small ghost" style={{ marginTop: 8 }} onClick={() => (props.alCambiar([...props.lineas, lineaVacia()]), setTimeout(() => enfocar(props.lineas.length, 0), 30))}>
          + Añadir línea
        </button>
      )}
      {!props.soloLectura && <span className="muted dx-ayuda">Intro: siguiente casilla · ↑↓: cambiar de línea · F2: buscar artículo · el precio en gris es el de tarifa</span>}
    </div>
  );
}
