/** Listados de documentos con búsqueda, filtros por estado y fechas, totales y paginación (facturas). */
import { useEffect, useMemo, useState } from "react";
import { useDocs, type TipoDocumento } from "./contexto";
import type { FacturaResumen, Gasto, Pagina, PedidoCompra, PedidoVenta, PresupuestoResumen } from "./tipos";
import { clasePill, eur, fecha, useRetardado } from "./util";

interface Fila {
  id: string;
  numero: string;
  fecha: string;
  tercero: string;
  total: number;
  estado: string;
  extra?: string;
}

const TITULO: Record<TipoDocumento, string> = { presupuesto: "Presupuestos", pedido: "Pedidos de venta", factura: "Facturas emitidas", compra: "Pedidos de compra", gasto: "Facturas de proveedor y gastos" };
const NUEVO: Record<TipoDocumento, string> = { presupuesto: "+ Nuevo presupuesto", pedido: "+ Nuevo pedido", factura: "+ Nueva factura", compra: "+ Nuevo pedido", gasto: "+ Registrar factura" };
const ESTADOS: Record<TipoDocumento, string[]> = {
  presupuesto: ["Borrador", "Aceptado", "Rechazado"],
  pedido: ["Borrador", "Confirmado", "ServidoParcial", "Servido", "Facturado", "Cancelado"],
  factura: ["Emitida", "Rectificada", "Anulada"],
  compra: ["Borrador", "Confirmado", "RecibidoParcial", "Recibido", "Facturado", "Cancelado"],
  gasto: ["Registrado", "Anulado"],
};

export function Listado(props: { tipo: TipoDocumento }) {
  const { api, navegar } = useDocs();
  const [texto, setTexto] = useState("");
  const [estado, setEstado] = useState("");
  const [desde, setDesde] = useState("");
  const [hasta, setHasta] = useState("");
  const [pagina, setPagina] = useState(1);
  const [filas, setFilas] = useState<Fila[] | null>(null);
  const [totalFilas, setTotalFilas] = useState(0);
  const [error, setError] = useState("");
  const buscado = useRetardado(texto, 250);
  const tam = 50;

  useEffect(() => setPagina(1), [buscado, estado, desde, hasta, props.tipo]);

  useEffect(() => {
    setError("");
    const carga = async (): Promise<Fila[]> => {
      switch (props.tipo) {
        case "factura": {
          const q = new URLSearchParams({ pagina: String(pagina), tamanoPagina: String(tam) });
          if (buscado.trim()) q.set("texto", buscado.trim());
          if (estado) q.set("estado", estado);
          if (desde) q.set("desde", desde);
          if (hasta) q.set("hasta", hasta);
          const r = await api.get<Pagina<FacturaResumen>>(`/facturas/buscar?${q}`);
          setTotalFilas(r.total);
          return r.elementos.map((f) => ({ id: f.id, numero: f.numeroCompleto, fecha: f.fechaEmision, tercero: f.clienteNombre + (f.clienteNif ? ` · ${f.clienteNif}` : ""), total: f.total, estado: f.estado, extra: f.tipo !== "Ordinaria" ? f.tipo : undefined }));
        }
        case "gasto": {
          const q = new URLSearchParams({ pagina: String(pagina), tamanoPagina: String(tam) });
          if (buscado.trim()) q.set("texto", buscado.trim());
          if (estado) q.set("estado", estado);
          if (desde) q.set("desde", desde);
          if (hasta) q.set("hasta", hasta);
          const r = await api.get<Pagina<Gasto>>(`/gastos/buscar?${q}`);
          setTotalFilas(r.total);
          return r.elementos.map((g) => ({ id: g.id, numero: g.numeroFactura ?? "—", fecha: g.fecha, tercero: `${g.proveedorTexto ?? ""}${g.numeroFactura ? "" : ` · ${g.concepto}`}`, total: g.total, estado: g.estado === "Anulado" ? "Anulada" : g.estado }));
        }
        case "presupuesto":
          return (await api.get<PresupuestoResumen[]>("/presupuestos")).map((p) => ({ id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.clienteNombre, total: p.total, estado: p.estado }));
        case "pedido":
          return (await api.get<PedidoVenta[]>("/pedidos-venta")).map((p) => ({ id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.clienteNombre, total: p.total, estado: p.estado }));
        case "compra":
          return (await api.get<PedidoCompra[]>("/compras/pedidos")).map((p) => ({ id: p.id, numero: p.numeroCompleto, fecha: p.fecha, tercero: p.proveedorTexto, total: p.total, estado: p.estado, extra: p.empresaOrigenId ? "Intragrupo" : undefined }));
      }
    };
    carga().then(setFilas).catch((e: Error) => (setError(e.message), setFilas([])));
  }, [api, props.tipo, pagina, buscado, estado, desde, hasta]);

  // Filtro local para los listados que el servidor devuelve enteros.
  const visibles = useMemo(() => {
    if (!filas || props.tipo === "factura" || props.tipo === "gasto") return filas ?? [];
    const q = buscado.trim().toLowerCase();
    return filas.filter((f) => (!q || f.numero.toLowerCase().includes(q) || f.tercero.toLowerCase().includes(q)) && (!estado || f.estado === estado) && (!desde || f.fecha >= desde) && (!hasta || f.fecha <= hasta));
  }, [filas, buscado, estado, desde, hasta, props.tipo]);
  const suma = visibles.reduce((t, f) => t + (f.estado === "Anulada" || f.estado === "Cancelado" ? 0 : f.total), 0);
  const paginado = props.tipo === "factura" || props.tipo === "gasto";
  const paginas = paginado ? Math.max(1, Math.ceil(totalFilas / tam)) : 1;

  return (
    <div className="panel">
      <div className="panel-head">
        <h2>{TITULO[props.tipo]}</h2>
        <button className="btn small" onClick={() => navegar({ tipo: props.tipo, pantalla: "editor" })}>{NUEVO[props.tipo]}</button>
      </div>
      <div className="dx-filtros">
        <input placeholder={props.tipo === "compra" || props.tipo === "gasto" ? "Número, proveedor o concepto…" : "Número, cliente o NIF…"} value={texto} onChange={(e) => setTexto(e.target.value)} autoFocus />
        <select value={estado} onChange={(e) => setEstado(e.target.value)}>
          <option value="">Todos los estados</option>
          {ESTADOS[props.tipo].map((e) => <option key={e} value={e}>{e}</option>)}
        </select>
        <input type="date" value={desde} onChange={(e) => setDesde(e.target.value)} title="Desde" />
        <input type="date" value={hasta} onChange={(e) => setHasta(e.target.value)} title="Hasta" />
      </div>
      {error && <p className="dx-rojo">{error}</p>}
      {filas === null ? (
        <p className="muted">Cargando…</p>
      ) : visibles.length === 0 ? (
        <p className="muted">No hay documentos con estos filtros.</p>
      ) : (
        <table className="dx-lista-docs">
          <thead><tr><th>Número</th><th>Fecha</th><th>{props.tipo === "compra" || props.tipo === "gasto" ? "Proveedor" : "Cliente"}</th><th>Estado</th><th className="num">Total</th></tr></thead>
          <tbody>
            {visibles.map((f) => (
              <tr key={f.id} onClick={() => navegar({ tipo: props.tipo, pantalla: "vista", id: f.id })} tabIndex={0} onKeyDown={(e) => e.key === "Enter" && navegar({ tipo: props.tipo, pantalla: "vista", id: f.id })}>
                <td className="mono"><strong>{f.numero}</strong>{f.extra && <span className="pill part" style={{ marginLeft: 6 }}>{f.extra}</span>}</td>
                <td>{fecha(f.fecha)}</td>
                <td>{f.tercero}</td>
                <td><span className={clasePill(f.estado)}>{f.estado}</span></td>
                <td className="num"><strong>{eur(f.total)}</strong></td>
              </tr>
            ))}
          </tbody>
          <tfoot><tr><td colSpan={4} className="muted">{paginado ? `${totalFilas} documentos` : `${visibles.length} documentos`} · suma de la página sin anulados</td><td className="num"><strong>{eur(suma)}</strong></td></tr></tfoot>
        </table>
      )}
      {paginas > 1 && (
        <div className="dx-paginas">
          <button className="btn small secondary" disabled={pagina <= 1} onClick={() => setPagina(pagina - 1)}>←</button>
          <span className="muted">Página {pagina} de {paginas}</span>
          <button className="btn small secondary" disabled={pagina >= paginas} onClick={() => setPagina(pagina + 1)}>→</button>
        </div>
      )}
    </div>
  );
}
