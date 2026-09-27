/** Vistas de los documentos (solo lectura) con sus acciones y la cadena de documentos relacionados. */
import { useCallback, useEffect, useState, type ReactNode } from "react";
import { abrirFichero, useDocs } from "./contexto";
import { ConceptosAplicados, Dialogo } from "./Componentes";
import type { Albaran, Almacen, Factura, FormaPago, PedidoCompra, PedidoVenta, Presupuesto, Saldo, TipoIva } from "./tipos";
import { cant, clasePill, eur, fecha, hoyIso, num2 } from "./util";

function Cabecera(props: { titulo: ReactNode; estado?: string; acciones?: ReactNode; volver: () => void }) {
  return (
    <div className="panel-head">
      <h2 style={{ display: "flex", alignItems: "center", gap: 10 }}>
        <button className="btn small secondary" onClick={props.volver} title="Volver a la lista">←</button>
        {props.titulo}
        {props.estado && <span className={clasePill(props.estado)}>{props.estado}</span>}
      </h2>
      <div className="dx-acciones">{props.acciones}</div>
    </div>
  );
}

function Dato(props: { etiqueta: string; children: ReactNode }) {
  return (
    <div className="dx-dato">
      <small>{props.etiqueta}</small>
      <div>{props.children}</div>
    </div>
  );
}

function Enlace(props: { children: ReactNode; alPulsar: () => void }) {
  return <button type="button" className="dx-enlace" onClick={props.alPulsar}>{props.children}</button>;
}

function useCarga<T>(cargar: () => Promise<T>) {
  const [dato, setDato] = useState<T | null>(null);
  const [error, setError] = useState("");
  const recargar = useCallback(() => {
    cargar().then(setDato).catch((e: Error) => setError(e.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
  useEffect(recargar, [recargar]);
  return { dato, error, recargar };
}

async function accion(fn: () => Promise<unknown>, aviso: (m: string, t?: "ok" | "err") => void, ok?: string) {
  try {
    await fn();
    if (ok) aviso(ok, "ok");
    return true;
  } catch (e) {
    aviso((e as Error).message, "err");
    return false;
  }
}

// ------------------------------------------------------------------------------------------------ Factura
export function VistaFactura(props: { id: string }) {
  const { api, anfitrion, navegar } = useDocs();
  const { dato: f, error, recargar } = useCarga(() => api.get<Factura>(`/facturas/${props.id}`));
  const [saldo, setSaldo] = useState<Saldo | null>(null);
  const [anular, setAnular] = useState(false);
  const [motivo, setMotivo] = useState("");
  useEffect(() => void api.get<Saldo>(`/facturas/${props.id}/saldo`).then(setSaldo).catch(() => setSaldo(null)), [api, props.id, f]);
  if (error) return <div className="panel"><p className="dx-rojo">{error}</p></div>;
  if (!f) return <div className="muted">Cargando…</div>;
  const semilla = { clienteId: f.clienteId ?? undefined, lineas: f.lineas };
  const coste = f.lineas.reduce((t, l) => t + (l.base - l.margen), 0);
  const emitida = f.estado === "Emitida";
  return (
    <div className="dx-editor">
      <div className="panel">
        <Cabecera
          titulo={<>{f.tipo === "Rectificativa" ? "Rectificativa" : f.tipo === "Simplificada" ? "Ticket" : "Factura"} <span className="mono">{f.numeroCompleto}</span></>}
          estado={f.estado}
          volver={() => navegar({ tipo: "factura", pantalla: "lista" })}
          acciones={
            <>
              <button className="btn small secondary" onClick={() => abrirFichero(anfitrion, `/facturas/${f.id}/pdf`).catch((e) => anfitrion.aviso(e.message, "err"))}>PDF</button>
              {f.tipo !== "Simplificada" && f.clienteNif && (
                <button className="btn small secondary" onClick={() => abrirFichero(anfitrion, `/facturas/${f.id}/facturae.xml`).catch((e) => anfitrion.aviso(e.message, "err"))}>Facturae</button>
              )}
              <button className="btn small secondary" onClick={() => navegar({ tipo: "factura", pantalla: "editor", semilla })}>Duplicar</button>
              {emitida && f.tipo === "Ordinaria" && (
                <button className="btn small secondary" onClick={() => navegar({ tipo: "factura", pantalla: "editor", semilla: { ...semilla, rectificaId: f.id, rectificaNumero: f.numeroCompleto } })}>Rectificar</button>
              )}
              {emitida && <button className="btn small ghost" onClick={() => setAnular(true)}>Anular</button>}
            </>
          }
        />
        <div className="dx-datos">
          <Dato etiqueta="Cliente"><strong>{f.clienteNombre}</strong>{f.clienteNif && <div className="muted mono">{f.clienteNif}</div>}<div className="muted">{[f.clienteCalle, f.clienteCodigoPostal, f.clientePoblacion].filter(Boolean).join(" ")}</div></Dato>
          <Dato etiqueta="Emisión">{fecha(f.fechaEmision)}{f.fechaOperacion !== f.fechaEmision && <div className="muted">Operación {fecha(f.fechaOperacion)}</div>}</Dato>
          <Dato etiqueta="Vencimiento">{fecha(f.fechaVencimiento)}</Dato>
          <Dato etiqueta="Cobro">
            {saldo ? (
              saldo.pendiente <= 0 ? <span className="pill ok">Cobrada</span> : <>Pendiente <strong>{eur(saldo.pendiente)}</strong>{saldo.liquidado > 0 && <div className="muted">Cobrado {eur(saldo.liquidado)}</div>}</>
            ) : "—"}
            {saldo && saldo.pendiente > 0 && emitida && anfitrion.irA && <div><Enlace alPulsar={() => anfitrion.irA!("cobros")}>Registrar cobro</Enlace></div>}
          </Dato>
        </div>
        {f.motivoRectificacion && <p className="muted">Rectifica: {f.motivoRectificacion}{f.rectificaFacturaId && <> · <Enlace alPulsar={() => navegar({ tipo: "factura", pantalla: "vista", id: f.rectificaFacturaId })}>ver la factura original</Enlace></>}</p>}
        {f.motivoAnulacion && <p className="dx-rojo">Anulada: {f.motivoAnulacion}</p>}
      </div>
      <div className="panel">
        <table>
          <thead><tr><th>Descripción</th><th className="num">Cantidad</th><th className="num">Precio</th><th className="num">Dto</th><th>Impuesto</th><th className="num">Importe</th><th className="num">Margen</th></tr></thead>
          <tbody>
            {f.lineas.map((l, i) => (
              <tr key={i}>
                <td>{l.descripcion}<ConceptosAplicados conceptos={l.conceptos} /></td>
                <td className="num">{cant(l.cantidad)}</td>
                <td className="num">{eur(l.precioUnitario)}</td>
                <td className="num">{l.porcentajeDescuento ? `${num2(l.porcentajeDescuento)} %` : ""}</td>
                <td>{l.codigoIva} · {num2(l.porcentajeIva)} %</td>
                <td className="num"><strong>{eur(l.base)}</strong></td>
                <td className="num muted">{l.costeUnitario || l.costeConceptos ? eur(l.margen) : ""}</td>
              </tr>
            ))}
          </tbody>
        </table>
        <div className="dx-totales-vista">
          <div className="dx-tot"><span className="muted">Base imponible</span><span>{eur(f.baseImponible)}</span></div>
          <div className="dx-tot"><span className="muted">{f.impuesto === "Igic" ? "IGIC" : "IVA"}</span><span>{eur(f.cuotaIva)}</span></div>
          {!!f.recargoTotal && <div className="dx-tot"><span className="muted">Recargo de equivalencia</span><span>{eur(f.recargoTotal)}</span></div>}
          {!!f.retencionIrpf && <div className="dx-tot"><span className="muted">Retención IRPF ({num2(f.porcentajeIrpf)} %)</span><span>−{eur(f.retencionIrpf)}</span></div>}
          <div className="dx-tot dx-grande"><span>Total</span><span>{eur(f.total)}</span></div>
          {coste > 0 && <div className="dx-tot"><span className="muted">Margen</span><span className="muted">{eur(f.baseImponible - coste)} ({num2(f.baseImponible ? ((f.baseImponible - coste) / f.baseImponible) * 100 : 0)} %)</span></div>}
        </div>
        {f.mencionFiscal && <p className="muted" style={{ fontSize: 12 }}>{f.mencionFiscal}</p>}
        {f.huella && <p className="muted mono" style={{ fontSize: 11, wordBreak: "break-all" }}>VeriFactu · {f.huella}</p>}
      </div>
      {anular && (
        <Dialogo titulo={`Anular ${f.numeroCompleto}`} alCerrar={() => setAnular(false)}
          acciones={<><button className="btn small secondary" onClick={() => setAnular(false)}>Cancelar</button>
            <button className="btn small" disabled={!motivo.trim()} onClick={async () => (await accion(() => api.post(`/facturas/${f.id}/anular`, { motivo }), anfitrion.aviso, "Factura anulada.")) && (setAnular(false), recargar())}>Anular</button></>}>
          <p className="muted" style={{ marginTop: 0 }}>La factura no se borra: queda anulada con un registro de anulación VeriFactu. Si hay que corregir importes, rectifícala en lugar de anularla.</p>
          <label>Motivo</label>
          <input value={motivo} onChange={(e) => setMotivo(e.target.value)} autoFocus />
        </Dialogo>
      )}
    </div>
  );
}

// ------------------------------------------------------------------------------------------------ Presupuesto
export function VistaPresupuesto(props: { id: string }) {
  const { api, anfitrion, navegar } = useDocs();
  const { dato: p, error, recargar } = useCarga(() => api.get<Presupuesto>(`/presupuestos/${props.id}`));
  if (error) return <div className="panel"><p className="dx-rojo">{error}</p></div>;
  if (!p) return <div className="muted">Cargando…</div>;
  const borrador = p.estado === "Borrador";
  const semilla = { clienteId: p.clienteId, lineas: p.lineas };
  return (
    <div className="dx-editor">
      <div className="panel">
        <Cabecera titulo={<>Presupuesto <span className="mono">{p.numeroCompleto}</span></>} estado={p.estado} volver={() => navegar({ tipo: "presupuesto", pantalla: "lista" })}
          acciones={
            <>
              <button className="btn small secondary" onClick={() => abrirFichero(anfitrion, `/presupuestos/${p.id}/pdf`).catch((e) => anfitrion.aviso(e.message, "err"))}>PDF</button>
              <button className="btn small secondary" onClick={() => navegar({ tipo: "presupuesto", pantalla: "editor", semilla })}>Duplicar</button>
              {borrador && <button className="btn small secondary" onClick={() => navegar({ tipo: "presupuesto", pantalla: "editor", id: p.id, semilla })}>Editar</button>}
              {borrador && <button className="btn small secondary" onClick={async () => { try { const r = await api.post<{ id: string }>("/pedidos-venta/desde-presupuesto", { presupuestoId: p.id }); anfitrion.aviso("Pedido creado.", "ok"); navegar({ tipo: "pedido", pantalla: "vista", id: r.id }); } catch (e) { anfitrion.aviso((e as Error).message, "err"); } }}>Pasar a pedido</button>}
              {borrador && <button className="btn small" onClick={async () => { try { const r = await api.post<{ id: string }>(`/presupuestos/${p.id}/aceptar`, {}); anfitrion.aviso("Factura emitida.", "ok"); navegar({ tipo: "factura", pantalla: "vista", id: r.id }); } catch (e) { anfitrion.aviso((e as Error).message, "err"); } }}>Aceptar y facturar</button>}
              {borrador && <button className="btn small ghost" onClick={async () => (await accion(() => api.post(`/presupuestos/${p.id}/rechazar`, {}), anfitrion.aviso, "Presupuesto rechazado.")) && recargar()}>Rechazar</button>}
            </>
          }
        />
        <div className="dx-datos">
          <Dato etiqueta="Cliente"><strong>{p.clienteNombre}</strong></Dato>
          <Dato etiqueta="Fecha">{fecha(p.fecha)}</Dato>
          <Dato etiqueta="Válido hasta">{fecha(p.validez)}</Dato>
          <Dato etiqueta="Factura">{p.facturaId ? <Enlace alPulsar={() => navegar({ tipo: "factura", pantalla: "vista", id: p.facturaId })}>ver la factura</Enlace> : "—"}</Dato>
        </div>
      </div>
      <div className="panel">
        <table>
          <thead><tr><th>Descripción</th><th className="num">Cantidad</th><th className="num">Precio</th><th className="num">Dto</th><th>Impuesto</th><th className="num">Importe</th></tr></thead>
          <tbody>{p.lineas.map((l, i) => (
            <tr key={i}><td>{l.descripcion}<ConceptosAplicados conceptos={l.conceptos} /></td><td className="num">{cant(l.cantidad)}</td><td className="num">{eur(l.precioUnitario)}</td>
              <td className="num">{l.porcentajeDescuento ? `${num2(l.porcentajeDescuento)} %` : ""}</td><td>{l.codigoIva}</td><td className="num"><strong>{eur(l.base)}</strong></td></tr>
          ))}</tbody>
        </table>
        <div className="dx-totales-vista">
          <div className="dx-tot"><span className="muted">Base imponible</span><span>{eur(p.baseImponible)}</span></div>
          <div className="dx-tot"><span className="muted">Impuestos</span><span>{eur(p.cuotaIva)}</span></div>
          <div className="dx-tot dx-grande"><span>Total</span><span>{eur(p.total)}</span></div>
        </div>
      </div>
    </div>
  );
}

// ------------------------------------------------------------------------------------------------ Pedido de venta
export function VistaPedidoVenta(props: { id: string }) {
  const { api, anfitrion, navegar } = useDocs();
  const { dato: p, error, recargar } = useCarga(() => api.get<PedidoVenta>(`/pedidos-venta/${props.id}`));
  const [albaranes, setAlbaranes] = useState<Albaran[]>([]);
  const [formas, setFormas] = useState<FormaPago[]>([]);
  const [entregar, setEntregar] = useState<Record<string, number> | null>(null);
  const [refEntrega, setRefEntrega] = useState("");
  const [fechaEntrega, setFechaEntrega] = useState(hoyIso());
  const [facturar, setFacturar] = useState(false);
  const [fechaFactura, setFechaFactura] = useState(hoyIso());
  const [formaPago, setFormaPago] = useState("");
  useEffect(() => void api.get<Albaran[]>(`/pedidos-venta/${props.id}/albaranes`).then(setAlbaranes).catch(() => setAlbaranes([])), [api, props.id, p]);
  useEffect(() => void api.get<FormaPago[]>("/formas-pago").then((f) => setFormas(f.filter((x) => x.activo))).catch(() => setFormas([])), [api]);
  if (error) return <div className="panel"><p className="dx-rojo">{error}</p></div>;
  if (!p) return <div className="muted">Cargando…</div>;
  const modificable = (p.estado === "Borrador" || p.estado === "Confirmado") && p.lineas.every((l) => l.cantidadServida === 0);
  const pendiente = p.lineas.some((l) => l.pendienteServir > 0);
  const vivo = p.estado !== "Cancelado" && p.estado !== "Facturado";
  const semilla = { clienteId: p.clienteId, fecha: p.fecha, lineas: p.lineas };
  return (
    <div className="dx-editor">
      <div className="panel">
        <Cabecera titulo={<>Pedido de venta <span className="mono">{p.numeroCompleto}</span></>} estado={p.estado} volver={() => navegar({ tipo: "pedido", pantalla: "lista" })}
          acciones={
            <>
              <button className="btn small secondary" onClick={() => navegar({ tipo: "pedido", pantalla: "editor", semilla: { ...semilla, fecha: undefined } })}>Duplicar</button>
              {modificable && <button className="btn small secondary" onClick={() => navegar({ tipo: "pedido", pantalla: "editor", id: p.id, semilla })}>Editar</button>}
              {p.estado === "Borrador" && <button className="btn small secondary" onClick={async () => (await accion(() => api.post(`/pedidos-venta/${p.id}/confirmar`), anfitrion.aviso, "Pedido confirmado.")) && recargar()}>Confirmar</button>}
              {vivo && p.estado !== "Borrador" && pendiente && <button className="btn small secondary" onClick={() => setEntregar(Object.fromEntries(p.lineas.map((l) => [l.id, l.pendienteServir])))}>Entregar (albarán)</button>}
              {vivo && p.estado !== "Borrador" && <button className="btn small" onClick={() => setFacturar(true)}>Facturar</button>}
              {vivo && <button className="btn small ghost" onClick={async () => window.confirm("¿Cancelar el pedido?") && (await accion(() => api.post(`/pedidos-venta/${p.id}/cancelar`), anfitrion.aviso, "Pedido cancelado.")) && recargar()}>Cancelar</button>}
            </>
          }
        />
        <div className="dx-datos">
          <Dato etiqueta="Cliente"><strong>{p.clienteNombre}</strong></Dato>
          <Dato etiqueta="Fecha">{fecha(p.fecha)}</Dato>
          <Dato etiqueta="Viene de">{p.presupuestoOrigenId ? <Enlace alPulsar={() => navegar({ tipo: "presupuesto", pantalla: "vista", id: p.presupuestoOrigenId })}>presupuesto</Enlace> : "—"}</Dato>
          <Dato etiqueta="Factura">{p.facturaId ? <Enlace alPulsar={() => navegar({ tipo: "factura", pantalla: "vista", id: p.facturaId })}>ver la factura</Enlace> : "—"}</Dato>
        </div>
      </div>
      <div className="panel">
        <table>
          <thead><tr><th>Descripción</th><th className="num">Pedido</th><th className="num">Servido</th><th className="num">Pendiente</th><th className="num">Precio</th><th className="num">Dto</th><th className="num">Importe</th></tr></thead>
          <tbody>{p.lineas.map((l) => (
            <tr key={l.id}><td>{l.descripcion}<ConceptosAplicados conceptos={l.conceptos} /></td><td className="num">{cant(l.cantidad)}</td><td className="num">{cant(l.cantidadServida)}</td>
              <td className="num">{l.pendienteServir > 0 ? <strong>{cant(l.pendienteServir)}</strong> : "—"}</td><td className="num">{eur(l.precioUnitario)}</td>
              <td className="num">{l.porcentajeDescuento ? `${num2(l.porcentajeDescuento)} %` : ""}</td><td className="num"><strong>{eur(l.base)}</strong></td></tr>
          ))}</tbody>
        </table>
        <div className="dx-totales-vista"><div className="dx-tot dx-grande"><span>Total (sin impuestos)</span><span>{eur(p.total)}</span></div></div>
      </div>
      <div className="panel">
        <div className="panel-head"><h2>Albaranes de entrega</h2></div>
        {albaranes.length ? (
          <table>
            <thead><tr><th>Número</th><th>Fecha</th><th>Referencia</th><th>Líneas</th><th /></tr></thead>
            <tbody>{albaranes.map((a) => (
              <tr key={a.id}><td className="mono"><strong>{a.numeroCompleto}</strong> {a.anulado && <span className="pill neg" title={a.motivoAnulacion ?? ""}>Anulado</span>}</td><td>{fecha(a.fecha)}</td><td className="muted">{a.referencia}</td>
                <td className="muted">{a.lineas.map((l) => `${cant(l.cantidad)} × ${l.descripcion}`).join(" · ")}</td>
                <td className="right">{!a.anulado && p.estado !== "Facturado" && <button className="btn small ghost" onClick={async () => { const m = window.prompt("Motivo de la anulación del albarán:"); if (m !== null && (await accion(() => api.post(`/pedidos-venta/${p.id}/albaranes/${a.id}/anular`, { motivo: m || null }), anfitrion.aviso, "Albarán anulado."))) recargar(); }}>Anular</button>}</td></tr>
            ))}</tbody>
          </table>
        ) : <p className="muted" style={{ margin: 0 }}>Sin entregas todavía.</p>}
      </div>
      {entregar && (
        <Dialogo titulo="Entrega (albarán de venta)" alCerrar={() => setEntregar(null)} ancho={640}
          acciones={<><button className="btn small secondary" onClick={() => setEntregar(null)}>Cancelar</button>
            <button className="btn small" onClick={async () => (await accion(() => api.post(`/pedidos-venta/${p.id}/entregar`, { fecha: fechaEntrega, referencia: refEntrega || null, lineas: Object.entries(entregar).filter(([, c]) => c > 0).map(([lineaPedidoId, cantidad]) => ({ lineaPedidoId, cantidad })) }), anfitrion.aviso, "Albarán creado.")) && (setEntregar(null), recargar())}>Crear albarán</button></>}>
          <div className="dx-fila"><div><label>Fecha</label><input type="date" value={fechaEntrega} onChange={(e) => setFechaEntrega(e.target.value)} /></div><div><label>Referencia</label><input value={refEntrega} onChange={(e) => setRefEntrega(e.target.value)} /></div></div>
          <table style={{ marginTop: 10 }}><thead><tr><th>Línea</th><th className="num">Pendiente</th><th className="num" style={{ width: 120 }}>Entregar</th></tr></thead>
            <tbody>{p.lineas.filter((l) => l.pendienteServir > 0).map((l) => (
              <tr key={l.id}><td>{l.descripcion}</td><td className="num">{cant(l.pendienteServir)}</td><td><input className="num" type="number" step="0.001" value={entregar[l.id] ?? 0} onChange={(e) => setEntregar({ ...entregar, [l.id]: Number(e.target.value) })} /></td></tr>
            ))}</tbody></table>
        </Dialogo>
      )}
      {facturar && (
        <Dialogo titulo={`Facturar el pedido ${p.numeroCompleto}`} alCerrar={() => setFacturar(false)}
          acciones={<><button className="btn small secondary" onClick={() => setFacturar(false)}>Cancelar</button>
            <button className="btn small" onClick={async () => { try { const f = await api.post<{ id: string }>(`/pedidos-venta/${p.id}/facturar`, { fechaEmision: fechaFactura, formaPagoId: formaPago || null }); anfitrion.aviso("Factura emitida.", "ok"); navegar({ tipo: "factura", pantalla: "vista", id: f.id }); } catch (e) { anfitrion.aviso((e as Error).message, "err"); } }}>Emitir factura</button></>}>
          <p className="muted" style={{ marginTop: 0 }}>Se factura el pedido completo con sus precios y conceptos. La factura queda numerada y encadenada en VeriFactu.</p>
          <div className="dx-fila"><div><label>Fecha de emisión</label><input type="date" value={fechaFactura} onChange={(e) => setFechaFactura(e.target.value)} /></div>
            <div><label>Forma de pago</label><select value={formaPago} onChange={(e) => setFormaPago(e.target.value)}><option value="">La del cliente</option>{formas.map((f) => <option key={f.id} value={f.id}>{f.nombre}</option>)}</select></div></div>
        </Dialogo>
      )}
    </div>
  );
}

// ------------------------------------------------------------------------------------------------ Pedido de compra
export function VistaPedidoCompra(props: { id: string }) {
  const { api, anfitrion, navegar } = useDocs();
  const { dato: p, error, recargar } = useCarga(() => api.get<PedidoCompra>(`/compras/pedidos/${props.id}`));
  const [albaranes, setAlbaranes] = useState<Albaran[]>([]);
  const [almacenes, setAlmacenes] = useState<Almacen[]>([]);
  const [ivas, setIvas] = useState<TipoIva[]>([]);
  const [recibir, setRecibir] = useState<Record<string, { cantidad: number; lote: string }> | null>(null);
  const [almacenId, setAlmacenId] = useState("");
  const [refRecepcion, setRefRecepcion] = useState("");
  const [fechaRecepcion, setFechaRecepcion] = useState(hoyIso());
  const [facturar, setFacturar] = useState(false);
  const [iva, setIva] = useState("IVA21");
  const [irpf, setIrpf] = useState(0);
  const [numFactura, setNumFactura] = useState("");
  const [fechaFactura, setFechaFactura] = useState(hoyIso());
  useEffect(() => void api.get<Albaran[]>(`/compras/pedidos/${props.id}/albaranes`).then(setAlbaranes).catch(() => setAlbaranes([])), [api, props.id, p]);
  useEffect(() => {
    api.get<Almacen[]>("/inventario/almacenes").then((a) => (setAlmacenes(a), a[0] && setAlmacenId(a[0].id))).catch(() => setAlmacenes([]));
    api.get<TipoIva[]>("/tipos-iva").then((t) => setIvas(t.filter((x) => x.activo))).catch(() => setIvas([]));
  }, [api]);
  if (error) return <div className="panel"><p className="dx-rojo">{error}</p></div>;
  if (!p) return <div className="muted">Cargando…</div>;
  const modificable = (p.estado === "Borrador" || p.estado === "Confirmado") && p.lineas.every((l) => l.cantidadRecibida === 0 && l.cantidadFacturada === 0) && !p.empresaOrigenId;
  const vivo = p.estado !== "Cancelado" && p.estado !== "Facturado";
  const pendiente = p.lineas.some((l) => l.pendienteRecibir > 0);
  const costeAnadido = p.lineas.reduce((t, l) => t + l.costeConceptos, 0);
  return (
    <div className="dx-editor">
      <div className="panel">
        <Cabecera titulo={<>Pedido de compra <span className="mono">{p.numeroCompleto}</span></>} estado={p.estado} volver={() => navegar({ tipo: "compra", pantalla: "lista" })}
          acciones={
            <>
              {!p.empresaOrigenId && <button className="btn small secondary" onClick={() => navegar({ tipo: "compra", pantalla: "editor", semilla: { ...p, fecha: hoyIso() } })}>Duplicar</button>}
              {modificable && <button className="btn small secondary" onClick={() => navegar({ tipo: "compra", pantalla: "editor", id: p.id, semilla: p })}>Editar</button>}
              {p.estado === "Borrador" && <button className="btn small secondary" onClick={async () => (await accion(() => api.post(`/compras/pedidos/${p.id}/confirmar`), anfitrion.aviso, "Pedido confirmado.")) && recargar()}>Confirmar</button>}
              {vivo && p.estado !== "Borrador" && pendiente && <button className="btn small secondary" onClick={() => setRecibir(Object.fromEntries(p.lineas.map((l) => [l.id, { cantidad: l.pendienteRecibir, lote: "" }])))}>Recibir mercancía</button>}
              {vivo && p.estado !== "Borrador" && <button className="btn small" onClick={() => setFacturar(true)}>Facturar</button>}
              {vivo && !p.empresaOrigenId && <button className="btn small ghost" onClick={async () => window.confirm("¿Cancelar el pedido?") && (await accion(() => api.post(`/compras/pedidos/${p.id}/cancelar`), anfitrion.aviso, "Pedido cancelado.")) && recargar()}>Cancelar</button>}
            </>
          }
        />
        <div className="dx-datos">
          <Dato etiqueta="Proveedor"><strong>{p.proveedorTexto}</strong></Dato>
          <Dato etiqueta="Fecha">{fecha(p.fecha)}</Dato>
          <Dato etiqueta="Total">{eur(p.total)}</Dato>
          <Dato etiqueta="Costes añadidos">{costeAnadido ? eur(costeAnadido) : "—"}</Dato>
        </div>
        {p.empresaOrigenId && <p className="muted">Traspaso de otra empresa del grupo: se gestiona desde el documento de venta de origen.</p>}
      </div>
      <div className="panel">
        <table>
          <thead><tr><th>Descripción</th><th className="num">Pedido</th><th className="num">Recibido</th><th className="num">Pendiente</th><th className="num">Precio</th><th className="num">Importe</th><th className="num">Coste entrada</th></tr></thead>
          <tbody>{p.lineas.map((l) => (
            <tr key={l.id}><td>{l.descripcion}<ConceptosAplicados conceptos={l.conceptos} /></td><td className="num">{cant(l.cantidad)}</td><td className="num">{cant(l.cantidadRecibida)}</td>
              <td className="num">{l.pendienteRecibir > 0 ? <strong>{cant(l.pendienteRecibir)}</strong> : "—"}</td><td className="num">{eur(l.precioUnitario)}</td>
              <td className="num"><strong>{eur(l.importe)}</strong></td><td className="num muted">{eur(l.costeUnitarioEntrada)}/ud</td></tr>
          ))}</tbody>
        </table>
      </div>
      <div className="panel">
        <div className="panel-head"><h2>Albaranes de recepción</h2></div>
        {albaranes.length ? (
          <table>
            <thead><tr><th>Número</th><th>Fecha</th><th>Referencia</th><th>Almacén</th><th>Líneas</th><th /></tr></thead>
            <tbody>{albaranes.map((a) => (
              <tr key={a.id}><td className="mono"><strong>{a.numeroCompleto}</strong> {a.anulado && <span className="pill neg" title={a.motivoAnulacion ?? ""}>Anulado</span>}</td><td>{fecha(a.fecha)}</td><td className="muted">{a.referencia}</td>
                <td className="muted">{almacenes.find((x) => x.id === a.almacenId)?.nombre ?? "—"}</td>
                <td className="muted">{a.lineas.map((l) => `${cant(l.cantidad)} × ${l.descripcion}`).join(" · ")}</td>
                <td className="right">{!a.anulado && p.estado !== "Facturado" && <button className="btn small ghost" onClick={async () => { const m = window.prompt("Motivo de la anulación del albarán:"); if (m !== null && (await accion(() => api.post(`/compras/pedidos/${p.id}/albaranes/${a.id}/anular`, { motivo: m || null }), anfitrion.aviso, "Albarán anulado."))) recargar(); }}>Anular</button>}</td></tr>
            ))}</tbody>
          </table>
        ) : <p className="muted" style={{ margin: 0 }}>Sin recepciones todavía.</p>}
      </div>
      {recibir && (
        <Dialogo titulo="Recepción de mercancía" alCerrar={() => setRecibir(null)} ancho={680}
          acciones={<><button className="btn small secondary" onClick={() => setRecibir(null)}>Cancelar</button>
            <button className="btn small" onClick={async () => (await accion(() => api.post(`/compras/pedidos/${p.id}/recibir`, { fecha: fechaRecepcion, referencia: refRecepcion || null, almacenId: almacenId || null, lineas: Object.entries(recibir).filter(([, r]) => r.cantidad > 0).map(([lineaPedidoId, r]) => ({ lineaPedidoId, cantidad: r.cantidad, lote: r.lote || null })) }), anfitrion.aviso, "Recepción registrada.")) && (setRecibir(null), recargar())}>Registrar recepción</button></>}>
          <div className="dx-fila">
            <div><label>Fecha</label><input type="date" value={fechaRecepcion} onChange={(e) => setFechaRecepcion(e.target.value)} /></div>
            <div><label>Albarán del proveedor</label><input value={refRecepcion} onChange={(e) => setRefRecepcion(e.target.value)} /></div>
            <div><label>Almacén</label><select value={almacenId} onChange={(e) => setAlmacenId(e.target.value)}><option value="">Sin entrada en almacén</option>{almacenes.map((a) => <option key={a.id} value={a.id}>{a.nombre}</option>)}</select></div>
          </div>
          <table style={{ marginTop: 10 }}><thead><tr><th>Línea</th><th className="num">Pendiente</th><th className="num" style={{ width: 110 }}>Recibir</th><th style={{ width: 130 }}>Lote</th></tr></thead>
            <tbody>{p.lineas.filter((l) => l.pendienteRecibir > 0).map((l) => (
              <tr key={l.id}><td>{l.descripcion}</td><td className="num">{cant(l.pendienteRecibir)}</td>
                <td><input className="num" type="number" step="0.001" value={recibir[l.id]?.cantidad ?? 0} onChange={(e) => setRecibir({ ...recibir, [l.id]: { ...recibir[l.id], cantidad: Number(e.target.value) } })} /></td>
                <td><input value={recibir[l.id]?.lote ?? ""} onChange={(e) => setRecibir({ ...recibir, [l.id]: { ...recibir[l.id], lote: e.target.value } })} /></td></tr>
            ))}</tbody></table>
        </Dialogo>
      )}
      {facturar && (
        <Dialogo titulo={`Facturar el pedido ${p.numeroCompleto}`} alCerrar={() => setFacturar(false)}
          acciones={<><button className="btn small secondary" onClick={() => setFacturar(false)}>Cancelar</button>
            <button className="btn small" onClick={async () => (await accion(() => api.post(`/compras/pedidos/${p.id}/facturar`, { codigoIva: iva, porcentajeIrpf: irpf, numeroFactura: numFactura || null, fechaFactura }), anfitrion.aviso, "Factura del proveedor registrada como gasto.")) && (setFacturar(false), recargar())}>Registrar factura</button></>}>
          <p className="muted" style={{ marginTop: 0 }}>Registra la factura del proveedor por el total del pedido ({eur(p.total)}) como gasto, con su asiento si la contabilidad es automática.</p>
          <div className="dx-fila">
            <div><label>Nº de factura del proveedor</label><input value={numFactura} onChange={(e) => setNumFactura(e.target.value)} autoFocus /></div>
            <div><label>Fecha de la factura</label><input type="date" value={fechaFactura} onChange={(e) => setFechaFactura(e.target.value)} /></div>
          </div>
          <div className="dx-fila">
            <div><label>Impuesto</label><select value={iva} onChange={(e) => setIva(e.target.value)}>{ivas.map((t) => <option key={t.codigo} value={t.codigo}>{t.nombre}</option>)}</select></div>
            <div><label>Retención IRPF %</label><input type="number" step="0.01" value={irpf} onChange={(e) => setIrpf(Number(e.target.value))} /></div>
          </div>
        </Dialogo>
      )}
    </div>
  );
}
