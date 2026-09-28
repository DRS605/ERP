/**
 * Listados de documentos: búsqueda, filtros (estado, fechas, cliente/proveedor, serie, importe, estado de cobro),
 * columnas de importes (base, impuestos, total, pendiente), orden por columna, totales y exportación a Excel/CSV.
 * Facturas y gastos se filtran, ordenan y totalizan en el servidor (los totales son de todo el filtro, no de la
 * página); presupuestos y pedidos llegan enteros y se filtran aquí.
 */
import { useEffect, useMemo, useState } from "react";
import { useDocs, type TipoDocumento } from "./contexto";
import { exportarCsv, exportarXlsx, type PeticionExportacion } from "./exportar";
import type { FacturaResumen, Gasto, PaginaConTotales, PedidoCompra, PedidoVenta, PresupuestoResumen, Tercero, TotalesListado } from "./tipos";
import { clasePill, eur, fecha, hoyIso, redondear2, useRetardado, useUltimaPeticion } from "./util";

export interface Fila {
  id: string;
  numero: string;
  fecha: string;
  tercero: string;
  terceroId?: string | null;
  base: number;
  impuestos: number;
  total: number;
  estado: string;
  extra?: string;
  /** Pendiente de cobro/pago (facturas y gastos). */
  pendiente?: number | null;
  vencimiento?: string | null;
}

export type Campo = "numero" | "fecha" | "tercero" | "estado" | "base" | "impuestos" | "total" | "pendiente";

const TITULO: Record<TipoDocumento, string> = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" };
const NUEVO: Record<TipoDocumento, string> = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" };
const ESTADOS: Record<TipoDocumento, string[]> = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"],
};
/** Campos que el servidor sabe ordenar en los listados paginados (facturas y gastos). */
const ORDEN_SERVIDOR: Partial<Record<Campo, string>> = { numero: "numero", fecha: "fecha", tercero: "tercero", base: "base", impuestos: "impuestos", total: "total" };
const TAM = 50;

/** Documentos que no cuentan en los totales. */
export const excluido = (estado: string) => estado === "Anulada" || estado === "Anulado" || estado === "Cancelado";

/** Totales de filas cargadas enteras (sin anulados ni cancelados). */
export function totalesLocales(filas: Fila[], hoy: string): TotalesListado {
  const t: TotalesListado = { documentos: 0, baseImponible: 0, impuestos: 0, retenciones: 0, total: 0, pendiente: 0, vencido: 0, documentosVencidos: 0 };
  for (const f of filas) {
    if (excluido(f.estado)) continue;
    t.documentos++;
    t.baseImponible += f.base;
    t.impuestos += f.impuestos;
    t.total += f.total;
    const p = f.pendiente ?? 0;
    if (p > 0) {
      t.pendiente += p;
      if (f.vencimiento && f.vencimiento < hoy) (t.vencido += p), t.documentosVencidos++;
    }
  }
  t.baseImponible = redondear2(t.baseImponible);
  t.impuestos = redondear2(t.impuestos);
  t.total = redondear2(t.total);
  t.pendiente = redondear2(t.pendiente);
  t.vencido = redondear2(t.vencido);
  return t;
}

/** Orden local de las filas por un campo. */
export function ordenar(filas: Fila[], campo: Campo, desc: boolean): Fila[] {
  const valor = (f: Fila): string | number => (campo === "numero" || campo === "tercero" || campo === "estado" ? f[campo].toLowerCase() : campo === "fecha" ? f.fecha : (f[campo] ?? 0));
  return [...filas].sort((a, b) => {
    const x = valor(a), y = valor(b);
    const r = typeof x === "number" && typeof y === "number" ? x - y : String(x).localeCompare(String(y), "es", { numeric: true });
    return desc ? -r : r;
  });
}

const deFactura = (f: FacturaResumen, pendientes?: Record<string, number>): Fila => ({
  id: f.id, numero: f.numeroCompleto, fecha: f.fechaEmision, tercero: f.clienteNombre + (f.clienteNif ? ` · ${f.clienteNif}` : ""), terceroId: f.clienteId,
  base: f.baseImponible, impuestos: f.cuotaIva, total: f.total, estado: f.estado, extra: f.tipo !== "Ordinaria" ? f.tipo : undefined,
  pendiente: pendientes ? pendientes[f.id] ?? 0 : null, vencimiento: f.fechaVencimiento,
});
const deGasto = (g: Gasto, pendientes?: Record<string, number>): Fila => ({
  id: g.id, numero: g.numeroFactura ?? "—", fecha: g.fecha, tercero: `${g.proveedorTexto ?? ""}${g.numeroFactura ? "" : ` · ${g.concepto}`}`, terceroId: g.proveedorId,
  base: g.baseImponible, impuestos: redondear2(g.cuotaIva + (g.recargoTotal || 0)), total: g.total, estado: g.estado === "Anulado" ? "Anulada" : g.estado, extra: g.esRectificativa ? "Rectificativa" : undefined,
  pendiente: pendientes ? pendientes[g.id] ?? 0 : null, vencimiento: g.vencimientos?.[0]?.fecha ?? g.fecha,
});

export function Listado(props: { tipo: TipoDocumento }) {
  const { api, navegar, anfitrion } = useDocs();
  const tipo = props.tipo;
  const paginado = tipo === "factura" || tipo === "gasto";
  const esCompra = tipo === "compra" || tipo === "gasto";
  const [texto, setTexto] = useState("");
  const [estado, setEstado] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");
  const [tercero, setTercero] = useState("");
  const [serie, setSerie] = useState("");
  const [minimo, setMinimo] = useState("");
  const [maximo, setMaximo] = useState("");
  const [cobro, setCobro] = useState("");
  const [orden, setOrden] = useState<{ campo: Campo; desc: boolean }>({ campo: "fecha", desc: true });
  const [pagina, setPagina] = useState(1);
  const [filas, setFilas] = useState<Fila[] | null>(null);
  const [totalFilas, setTotalFilas] = useState(0);
  const [totalesServidor, setTotalesServidor] = useState<TotalesListado | null>(null);
  const [terceros, setTerceros] = useState<Tercero[]>([]);
  const [series, setSeries] = useState<string[]>([]);
  const [error, setError] = useState("");
  const [exportando, setExportando] = useState(false);
  const buscado = useRetardado(texto, 250);
  const min = useRetardado(minimo, 350);
  const max = useRetardado(maximo, 350);
  const vigente = useUltimaPeticion();
  const hoy = hoyIso();

  // Maestros para los selectores (clientes o proveedores; series de facturas).
  useEffect(() => {
    api.get<Tercero[]>(esCompra ? "/proveedores" : "/clientes").then((t) => setTerceros([...t].sort((a, b) => a.nombre.localeCompare(b.nombre, "es")))).catch(() => setTerceros([]));
    if (tipo === "factura")
      api.get<{ prefijo: string; tipoDocumento: string | number }[]>("/series")
        .then((s) => setSeries([...new Set(s.filter((x) => x.tipoDocumento === "Factura" || x.tipoDocumento === 0).map((x) => x.prefijo))].sort()))
        .catch(() => setSeries([]));
  }, [api, tipo, esCompra]);

  useEffect(() => setPagina(1), [buscado, estado, desde, hasta, tercero, serie, min, max, cobro, orden, tipo]);

  /** Parámetros de la búsqueda en servidor (facturas y gastos). */
  const consulta = (pag: number, tam: number) => {
    const q = new URLSearchParams({ pagina: String(pag), tamanoPagina: String(tam) });
    if (buscado.trim()) q.set("texto", buscado.trim());
    if (estado) q.set("estado", estado === "Anulada" && tipo === "gasto" ? "Anulado" : estado);
    if (desde) q.set("desde", desde);
    if (hasta) q.set("hasta", hasta);
    if (tercero) q.set(tipo === "gasto" ? "proveedorId" : "clienteId", tercero);
    if (serie && tipo === "factura") q.set("serie", serie);
    const nMin = parseFloat(min.replace(/\./g, "").replace(",", "."));
    const nMax = parseFloat(max.replace(/\./g, "").replace(",", "."));
    if (!isNaN(nMin)) q.set("importeMin", String(nMin));
    if (!isNaN(nMax)) q.set("importeMax", String(nMax));
    if (cobro) q.set("cobro", cobro);
    const o = ORDEN_SERVIDOR[orden.campo];
    if (o) (q.set("orden", o === "tercero" ? (tipo === "gasto" ? "proveedor" : "cliente") : o), q.set("desc", String(orden.desc)));
    return q;
  };

  const cargarPagina = async (pag: number, tam: number) => {
    if (tipo === "factura") {
      const r = await api.get<PaginaConTotales<FacturaResumen>>(`/facturas/buscar?${consulta(pag, tam)}`);
      return { r, filas: r.elementos.map((f) => deFactura(f, r.pendientes)) };
    }
    const r = await api.get<PaginaConTotales<Gasto>>(`/gastos/buscar?${consulta(pag, tam)}`);
    return { r, filas: r.elementos.map((g) => deGasto(g, r.pendientes)) };
  };

  useEffect(() => {
    setError("");
    const esVigente = vigente();
    const carga = async (): Promise<Fila[]> => {
      if (paginado) {
        const { r, filas } = await cargarPagina(pagina, TAM);
        if (esVigente()) (setTotalFilas(r.total), setTotalesServidor(r.totales ?? null));
        return filas;
      }
      switch (tipo) {
        case "presupuesto":
          return (await api.get<PresupuestoResumen[]>("/presupuestos")).map((p) => ({ id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.clienteNombre, terceroId: p.clienteId, base: p.baseImponible ?? p.total, impuestos: p.cuotaIva ?? 0, total: p.total, estado: p.estado }));
        case "pedido":
          return (await api.get<PedidoVenta[]>("/pedidos-venta")).map((p) => {
            const base = redondear2(p.lineas.reduce((s, l) => s + l.base, 0));
            return { id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.clienteNombre, terceroId: p.clienteId, base, impuestos: redondear2(p.total - base), total: p.total, estado: p.estado };
          });
        default:
          return (await api.get<PedidoCompra[]>("/compras/pedidos")).map((p) => {
            const base = redondear2(p.lineas.reduce((s, l) => s + l.importe, 0));
            return { id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.proveedorTexto, terceroId: p.proveedorId, base, impuestos: redondear2(p.total - base), total: p.total, estado: p.estado, extra: p.empresaOrigenId ? "Intragrupo" : undefined };
          });
      }
    };
    carga()
      .then((f) => esVigente() && setFilas(f))
      .catch((e: Error) => esVigente() && (setError(e.message), setFilas([])));
  }, [api, tipo, pagina, buscado, estado, desde, hasta, tercero, serie, min, max, cobro, orden.campo, orden.desc]);

  // Filtro y orden locales (listados que el servidor devuelve enteros; en los paginados, solo el orden que el servidor no sabe hacer).
  const visibles = useMemo(() => {
    if (!filas) return [];
    if (paginado) return ORDEN_SERVIDOR[orden.campo] ? filas : ordenar(filas, orden.campo, orden.desc);
    const q = buscado.trim().toLowerCase();
    const nMin = parseFloat(min.replace(/\./g, "").replace(",", "."));
    const nMax = parseFloat(max.replace(/\./g, "").replace(",", "."));
    const r = filas.filter((f) => (!q || f.numero.toLowerCase().includes(q) || f.tercero.toLowerCase().includes(q)) && (!estado || f.estado === estado) && (!desde || f.fecha >= desde) && (!hasta || f.fecha <= hasta)
      && (!tercero || f.terceroId === tercero) && (isNaN(nMin) || f.total >= nMin) && (isNaN(nMax) || f.total <= nMax));
    return ordenar(r, orden.campo, orden.desc);
  }, [filas, buscado, estado, desde, hasta, tercero, min, max, orden, paginado]);

  const totales = paginado ? totalesServidor : totalesLocales(visibles, hoy);
  const paginas = paginado ? Math.max(1, Math.ceil(totalFilas / TAM)) : 1;
  const nFiltros = [estado, desde, hasta, tercero, serie, minimo, maximo, cobro].filter(Boolean).length;
  const etiquetaTercero = esCompra ? "Proveedor" : "Cliente";

  function limpiar() {
    setTexto(""), setEstado(""), setDesde(""), setHasta(""), setTercero(""), setSerie(""), setMinimo(""), setMaximo(""), setCobro("");
  }

  function cabecera(campo: Campo, etiqueta: string, num = false) {
    const activo = orden.campo === campo;
    return (
      <th className={(num ? "num " : "") + "dx-ordenable" + (activo ? " activo" : "")} onClick={() => setOrden({ campo, desc: activo ? !orden.desc : campo === "fecha" || num })} title={`Ordenar por ${etiqueta.toLowerCase()}`}>
        {etiqueta}<span className="dx-flecha">{activo ? (orden.desc ? "▼" : "▲") : ""}</span>
      </th>
    );
  }

  /** Todas las filas del filtro (en los paginados, pidiendo las páginas al servidor) para exportar. */
  async function todasLasFilas(): Promise<Fila[]> {
    if (!paginado) return visibles;
    const resultado: Fila[] = [];
    for (let p = 1; p <= 500; p++) {
      const { r, filas } = await cargarPagina(p, 200);
      resultado.push(...filas);
      if (resultado.length >= r.total || filas.length === 0) break;
    }
    return ORDEN_SERVIDOR[orden.campo] ? resultado : ordenar(resultado, orden.campo, orden.desc);
  }

  async function exportar(formato: "xlsx" | "csv") {
    setExportando(true);
    try {
      const todas = await todasLasFilas();
      const conPendiente = paginado;
      const t = paginado ? totalesServidor : totalesLocales(todas, hoy);
      const peticion: PeticionExportacion = {
        titulo: TITULO[tipo],
        columnas: [
          { titulo: "Número", tipo: "texto" }, { titulo: "Fecha", tipo: "fecha" }, { titulo: etiquetaTercero, tipo: "texto" }, { titulo: "Estado", tipo: "texto" },
          { titulo: "Base", tipo: "moneda", total: "no" }, { titulo: "Impuestos", tipo: "moneda", total: "no" }, { titulo: "Total", tipo: "moneda", total: "no" },
          ...(conPendiente ? [{ titulo: "Pendiente", tipo: "moneda" as const, total: "no" as const }, { titulo: "Vencimiento", tipo: "fecha" as const }] : []),
        ],
        filas: todas.map((f) => [f.numero + (f.extra ? ` (${f.extra})` : ""), fecha(f.fecha), f.tercero, f.estado, f.base, f.impuestos, f.total, ...(conPendiente ? [f.pendiente ?? 0, fecha(f.vencimiento)] : [])]),
        totales: t ? [`Total · ${t.documentos} (sin anulados)`, null, null, null, t.baseImponible, t.impuestos, t.total, ...(conPendiente ? [t.pendiente, null] : [])] : undefined,
      };
      if (formato === "xlsx") await exportarXlsx(anfitrion.token(), peticion);
      else exportarCsv(peticion);
      anfitrion.aviso(`Exportados ${todas.length} documento(s).`, "ok");
    } catch (e) {
      anfitrion.aviso("No se pudo exportar: " + (e as Error).message, "err");
    } finally {
      setExportando(false);
    }
  }

  return (
    <div className="panel">
      <div className="panel-head">
        <h2>{TITULO[tipo]}</h2>
        <div className="dx-acciones">
          <button className="btn small ghost" disabled={exportando || !visibles.length} onClick={() => exportar("xlsx")} title="Exportar a Excel todo lo filtrado">{exportando ? "Exportando…" : "↓ Excel"}</button>
          <button className="btn small ghost" disabled={exportando || !visibles.length} onClick={() => exportar("csv")} title="Exportar a CSV todo lo filtrado">↓ CSV</button>
          <button className="btn small" onClick={() => navegar({ tipo, pantalla: "editor" })}>{NUEVO[tipo]}</button>
        </div>
      </div>
      <div className="dx-filtros">
        <input placeholder={esCompra ? "Número, proveedor o concepto…" : "Número, cliente o NIF…"} value={texto} onChange={(e) => setTexto(e.target.value)} autoFocus />
        <select value={estado} onChange={(e) => setEstado(e.target.value)} aria-label="Estado">
          <option value="">Todos los estados</option>
          {ESTADOS[tipo].map((e) => <option key={e} value={e}>{e}</option>)}
        </select>
        <input type="date" value={desde} onChange={(e) => setDesde(e.target.value)} title="Desde" aria-label="Desde" />
        <input type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} title="Hasta" aria-label="Hasta" />
      </div>
      <div className="dx-filtros dx-filtros2">
        <select value={tercero} onChange={(e) => setTercero(e.target.value)} aria-label={etiquetaTercero}>
          <option value="">{esCompra ? "Todos los proveedores" : "Todos los clientes"}</option>
          {terceros.map((t) => <option key={t.id} value={t.id}>{t.nombre}{t.nifFiscal ? ` · ${t.nifFiscal}` : ""}</option>)}
        </select>
        {tipo === "factura" && (
          <select value={serie} onChange={(e) => setSerie(e.target.value)} aria-label="Serie">
            <option value="">Todas las series</option>
            {series.map((s) => <option key={s} value={s}>Serie {s}</option>)}
          </select>
        )}
        <input inputMode="decimal" placeholder="Importe mín." value={minimo} onChange={(e) => setMinimo(e.target.value)} aria-label="Importe mínimo" />
        <input inputMode="decimal" placeholder="Importe máx." value={maximo} onChange={(e) => setMaximo(e.target.value)} aria-label="Importe máximo" />
        {paginado && (
          <select value={cobro} onChange={(e) => setCobro(e.target.value)} aria-label={tipo === "gasto" ? "Estado de pago" : "Estado de cobro"}>
            <option value="">{tipo === "gasto" ? "Pagadas y pendientes" : "Cobradas y pendientes"}</option>
            <option value="pendiente">Pendientes</option>
            <option value="vencida">Vencidas</option>
            <option value={tipo === "gasto" ? "pagada" : "cobrada"}>{tipo === "gasto" ? "Pagadas" : "Cobradas"}</option>
          </select>
        )}
        {(nFiltros > 0 || texto) && <button className="btn small secondary" onClick={limpiar}>Limpiar{nFiltros ? ` (${nFiltros})` : ""}</button>}
      </div>
      {error && <p className="dx-rojo">{error}</p>}
      {filas === null ? (
        <p className="muted">Cargando…</p>
      ) : visibles.length === 0 ? (
        <p className="muted">No hay documentos con estos filtros.</p>
      ) : (
        <table className="dx-lista-docs">
          <thead>
            <tr>
              {cabecera("numero", "Número")}{cabecera("fecha", "Fecha")}{cabecera("tercero", etiquetaTercero)}{cabecera("estado", "Estado")}
              {cabecera("base", "Base", true)}{cabecera("impuestos", "Impuestos", true)}{cabecera("total", "Total", true)}{paginado && cabecera("pendiente", "Pendiente", true)}
            </tr>
          </thead>
          <tbody>
            {visibles.map((f) => {
              const vencida = (f.pendiente ?? 0) > 0 && !!f.vencimiento && f.vencimiento < hoy;
              return (
                <tr key={f.id} onClick={() => navegar({ tipo, pantalla: "vista", id: f.id })} tabIndex={0} onKeyDown={(e) => e.key === "Enter" && navegar({ tipo, pantalla: "vista", id: f.id })} className={excluido(f.estado) ? "dx-anulado" : undefined}>
                  <td className="mono"><strong>{f.numero}</strong>{f.extra && <span className="pill part" style={{ marginLeft: 6 }}>{f.extra}</span>}</td>
                  <td>{fecha(f.fecha)}</td>
                  <td>{f.tercero}</td>
                  <td><span className={clasePill(f.estado)}>{f.estado}</span></td>
                  <td className="num">{eur(f.base)}</td>
                  <td className="num">{eur(f.impuestos)}</td>
                  <td className="num"><strong>{eur(f.total)}</strong></td>
                  {paginado && (
                    <td className="num">
                      {(f.pendiente ?? 0) > 0 ? <strong className={vencida ? "dx-rojo" : undefined} title={vencida ? `Vencida el ${fecha(f.vencimiento)}` : `Vence el ${fecha(f.vencimiento)}`}>{eur(f.pendiente)}</strong> : <span className="muted">{excluido(f.estado) || f.estado === "Rectificada" ? "—" : tipo === "gasto" ? "Pagada" : "Cobrada"}</span>}
                    </td>
                  )}
                </tr>
              );
            })}
          </tbody>
          {totales && (
            <tfoot>
              <tr className="dx-total">
                <td colSpan={4}>
                  <strong>Total · {totales.documentos}</strong> <span className="muted">{paginado ? `documento${totales.documentos === 1 ? "" : "s"} de todo el filtro (${paginas} página${paginas === 1 ? "" : "s"}) · sin anulados` : "sin anulados ni cancelados"}</span>
                  {paginado && totales.vencido > 0 && <span className="dx-rojo" style={{ marginLeft: 8 }}>· vencido {eur(totales.vencido)} ({totales.documentosVencidos})</span>}
                </td>
                <td className="num"><strong>{eur(totales.baseImponible)}</strong></td>
                <td className="num"><strong>{eur(totales.impuestos)}</strong></td>
                <td className="num"><strong>{eur(totales.total)}</strong></td>
                {paginado && <td className="num"><strong>{eur(totales.pendiente)}</strong></td>}
              </tr>
            </tfoot>
          )}
        </table>
      )}
      {paginas > 1 && (
        <div className="dx-paginas">
          <button className="btn small secondary" disabled={pagina <= 1} onClick={() => setPagina(pagina - 1)}>←</button>
          <span className="muted">Página {pagina} de {paginas} · {totalFilas} documentos</span>
          <button className="btn small secondary" disabled={pagina >= paginas} onClick={() => setPagina(pagina + 1)}>→</button>
        </div>
      )}
    </div>
  );
}
