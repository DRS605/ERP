/** Piezas comunes del módulo de documentos: diálogo, buscadores y editor de conceptos. */
import { useEffect, useMemo, useRef, useState, type ReactNode, type KeyboardEvent } from "react";
import { useDocs } from "./contexto";
import type { ConceptoCatalogo, ConceptoSolicitado, Producto, Tercero } from "./tipos";
import { cant, dinero, eur, useRetardado, useUltimaPeticion } from "./util";

export function Dialogo(props: { titulo: string; children: ReactNode; alCerrar: () => void; acciones?: ReactNode; ancho?: number }) {
  useEffect(() => {
    const tecla = (e: globalThis.KeyboardEvent) => e.key === "Escape" && props.alCerrar();
    window.addEventListener("keydown", tecla);
    return () => window.removeEventListener("keydown", tecla);
  }, [props]);
  return (
    <div className="dx-fondo" onMouseDown={(e) => e.target === e.currentTarget && props.alCerrar()}>
      <div className="dx-dialogo" style={{ maxWidth: props.ancho ?? 560 }} role="dialog" aria-label={props.titulo}>
        <div className="dx-dialogo-cab">
          <strong>{props.titulo}</strong>
          <button className="btn small secondary" onClick={props.alCerrar} aria-label="Cerrar">✕</button>
        </div>
        <div className="dx-dialogo-cuerpo">{props.children}</div>
        {props.acciones && <div className="dx-dialogo-pie">{props.acciones}</div>}
      </div>
    </div>
  );
}

/**
 * Buscador de artículos por referencia o nombre (en el servidor). Con el teclado: flechas para moverse, Intro para
 * elegir, Escape para cerrar. Muestra precio, unidad y existencias.
 */
export function BuscadorArticulo(props: {
  texto: string;
  alCambiarTexto: (t: string) => void;
  alElegir: (p: Producto) => void;
  alTeclaFuera?: (e: KeyboardEvent<HTMLInputElement>) => void;
  precioDe?: (p: Producto) => number;
  autoFocus?: boolean;
  datos?: Record<string, string | number>;
}) {
  const { api } = useDocs();
  const [abierto, setAbierto] = useState(false);
  const [resultados, setResultados] = useState<Producto[]>([]);
  const [consulta, setConsulta] = useState<string | null>(null);
  const [activo, setActivo] = useState(0);
  const buscado = useRetardado(props.texto, 180);
  // Solo valen los resultados de lo que hay escrito ahora (no los de una búsqueda anterior).
  const vigentes = consulta === props.texto.trim() ? resultados : [];
  const ultima = useUltimaPeticion();

  useEffect(() => {
    if (!abierto) return;
    const vigente = ultima();
    const q = encodeURIComponent(buscado.trim());
    api
      .get<{ elementos: Producto[] }>(`/productos/buscar?texto=${q}&tamanoPagina=12`)
      .then((r) => vigente() && (setResultados(r.elementos ?? []), setConsulta(buscado.trim()), setActivo(0)))
      .catch(() => vigente() && (setResultados([]), setConsulta(buscado.trim())));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [buscado, abierto]);

  function tecla(e: KeyboardEvent<HTMLInputElement>) {
    if (abierto && e.key === "Enter" && props.texto.trim() && !vigentes.length) {
      // Aún buscando lo que se acaba de escribir: no se pasa de casilla con un resultado viejo.
      e.preventDefault();
      e.stopPropagation();
      return;
    }
    if (abierto && vigentes.length) {
      if (e.key === "ArrowDown") return e.preventDefault(), setActivo((a) => Math.min(a + 1, vigentes.length - 1));
      if (e.key === "ArrowUp") return e.preventDefault(), setActivo((a) => Math.max(a - 1, 0));
      if (e.key === "Enter") {
        e.preventDefault();
        e.stopPropagation();
        props.alElegir(vigentes[activo]);
        setAbierto(false);
        return;
      }
    }
    if (e.key === "Escape") return setAbierto(false);
    if (e.key === "F2") return e.preventDefault(), setAbierto(true);
    props.alTeclaFuera?.(e);
  }

  return (
    <div className="dx-buscador">
      <input
        value={props.texto}
        placeholder="Buscar artículo…"
        autoFocus={props.autoFocus}
        onChange={(e) => (props.alCambiarTexto(e.target.value), setAbierto(true))}
        onFocus={(e) => e.target.select()}
        onBlur={() => setTimeout(() => setAbierto(false), 150)}
        onKeyDown={tecla}
        {...Object.fromEntries(Object.entries(props.datos ?? {}).map(([k, v]) => [`data-${k}`, v]))}
      />
      {abierto && vigentes.length > 0 && (
        <div className="dx-lista">
          {vigentes.map((p, i) => (
            <div
              key={p.id}
              className={"dx-opcion" + (i === activo ? " activa" : "")}
              onMouseDown={(e) => (e.preventDefault(), props.alElegir(p), setAbierto(false))}
              onMouseEnter={() => setActivo(i)}
            >
              <span>
                {p.referencia && <span className="mono muted">{p.referencia} · </span>}
                <strong>{p.nombre}</strong>
                {p.familia && <span className="muted"> · {p.familia}</span>}
              </span>
              <span className="muted" style={{ whiteSpace: "nowrap" }}>
                {eur(props.precioDe ? props.precioDe(p) : p.precioUnitario)}/{p.unidad}
                {p.controlarStock && <> · stock {cant(p.stock)}</>}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

/** Selector de cliente o proveedor con búsqueda por nombre o NIF. */
export function SelectorTercero(props: { terceros: Tercero[]; valor: string; alCambiar: (id: string) => void; etiqueta: string; deshabilitado?: boolean }) {
  const [texto, setTexto] = useState("");
  const [abierto, setAbierto] = useState(false);
  const [activo, setActivo] = useState(0);
  const elegido = props.terceros.find((t) => t.id === props.valor);
  const filtrados = useMemo(() => {
    const q = texto.trim().toLowerCase();
    return props.terceros.filter((t) => t.activo !== false && (!q || t.nombre.toLowerCase().includes(q) || (t.nifFiscal ?? "").toLowerCase().includes(q))).slice(0, 30);
  }, [texto, props.terceros]);
  const ref = useRef<HTMLInputElement>(null);

  function elegir(t: Tercero) {
    props.alCambiar(t.id);
    setTexto("");
    setAbierto(false);
  }

  return (
    <div>
      <label>{props.etiqueta}</label>
      <div className="dx-buscador">
        <input
          ref={ref}
          disabled={props.deshabilitado}
          value={abierto ? texto : elegido ? `${elegido.nombre}${elegido.nifFiscal ? " · " + elegido.nifFiscal : ""}` : texto}
          placeholder={`Buscar ${props.etiqueta.toLowerCase()} por nombre o NIF…`}
          onFocus={() => (setAbierto(true), setTexto(""))}
          onBlur={() => setTimeout(() => setAbierto(false), 150)}
          onChange={(e) => (setTexto(e.target.value), setActivo(0))}
          onKeyDown={(e) => {
            if (e.key === "ArrowDown") return e.preventDefault(), setActivo((a) => Math.min(a + 1, filtrados.length - 1));
            if (e.key === "ArrowUp") return e.preventDefault(), setActivo((a) => Math.max(a - 1, 0));
            if (e.key === "Enter" && filtrados[activo]) return e.preventDefault(), elegir(filtrados[activo]);
            if (e.key === "Escape") return setAbierto(false);
          }}
        />
        {abierto && filtrados.length > 0 && (
          <div className="dx-lista">
            {filtrados.map((t, i) => (
              <div key={t.id} className={"dx-opcion" + (i === activo ? " activa" : "")} onMouseDown={(e) => (e.preventDefault(), elegir(t))} onMouseEnter={() => setActivo(i)}>
                <strong>{t.nombre}</strong>
                <span className="muted">{[t.nifFiscal, t.poblacion].filter(Boolean).join(" · ")}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

const CALCULO: Record<ConceptoCatalogo["calculo"], string> = { Porcentaje: "%", PorUnidad: "€/ud", PorKilo: "€/kg", Importe: "€" };

/**
 * Conceptos de una línea o del documento. En una línea, `lista` undefined = los que se pongan solos (se enseñan los
 * sugeridos); al tocarlos pasan a ser los de la lista.
 */
export function EditorConceptos(props: {
  catalogo: ConceptoCatalogo[];
  lista: ConceptoSolicitado[] | undefined;
  sugeridos?: ConceptoSolicitado[];
  alCambiar: (l: ConceptoSolicitado[] | undefined) => void;
  documento?: boolean;
}) {
  if (!props.catalogo.length) return null;
  const auto = props.lista === undefined;
  const lista = props.lista ?? props.sugeridos ?? [];
  const cambiar = (i: number, c: Partial<ConceptoSolicitado>) => props.alCambiar(lista.map((x, j) => (j === i ? { ...x, ...c } : x)));
  return (
    <div className="dx-conceptos">
      <span className="muted">{props.documento ? "Conceptos del documento" : auto ? "Conceptos automáticos" : "Conceptos"}:</span>
      {lista.map((c, i) => {
        const d = props.catalogo.find((x) => x.id === c.conceptoId);
        return (
          <span key={i} className={"dx-chip" + (d?.efecto === "Coste" ? " coste" : "") + (auto ? " auto" : "")}>
            <select value={c.conceptoId} onChange={(e) => cambiar(i, { conceptoId: e.target.value, valor: null })}>
              {props.catalogo.map((x) => (
                <option key={x.id} value={x.id}>
                  {x.codigo} {x.sentido === "Resta" ? "−" : "+"}
                  {CALCULO[x.calculo]}
                  {x.efecto === "Coste" ? " · coste" : ""}
                </option>
              ))}
            </select>
            <input type="number" step="0.0001" value={c.valor ?? ""} placeholder={String(d?.valor ?? "")} onChange={(e) => cambiar(i, { valor: e.target.value === "" ? null : Number(e.target.value) })} />
            <button type="button" title="Quitar" onClick={() => props.alCambiar(lista.filter((_, j) => j !== i))}>✕</button>
          </span>
        );
      })}
      <button type="button" className="dx-enlace" onClick={() => props.alCambiar([...lista, { conceptoId: props.catalogo[0].id, valor: null }])}>+ concepto</button>
      {!props.documento && !auto && (
        <button type="button" className="dx-enlace" onClick={() => props.alCambiar(undefined)}>volver a los automáticos</button>
      )}
      {!props.documento && auto && lista.length === 0 && <span className="muted">ninguno</span>}
    </div>
  );
}

/** Línea de detalle de un concepto aplicado (en las vistas). */
/** Conceptos aplicados a una línea. En un documento en divisa, sus importes en la divisa (en la factura, el de la divisa y no su contravalor). */
export function ConceptosAplicados(props: {
  conceptos?: { nombre: string; calculo: string; valor: number; importe: number; importeDivisa?: number | null; efecto: string; repartido: boolean }[] | null;
  moneda?: string | null;
}) {
  if (!props.conceptos?.length) return null;
  return (
    <div className="dx-aplicados">
      {props.conceptos.map((c, i) => (
        <div key={i}>
          · {c.nombre}
          {c.calculo === "Porcentaje" ? ` (${cant(c.valor)} %)` : ""}
          {c.repartido ? " · del documento" : ""}
          {c.efecto === "Coste" ? " · coste" : ""}: <strong>{dinero(c.importeDivisa ?? c.importe, props.moneda)}</strong>
        </div>
      ))}
    </div>
  );
}
