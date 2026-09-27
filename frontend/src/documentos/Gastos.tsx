/**
 * Facturas de proveedor (gastos): editor con número y fecha de la factura, líneas con su cuenta de gasto, base, tipo de
 * impuesto (también inversión del sujeto pasivo e intracomunitarias, que se autoliquidan), parte deducible y recargo;
 * retención, vencimientos y totales calculados en el servidor sin guardar. Vista con pagos, corrección y anulación.
 */
import { useEffect, useMemo, useState } from "react";
import { useDocs } from "./contexto";
import { Dialogo, SelectorTercero } from "./Componentes";
import type { Cuenta, FormaPago, Gasto, Saldo, Tercero, TipoIvaCompleto } from "./tipos";
import { clasePill, eur, fecha, hoyIso, nuevaClave, num2, redondear2, useRetardado, useUltimaPeticion } from "./util";

interface LineaEd {
  clave: string;
  descripcion: string;
  cuentaGasto: string;
  base: number;
  codigoIva: string;
  porcentajeIva: number | null;
  porcentajeDeducible: number;
}

const lineaNueva = (codigo = ""): LineaEd => ({ clave: nuevaClave(), descripcion: "", cuentaGasto: "", base: 0, codigoIva: codigo, porcentajeIva: null, porcentajeDeducible: 100 });
const autoliquidable = (clase?: string) => clase === "InversionSujetoPasivo" || clase === "Intracomunitario";

export function EditorGasto(props: { id?: string | null; semilla?: Gasto; alGuardar: (id: string) => void; alCancelar: () => void }) {
  const { api, anfitrion } = useDocs();
  const s = props.semilla;
  const [proveedores, setProveedores] = useState<Tercero[]>([]);
  const [ivas, setIvas] = useState<TipoIvaCompleto[]>([]);
  const [formas, setFormas] = useState<FormaPago[]>([]);
  const [cuentas, setCuentas] = useState<Cuenta[]>([]);
  const [proveedorId, setProveedorId] = useState(s?.proveedorId ?? "");
  const [numero, setNumero] = useState(props.id ? s?.numeroFactura ?? "" : "");
  const [fechaFactura, setFechaFactura] = useState(s?.fechaFactura ?? hoyIso());
  const [fechaRegistro, setFechaRegistro] = useState(props.id ? s?.fecha ?? hoyIso() : hoyIso());
  const [concepto, setConcepto] = useState(s?.concepto && !s.concepto.startsWith("Factura ") ? s.concepto : "");
  const [irpf, setIrpf] = useState(s?.porcentajeIrpf ?? 0);
  const [formaPagoId, setFormaPagoId] = useState("");
  const [recargo, setRecargo] = useState((s?.recargoTotal ?? 0) > 0);
  const rectificativa = !!s?.esRectificativa;
  const [numeroRectificado, setNumeroRectificado] = useState(s?.numeroRectificado ?? "");
  const [fechaRectificada, setFechaRectificada] = useState(s?.fechaRectificada ?? "");
  const [motivo, setMotivo] = useState(s?.motivoRectificacion ?? "");
  const [enRecargo, setEnRecargo] = useState(false);
  const [afectacion, setAfectacion] = useState(s?.afectacion ?? "Comun");
  const [lineas, setLineas] = useState<LineaEd[]>(() =>
    s?.lineas?.length
      ? s.lineas.map((l) => ({ clave: nuevaClave(), descripcion: l.descripcion ?? "", cuentaGasto: l.cuentaGasto ?? "", base: l.base, codigoIva: l.codigoIva, porcentajeIva: l.autoliquidada ? l.porcentajeIva : null, porcentajeDeducible: l.porcentajeDeducible }))
      : [lineaNueva()],
  );
  const [plazosManual, setPlazosManual] = useState<{ fecha: string; importe: number }[] | null>(props.id && s?.vencimientos && s.vencimientos.length > 1 ? s.vencimientos : null);
  const [calculo, setCalculo] = useState<Gasto | null>(null);
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);
  const ultima = useUltimaPeticion();

  useEffect(() => {
    api.get<Tercero[]>("/proveedores").then(setProveedores).catch(() => setProveedores([]));
    api.get<TipoIvaCompleto[]>("/tipos-iva").then((t) => setIvas(t.filter((x) => x.activo))).catch(() => setIvas([]));
    api.get<FormaPago[]>("/formas-pago").then((f) => setFormas(f.filter((x) => x.activo))).catch(() => setFormas([]));
    api.get<{ regimenIva?: string }>("/empresas/actual").then((e) => {
      // Comerciante minorista en recargo de equivalencia: el proveedor le cobra el recargo y no deduce el IVA.
      if (e.regimenIva === "RecargoEquivalencia") {
        setEnRecargo(true);
        if (!s) setRecargo(true);
      }
    }).catch(() => undefined);
    api.get<Cuenta[]>("/contabilidad/cuentas").then((c) => setCuentas(c.filter((x) => x.codigo.startsWith("6") || x.codigo.startsWith("2")))).catch(() => setCuentas([]));
  }, [api]);

  const proveedor = proveedores.find((p) => p.id === proveedorId);
  useEffect(() => {
    if (proveedor?.formaPagoDefectoId && !formaPagoId) setFormaPagoId(proveedor.formaPagoDefectoId);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [proveedor]);

  const comando = useMemo(
    () => ({
      proveedorId: proveedorId || null,
      proveedorTexto: proveedor?.nombre ?? null,
      numeroFactura: numero.trim() || null,
      fechaFactura: fechaFactura || null,
      fecha: fechaRegistro,
      concepto: concepto.trim() || null,
      porcentajeIrpf: irpf,
      formaPagoId: formaPagoId || null,
      recargoEquivalencia: recargo,
      afectacion,
      baseImponible: 0,
      lineas: lineas.filter((l) => l.base !== 0).map((l) => ({
        base: l.base,
        codigoIva: l.codigoIva || null,
        descripcion: l.descripcion.trim() || null,
        porcentajeIva: l.porcentajeIva,
        porcentajeDeducible: l.porcentajeDeducible,
        cuentaGasto: l.cuentaGasto.trim() || null,
      })),
      vencimientos: plazosManual,
      rectificaGastoId: rectificativa ? s?.rectificaGastoId ?? null : null,
      numeroRectificado: rectificativa ? numeroRectificado.trim() || null : null,
      fechaRectificada: rectificativa ? fechaRectificada || null : null,
      motivoRectificacion: rectificativa ? motivo.trim() || null : null,
    }),
    [proveedorId, proveedor, numero, fechaFactura, fechaRegistro, concepto, irpf, formaPagoId, recargo, afectacion, lineas, plazosManual, rectificativa, s, numeroRectificado, fechaRectificada, motivo],
  );
  const retardado = useRetardado(comando, 350);

  useEffect(() => {
    if (!retardado.lineas.length) {
      setCalculo(null);
      setError("Añade al menos una línea con base.");
      return;
    }
    const vigente = ultima();
    api
      .post<Gasto>("/gastos/simular", retardado)
      .then((g) => vigente() && (setCalculo(g), setError("")))
      .catch((e: Error) => vigente() && (setCalculo(null), setError(e.message)));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [retardado, api]);

  const cambiar = (clave: string, c: Partial<LineaEd>) => setLineas((ls) => ls.map((l) => (l.clave === clave ? { ...l, ...c } : l)));
  const ivaDe = (codigo: string) => ivas.find((t) => t.codigo === codigo);
  const cuotaLinea = (l: LineaEd) => calculo?.lineas?.[lineas.filter((x) => x.base !== 0).indexOf(l)];

  function repartir(n: number) {
    if (!calculo) return;
    const base = new Date((fechaFactura || fechaRegistro) + "T00:00:00");
    const parte = redondear2(calculo.total / n);
    setPlazosManual(Array.from({ length: n }, (_, i) => {
      const d = new Date(base);
      d.setMonth(d.getMonth() + i + 1);
      return { fecha: d.toISOString().slice(0, 10), importe: i === n - 1 ? redondear2(calculo.total - parte * (n - 1)) : parte };
    }));
  }

  async function guardar() {
    setGuardando(true);
    try {
      const r = props.id ? await api.put<Gasto>(`/gastos/${props.id}`, comando) : await api.post<Gasto>("/gastos", comando);
      anfitrion.aviso(props.id ? "Factura corregida." : "Factura registrada.", "ok");
      if (r.avisoRiesgo) anfitrion.aviso(r.avisoRiesgo, "err");
      props.alGuardar(r.id);
    } catch (e) {
      anfitrion.aviso((e as Error).message, "err");
    } finally {
      setGuardando(false);
    }
  }

  const sumaPlazos = redondear2((plazosManual ?? []).reduce((t, p) => t + (Number(p.importe) || 0), 0));
  return (
    <div className="dx-editor">
      <div className="panel">
        <div className="panel-head">
          <h2>{props.id ? `Corregir factura ${s?.numeroFactura ?? ""}` : rectificativa ? "Rectificativa / abono del proveedor" : "Nueva factura de proveedor"}</h2>
          <div style={{ display: "flex", gap: 8 }}>
            <button className="btn small secondary" onClick={props.alCancelar}>Cancelar</button>
            <button className="btn small" disabled={!calculo || guardando} onClick={guardar}>{props.id ? "Guardar corrección" : rectificativa ? "Registrar abono" : "Registrar factura"}</button>
          </div>
        </div>
        <div className="dx-cabecera">
          <div className="dx-cab-campos">
            <SelectorTercero terceros={proveedores} valor={proveedorId} alCambiar={setProveedorId} etiqueta="Proveedor" />
            <div className="dx-fila">
              <div><label>Nº de factura del proveedor</label><input value={numero} onChange={(e) => setNumero(e.target.value)} placeholder="F-2026/0117" /></div>
              <div><label>Fecha de la factura</label><input type="date" value={fechaFactura} onChange={(e) => setFechaFactura(e.target.value)} /></div>
              <div><label>Fecha de registro</label><input type="date" value={fechaRegistro} onChange={(e) => setFechaRegistro(e.target.value)} title="La del asiento y la del periodo de IVA en que se deduce" /></div>
            </div>
            <div className="dx-fila">
              <div><label>Forma de pago</label><select value={formaPagoId} onChange={(e) => setFormaPagoId(e.target.value)}><option value="">Contado (vence en la fecha de factura)</option>{formas.map((f) => <option key={f.id} value={f.id}>{f.nombre}</option>)}</select></div>
              <div><label>Retención IRPF %</label><input type="number" step="0.01" value={irpf} onChange={(e) => setIrpf(Number(e.target.value))} /></div>
              <div><label>Prorrata especial</label><select value={afectacion} onChange={(e) => setAfectacion(e.target.value)}><option value="Comun">Uso común</option><option value="ConDerecho">Solo operaciones con derecho</option><option value="SinDerecho">Solo operaciones exentas</option></select></div>
            </div>
            <div className="dx-fila">
              <div style={{ gridColumn: "1 / -1" }}><label>Concepto (vacío: «Factura nº»)</label><input value={concepto} onChange={(e) => setConcepto(e.target.value)} /></div>
            </div>
            {rectificativa && (
              <div className="dx-fila">
                <div><label>Factura que rectifica</label><input value={numeroRectificado} disabled={!!s?.rectificaGastoId} onChange={(e) => setNumeroRectificado(e.target.value)} /></div>
                <div><label>Fecha de la rectificada</label><input type="date" value={fechaRectificada} disabled={!!s?.rectificaGastoId} onChange={(e) => setFechaRectificada(e.target.value)} /></div>
                <div><label>Motivo</label><input value={motivo} onChange={(e) => setMotivo(e.target.value)} placeholder="Devolución, descuento posterior, error de precio…" /></div>
              </div>
            )}
            {rectificativa && <div className="muted">Las bases del abono van en negativo; el asiento y el SII (R1, por diferencias) salen con el signo.</div>}
            <label className="dx-check"><input type="checkbox" checked={recargo} onChange={(e) => setRecargo(e.target.checked)} /> El proveedor me cobra recargo de equivalencia</label>
            {enRecargo && <div className="muted">La empresa está en recargo de equivalencia: el IVA y el recargo soportados son coste (no se deducen).</div>}
          </div>
          <div className="dx-ficha">
            {proveedor ? (
              <>
                <strong>{proveedor.nombre}</strong>
                <div className="muted">{[proveedor.nifFiscal, proveedor.poblacion, proveedor.pais].filter(Boolean).join(" · ")}</div>
                {!proveedor.nifFiscal && <div className="dx-aviso">⚠ Sin NIF en la ficha: hace falta para el libro de IVA, el 347 y el SII.</div>}
                {calculo?.avisoRiesgo && <div className="dx-aviso">⚠ {calculo.avisoRiesgo}</div>}
              </>
            ) : (
              <span className="muted">Elige el proveedor: su NIF va al libro de IVA y al SII, y el número de factura no se puede repetir.</span>
            )}
          </div>
        </div>
      </div>

      <div className="panel dx-rejilla">
        <table>
          <thead>
            <tr><th>Descripción</th><th style={{ width: 130 }}>Cuenta de gasto</th><th className="num" style={{ width: 120 }}>Base</th><th style={{ width: 200 }}>Impuesto</th>
              <th className="num" style={{ width: 80 }}>% IVA</th><th className="num" style={{ width: 90 }}>% deduc.</th><th className="num" style={{ width: 110 }}>Cuota</th><th style={{ width: 40 }} /></tr>
          </thead>
          <tbody>
            {lineas.map((l, f) => {
              const t = ivaDe(l.codigoIva);
              const c = cuotaLinea(l);
              return (
                <tr key={l.clave} className={f % 2 ? "dx-par" : ""}>
                  <td><input value={l.descripcion} onChange={(e) => cambiar(l.clave, { descripcion: e.target.value })} placeholder="Qué se compra" /></td>
                  <td><input list="dx-cuentas-gasto" value={l.cuentaGasto} onChange={(e) => cambiar(l.clave, { cuentaGasto: e.target.value })} placeholder="la de la regla" /></td>
                  <td><input className="num" type="number" step="0.01" value={l.base || ""} onChange={(e) => cambiar(l.clave, { base: Number(e.target.value) })} /></td>
                  <td>
                    <select value={l.codigoIva} onChange={(e) => cambiar(l.clave, { codigoIva: e.target.value, porcentajeIva: null })}>
                      <option value="">General</option>
                      {ivas.map((x) => <option key={x.codigo} value={x.codigo}>{x.nombre}</option>)}
                    </select>
                  </td>
                  <td>
                    {autoliquidable(t?.clase)
                      ? <input className="num" type="number" step="0.01" value={l.porcentajeIva ?? ""} placeholder="21" title="Tipo que autoliquidas" onChange={(e) => cambiar(l.clave, { porcentajeIva: e.target.value === "" ? null : Number(e.target.value) })} />
                      : <span className="muted">{c ? `${num2(c.porcentajeIva)} %` : ""}</span>}
                  </td>
                  <td><input className="num" type="number" step="1" min={0} max={100} value={l.porcentajeDeducible} onChange={(e) => cambiar(l.clave, { porcentajeDeducible: Number(e.target.value) })} /></td>
                  <td className="num">
                    {c ? <><strong>{eur(c.cuota)}</strong>{c.autoliquidada && <div className="dx-sub">autoliquidada</div>}{c.cuotaRecargo !== 0 && <div className="dx-sub">+ recargo {eur(c.cuotaRecargo)}</div>}</> : "—"}
                  </td>
                  <td className="right"><button className="dx-icono" title="Quitar" onClick={() => setLineas((ls) => (ls.length > 1 ? ls.filter((x) => x.clave !== l.clave) : [lineaNueva()]))}>✕</button></td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <datalist id="dx-cuentas-gasto">{cuentas.map((c) => <option key={c.codigo} value={c.codigo}>{c.nombre}</option>)}</datalist>
        <button className="btn small ghost" style={{ marginTop: 8 }} onClick={() => setLineas((ls) => [...ls, lineaNueva(ls[ls.length - 1]?.codigoIva ?? "")])}>+ Añadir línea</button>
        <span className="muted dx-ayuda">Una línea por cada base e impuesto de la factura. En inversión del sujeto pasivo e intracomunitarias el IVA lo autoliquidas tú (no lo cobra el proveedor).</span>
      </div>

      <div className="dx-pie">
        <div className="panel" style={{ margin: 0 }}>
          <div className="panel-head"><h2>Vencimientos</h2>
            <div className="dx-acciones">
              {[2, 3, 4].map((n) => <button key={n} className="btn small secondary" disabled={!calculo} onClick={() => repartir(n)}>{n} plazos</button>)}
              {plazosManual && <button className="btn small ghost" onClick={() => setPlazosManual(null)}>Según forma de pago</button>}
            </div>
          </div>
          {plazosManual ? (
            <>
              {plazosManual.map((p, i) => (
                <div key={i} className="dx-fila" style={{ marginBottom: 6 }}>
                  <input type="date" value={p.fecha} onChange={(e) => setPlazosManual(plazosManual.map((x, j) => (j === i ? { ...x, fecha: e.target.value } : x)))} />
                  <input className="num" type="number" step="0.01" value={p.importe} onChange={(e) => setPlazosManual(plazosManual.map((x, j) => (j === i ? { ...x, importe: Number(e.target.value) } : x)))} />
                </div>
              ))}
              {calculo && sumaPlazos !== calculo.total && <p className="dx-rojo" style={{ margin: 0 }}>Los plazos suman {eur(sumaPlazos)}; la factura, {eur(calculo.total)}.</p>}
            </>
          ) : (
            <p className="muted" style={{ margin: 0 }}>{(calculo?.vencimientos ?? []).map((v) => `${fecha(v.fecha)}: ${eur(v.importe)}`).join(" · ") || "—"}</p>
          )}
          {error && <p className="dx-rojo" style={{ marginBottom: 0 }}>{error}</p>}
        </div>
        <div className="panel dx-totales">
          {(calculo?.desglose ?? []).map((d, i) => (
            <div key={i} className="dx-tot"><span className="muted">{ivaDe(d.codigoIva)?.nombre ?? d.codigoIva} {d.autoliquidada ? `(${num2(d.porcentajeIva)} %, autoliquidado)` : ""} · base {num2(d.base)}</span><span>{eur(d.cuota)}</span></div>
          ))}
          <div className="dx-tot"><span className="muted">Base imponible</span><span>{eur(calculo?.baseImponible)}</span></div>
          <div className="dx-tot"><span className="muted">Impuestos</span><span>{eur(calculo?.cuotaIva)}</span></div>
          {!!calculo?.recargoTotal && <div className="dx-tot"><span className="muted">Recargo de equivalencia</span><span>{eur(calculo.recargoTotal)}</span></div>}
          {!!calculo?.retencionIrpf && <div className="dx-tot"><span className="muted">Retención IRPF</span><span>−{eur(calculo.retencionIrpf)}</span></div>}
          <div className="dx-tot dx-grande"><span>{(calculo?.total ?? 0) < 0 ? "A favor (abono del proveedor)" : "Total a pagar"}</span><span>{eur(calculo?.total)}</span></div>
          {calculo && (calculo.desglose ?? []).some((d) => d.cuotaDeducible !== d.cuota) && (
            <div className="dx-tot"><span className="muted">IVA deducible (antes de prorrata)</span><span className="muted">{eur((calculo.desglose ?? []).reduce((t, d) => t + d.cuotaDeducible, 0))}</span></div>
          )}
        </div>
      </div>
    </div>
  );
}

export function VistaGasto(props: { id: string }) {
  const { api, anfitrion, navegar } = useDocs();
  const [g, setG] = useState<Gasto | null>(null);
  const [saldo, setSaldo] = useState<Saldo | null>(null);
  const [error, setError] = useState("");
  const [anular, setAnular] = useState(false);
  const cargar = () => {
    api.get<Gasto>(`/gastos/${props.id}`).then(setG).catch((e: Error) => setError(e.message));
    api.get<Saldo>(`/gastos/${props.id}/saldo`).then(setSaldo).catch(() => setSaldo(null));
  };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(cargar, [props.id]);
  if (error) return <div className="panel"><p className="dx-rojo">{error}</p></div>;
  if (!g) return <div className="muted">Cargando…</div>;
  const vivo = g.estado === "Registrado";
  const sinPagos = !saldo || saldo.liquidado === 0;
  return (
    <div className="dx-editor">
      <div className="panel">
        <div className="panel-head">
          <h2 style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <button className="btn small secondary" onClick={() => navegar({ tipo: "gasto", pantalla: "lista" })}>←</button>
            Factura <span className="mono">{g.numeroFactura ?? "(sin número)"}</span> <span className={clasePill(g.estado === "Anulado" ? "Anulada" : "Emitida")}>{g.estado}</span>
            {g.esRectificativa && <span className="pill">Rectifica {g.numeroRectificado}</span>}
          </h2>
          <div className="dx-acciones">
            <button className="btn small secondary" onClick={() => navegar({ tipo: "gasto", pantalla: "editor", semilla: { ...g, numeroFactura: null } })}>Duplicar</button>
            {vivo && sinPagos && <button className="btn small secondary" onClick={() => navegar({ tipo: "gasto", pantalla: "editor", id: g.id, semilla: g })}>Corregir</button>}
            {vivo && saldo && saldo.pendiente > 0 && anfitrion.irA && <button className="btn small" onClick={() => anfitrion.irA!("pagos")}>Pagar</button>}
            {vivo && !g.esRectificativa && <button className="btn small secondary" onClick={() => navegar({ tipo: "gasto", pantalla: "editor", semilla: {
              ...g, numeroFactura: null, esRectificativa: true, rectificaGastoId: g.id, numeroRectificado: g.numeroFactura ?? g.concepto, fechaRectificada: g.fechaFactura ?? g.fecha,
              motivoRectificacion: "", vencimientos: null, lineas: g.lineas?.map((l) => ({ ...l, base: -l.base })) } })}>Rectificativa / abono</button>}
            {vivo && sinPagos && <button className="btn small ghost" onClick={() => setAnular(true)}>Anular</button>}
          </div>
        </div>
        <div className="dx-datos">
          <div className="dx-dato"><small>Proveedor</small><div><strong>{g.proveedorTexto}</strong></div></div>
          <div className="dx-dato"><small>Fecha de factura</small><div>{fecha(g.fechaFactura ?? g.fecha)}</div></div>
          <div className="dx-dato"><small>Registro</small><div>{fecha(g.fecha)}</div></div>
          <div className="dx-dato"><small>Pago</small><div>{saldo ? (saldo.pendiente <= 0 ? <span className="pill ok">Pagada</span> : <>Pendiente <strong>{eur(saldo.pendiente)}</strong></>) : "—"}</div></div>
        </div>
        <p className="muted" style={{ marginBottom: 0 }}>{g.concepto}</p>
      </div>
      <div className="panel">
        <table>
          <thead><tr><th>Descripción</th><th>Cuenta</th><th className="num">Base</th><th>Impuesto</th><th className="num">Cuota</th><th className="num">Deducible</th></tr></thead>
          <tbody>{(g.lineas ?? []).map((l, i) => (
            <tr key={i}><td>{l.descripcion ?? ""}</td><td className="mono muted">{l.cuentaGasto ?? "regla"}</td><td className="num">{eur(l.base)}</td>
              <td>{l.codigoIva} · {num2(l.porcentajeIva)} %{l.autoliquidada ? " · autoliquidada" : ""}{l.cuotaRecargo ? ` · recargo ${eur(l.cuotaRecargo)}` : ""}</td>
              <td className="num">{eur(l.cuota)}</td><td className="num muted">{l.porcentajeDeducible !== 100 ? `${num2(l.porcentajeDeducible)} % · ` : ""}{eur(l.cuotaDeducible)}</td></tr>
          ))}</tbody>
        </table>
        <div className="dx-totales-vista">
          <div className="dx-tot"><span className="muted">Base imponible</span><span>{eur(g.baseImponible)}</span></div>
          <div className="dx-tot"><span className="muted">Impuestos</span><span>{eur(g.cuotaIva)}</span></div>
          {!!g.recargoTotal && <div className="dx-tot"><span className="muted">Recargo de equivalencia</span><span>{eur(g.recargoTotal)}</span></div>}
          {!!g.retencionIrpf && <div className="dx-tot"><span className="muted">Retención IRPF ({num2(g.porcentajeIrpf)} %)</span><span>−{eur(g.retencionIrpf)}</span></div>}
          <div className="dx-tot dx-grande"><span>{g.total < 0 ? "A favor (abono del proveedor)" : "Total a pagar"}</span><span>{eur(g.total)}</span></div>
        </div>
        <p className="muted" style={{ fontSize: 12.5 }}>Vencimientos: {(g.vencimientos ?? []).map((v) => `${fecha(v.fecha)} ${eur(v.importe)}`).join(" · ")}</p>
      </div>
      {anular && (
        <Dialogo titulo="Anular la factura" alCerrar={() => setAnular(false)}
          acciones={<><button className="btn small secondary" onClick={() => setAnular(false)}>Cancelar</button>
            <button className="btn small" onClick={async () => { try { await api.post(`/gastos/${g.id}/anular`); anfitrion.aviso("Factura anulada.", "ok"); setAnular(false); cargar(); } catch (e) { anfitrion.aviso((e as Error).message, "err"); } }}>Anular</button></>}>
          <p style={{ margin: 0 }}>Sale de los libros de IVA y de las declaraciones, y se contabiliza su contraasiento. Si solo hay que cambiar algo, mejor «Corregir».</p>
        </Dialogo>
      )}
    </div>
  );
}
