/**
 * Editor de documentos de venta: presupuesto, pedido y factura (y rectificativa). Cabecera con el cliente y sus datos,
 * rejilla de líneas y totales. Cada cambio se calcula en el servidor sin guardar (tarifa del cliente, conceptos,
 * impuestos, recargo, retención, margen y riesgo), así que lo que se ve es lo que se emitirá.
 */
import { useEffect, useMemo, useState } from "react";
import { useDocs } from "./contexto";
import { Dialogo, EditorConceptos, SelectorTercero } from "./Componentes";
import { Rejilla, lineaVacia, type CalculoLinea } from "./Rejilla";
import { anticiposDisponibles, tipoEquivalente, anticiposFacturados, repartoAnticipos, type Anticipo, type ConceptoAplicado, type ConceptoCatalogo, type ConceptoSolicitado, type Factura, type FormaPago, type LineaEdicion, type Producto, type Tercero, type TipoIva, type TipoVenta } from "./tipos";
import { eur, hoyIso, nuevaClave, num2, redondear2, useRetardado, useUltimaPeticion } from "./util";

export const NOMBRE_TIPO: Record<TipoVenta, string> = { presupuesto: "presupuesto", pedido: "pedido de venta", factura: "factura" };

/** Semilla para abrir el editor desde un documento existente (editar, duplicar, rectificar, pasar a otro). */
export interface SemillaVenta {
  clienteId?: string;
  fecha?: string;
  lineas: {
    productoId?: string | null;
    descripcion: string;
    cantidad: number;
    precioUnitario: number;
    porcentajeDescuento: number;
    codigoIva: string;
    conceptos?: ConceptoAplicado[] | null;
  }[];
  rectificaId?: string;
  rectificaNumero?: string;
}

/** Conceptos propios de cada línea (sin los repartidos) y los del documento (agrupados). */
export function conceptosDeSemilla(lineas: SemillaVenta["lineas"]) {
  const documento = new Map<string, ConceptoSolicitado>();
  for (const l of lineas)
    for (const c of l.conceptos ?? [])
      if (c.repartido) {
        const x = documento.get(c.conceptoId);
        documento.set(c.conceptoId, { conceptoId: c.conceptoId, valor: c.calculo === "Importe" ? redondear2((x?.valor ?? 0) + c.valor) : c.valor });
      }
  return {
    porLinea: lineas.map((l) => (l.conceptos ?? []).filter((c) => !c.repartido).map((c) => ({ conceptoId: c.conceptoId, valor: c.valor }))),
    documento: [...documento.values()],
  };
}

export function EditorVenta(props: { tipo: TipoVenta; id?: string | null; semilla?: SemillaVenta; alGuardar: (id: string) => void; alCancelar: () => void }) {
  const { api, anfitrion } = useDocs();
  const rectificativa = !!props.semilla?.rectificaId;
  const [clientes, setClientes] = useState<Tercero[]>([]);
  const [ivas, setIvas] = useState<TipoIva[]>([]);
  const [formas, setFormas] = useState<FormaPago[]>([]);
  const [series, setSeries] = useState<string[]>([]);
  const [catalogo, setCatalogo] = useState<ConceptoCatalogo[]>([]);

  const [clienteId, setClienteId] = useState(props.semilla?.clienteId ?? "");
  const [fecha, setFecha] = useState(props.tipo === "pedido" && props.semilla?.fecha ? props.semilla.fecha : hoyIso());
  const [serie, setSerie] = useState("");
  const [formaPagoId, setFormaPagoId] = useState("");
  const [dias, setDias] = useState(0);
  const [recargo, setRecargo] = useState(false);
  const [irpf, setIrpf] = useState<number | null>(null);
  const [validez, setValidez] = useState(30);
  const [motivo, setMotivo] = useState("");
  const [lineas, setLineas] = useState<LineaEdicion[]>([lineaVacia()]);
  const [conceptosDoc, setConceptosDoc] = useState<ConceptoSolicitado[]>([]);
  // Empresa con actividad en la Península y en Canarias (mismo NIF): cada factura va con IVA o con IGIC.
  const [ambos, setAmbos] = useState(false);
  // Un documento de partida (editar, duplicar, convertir) conserva el impuesto de sus líneas.
  const territorioSemilla: "Iva" | "Igic" | null = props.semilla?.lineas.some((l) => /^(IGIC|REAGPIGIC)/i.test(l.codigoIva ?? "")) ? "Igic"
    : props.semilla?.lineas.length ? "Iva" : null;
  const [territorio, setTerritorio] = useState<"Iva" | "Igic">(territorioSemilla ?? "Iva");

  const [calculo, setCalculo] = useState<Factura | null>(null);
  const [errorCalculo, setErrorCalculo] = useState("");
  const [calculando, setCalculando] = useState(false);
  const [guardando, setGuardando] = useState(false);
  const [confirmar, setConfirmar] = useState(false);
  // Anticipos del cliente pendientes de aplicar: se avisa y, al emitir la factura, se aplican (asiento 438 a 430).
  const [anticipos, setAnticipos] = useState<Anticipo[]>([]);
  const [aplicarAnticipos, setAplicarAnticipos] = useState(true);
  // Anticipos con factura (su IVA ya declarado): se descuentan en esta factura con una línea negativa (base e IVA).
  const [facturados, setFacturados] = useState<Anticipo[]>([]);
  const [descontar, setDescontar] = useState(true);
  const ultima = useUltimaPeticion();

  // Catálogos.
  useEffect(() => {
    api.get<Tercero[]>("/clientes").then(setClientes).catch(() => setClientes([]));
    api.get<TipoIva[]>("/tipos-iva").then((t) => setIvas(t.filter((x) => x.activo))).catch(() => setIvas([]));
    api.get<FormaPago[]>("/formas-pago").then((f) => setFormas(f.filter((x) => x.activo))).catch(() => setFormas([]));
    api.get<{ prefijo: string; tipoDocumento: string }[]>("/series").then((s) => setSeries([...new Set(s.filter((x) => x.tipoDocumento === "Factura").map((x) => x.prefijo))])).catch(() => setSeries([]));
    api.get<ConceptoCatalogo[]>("/conceptos-linea?ambito=Ventas&activos=true").then(setCatalogo).catch(() => setCatalogo([]));
    api.get<{ territorioFiscal: string; operaEnAmbosTerritorios?: boolean }>("/empresas/actual")
      .then((e) => { setAmbos(!!e.operaEnAmbosTerritorios); setTerritorio(territorioSemilla ?? (e.territorioFiscal === "Canarias" ? "Igic" : "Iva")); })
      .catch(() => setAmbos(false));
  }, [api]);

  // Al cambiar de territorio, las líneas pasan al tipo equivalente del otro impuesto (las de artículo, al del artículo).
  function cambiarTerritorio(t: "Iva" | "Igic") {
    setTerritorio(t);
    setLineas((ls) => ls.map((l) => ({ ...l, iva: l.productoId ? null : tipoEquivalente(l.iva, t) })));
  }
  const ivasTerritorio = useMemo(() => (ambos ? ivas.filter((t) => (t.impuesto ?? "Iva") === territorio) : ivas), [ambos, ivas, territorio]);

  // Documento de partida (editar, duplicar, rectificar…): líneas con sus precios fijados y sus conceptos.
  useEffect(() => {
    const s = props.semilla;
    if (!s || !s.lineas.length) return;
    const { porLinea, documento } = conceptosDeSemilla(s.lineas);
    const base: LineaEdicion[] = s.lineas.map((l, i) => ({
      clave: nuevaClave(),
      productoId: l.productoId ?? null,
      descripcion: l.descripcion,
      cantidad: l.cantidad,
      precio: l.precioUnitario,
      dto: l.porcentajeDescuento,
      iva: l.codigoIva,
      conceptos: rectificativa ? [] : porLinea[i],
    }));
    setLineas(base);
    setConceptosDoc(rectificativa ? [] : documento);
    // Referencia, unidad y existencias de los artículos.
    Promise.all(base.map((l) => (l.productoId ? api.get<Producto>(`/productos/${l.productoId}`).catch(() => null) : Promise.resolve(null)))).then((ps) =>
      setLineas((ls) => ls.map((l, i) => (ps[i] ? { ...l, referencia: ps[i]!.referencia ?? ps[i]!.nombre, unidad: ps[i]!.unidad, stock: ps[i]!.stock, controlarStock: ps[i]!.controlarStock } : l))),
    );
  }, [props.semilla, api, rectificativa]);

  const cliente = clientes.find((c) => c.id === clienteId);
  useEffect(() => {
    if (props.tipo !== "factura" || rectificativa || !clienteId) {
      setAnticipos([]);
      setFacturados([]);
      return;
    }
    api.get<Anticipo[]>(`/anticipos?clienteId=${clienteId}`)
      .then((l) => { setAnticipos(anticiposDisponibles(l)); setFacturados(anticiposFacturados(l)); })
      .catch(() => { setAnticipos([]); setFacturados([]); });
  }, [api, clienteId, props.tipo, rectificativa]);
  const disponibleAnticipos = redondear2(anticipos.reduce((t, a) => t + a.disponible, 0));

  // Al elegir cliente: sus condiciones por defecto.
  useEffect(() => {
    if (!cliente) return;
    setRecargo(!!cliente.recargoEquivalencia);
    if (cliente.formaPagoDefectoId) setFormaPagoId(cliente.formaPagoDefectoId);
  }, [cliente]);

  const validas = useMemo(() => lineas.map((l, i) => ({ l, i })).filter(({ l }) => (l.productoId || l.descripcion.trim()) && l.cantidad > 0), [lineas]);

  const comando = useMemo(
    () => ({
      clienteId,
      fechaEmision: props.tipo === "factura" ? fecha : null,
      serie: serie || null,
      diasVencimiento: dias,
      formaPagoId: formaPagoId || null,
      recargoEquivalencia: recargo,
      porcentajeIrpf: irpf,
      conceptosDocumento: conceptosDoc,
      impuesto: ambos && !rectificativa ? territorio : null,
      descontarAnticipos: descontar && facturados.length ? facturados.map((a) => ({ anticipoId: a.id })) : null,
      lineas: validas.map(({ l }) => ({
        cantidad: l.cantidad,
        descripcion: l.descripcion.trim() || null,
        precioUnitario: l.precio,
        codigoIva: l.iva,
        porcentajeDescuento: l.dto,
        productoId: l.productoId,
        ...(rectificativa ? { conceptos: [] } : l.conceptos === undefined ? {} : { conceptos: l.conceptos }),
      })),
    }),
    [clienteId, fecha, serie, dias, formaPagoId, recargo, irpf, conceptosDoc, validas, props.tipo, rectificativa, descontar, facturados, ambos, territorio],
  );
  const comandoRetardado = useRetardado(comando, 350);

  useEffect(() => {
    if (!comandoRetardado.clienteId || comandoRetardado.lineas.length === 0) {
      setCalculo(null);
      setErrorCalculo(comandoRetardado.clienteId ? "" : "Elige el cliente para calcular precios e importes.");
      return;
    }
    const vigente = ultima();
    setCalculando(true);
    api
      .post<Factura>("/facturas/simular", comandoRetardado)
      .then((f) => vigente() && (setCalculo(f), setErrorCalculo("")))
      .catch((e: Error) => vigente() && (setCalculo(null), setErrorCalculo(e.message)))
      .finally(() => vigente() && setCalculando(false));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [comandoRetardado, api]);

  // Cálculo del servidor de cada línea de la rejilla (por posición entre las válidas).
  const calculos: (CalculoLinea | undefined)[] = useMemo(() => {
    const r: (CalculoLinea | undefined)[] = lineas.map(() => undefined);
    if (!calculo) return r;
    validas.forEach(({ i }, k) => {
      const c = calculo.lineas[k];
      if (c) r[i] = { precio: c.precioUnitario, dto: c.porcentajeDescuento, iva: c.codigoIva, importe: c.base, margen: c.productoId || c.costeUnitario || c.costeConceptos ? c.margen : undefined, conceptos: c.conceptos };
    });
    return r;
  }, [calculo, lineas, validas]);

  const sugeridos = useMemo(() => {
    const r: Record<string, ConceptoSolicitado[]> = {};
    lineas.forEach((l, i) => (r[l.clave] = (calculos[i]?.conceptos ?? []).filter((c) => !c.repartido).map((c) => ({ conceptoId: c.conceptoId, valor: c.valor }))));
    return r;
  }, [lineas, calculos]);

  function elegirArticulo(clave: string, p: Producto) {
    setLineas((ls) =>
      ls.map((l) =>
        l.clave === clave
          ? { ...l, productoId: p.id, referencia: p.referencia ?? p.nombre, descripcion: p.nombre, precio: null, dto: 0, iva: null, conceptos: undefined, unidad: p.unidad, stock: p.stock, controlarStock: p.controlarStock }
          : l,
      ),
    );
  }

  // Desglose por tipo de impuesto y margen.
  const desglose = useMemo(() => {
    const m = new Map<string, { base: number; cuota: number; pct: number }>();
    for (const l of calculo?.lineas ?? []) {
      const x = m.get(l.codigoIva) ?? { base: 0, cuota: 0, pct: l.porcentajeIva };
      x.base += l.base;
      x.cuota += l.cuotaIva;
      m.set(l.codigoIva, x);
    }
    return [...m.entries()];
  }, [calculo]);
  const coste = (calculo?.lineas ?? []).reduce((t, l) => t + (l.base - l.margen), 0);
  const margen = calculo ? calculo.baseImponible - coste : 0;
  const nombreIva = (codigo: string) => ivas.find((t) => t.codigo === codigo)?.nombre ?? codigo;

  async function guardar() {
    if (!calculo) return;
    setGuardando(true);
    try {
      // Se guarda lo calculado (precio, descuento e impuesto fijados), para que el documento sea el que se ha visto.
      const lineasFijas = validas.map(({ l }, k) => {
        const c = calculo.lineas[k];
        return {
          cantidad: l.cantidad,
          descripcion: c.descripcion,
          precioUnitario: c.precioUnitario,
          codigoIva: c.codigoIva,
          porcentajeDescuento: c.porcentajeDescuento,
          productoId: l.productoId,
          ...(rectificativa ? {} : l.conceptos === undefined ? {} : { conceptos: l.conceptos }),
        };
      });
      let id: string;
      if (rectificativa) {
        id = (await api.post<{ id: string }>(`/facturas/${props.semilla!.rectificaId}/rectificar`, { motivo, lineas: lineasFijas, fechaEmision: fecha, porcentajeIrpf: irpf, serie: serie || null })).id;
      } else if (props.tipo === "factura") {
        const emitida = await api.post<{ id: string; total: number; avisoRiesgo?: string }>("/facturas", { ...comando, lineas: lineasFijas });
        id = emitida.id;
        if (aplicarAnticipos && anticipos.length) {
          let aplicado = 0;
          try {
            for (const r of repartoAnticipos(anticipos, emitida.total)) {
              await api.post(`/anticipos/${r.id}/aplicar`, { facturaId: id, importe: r.importe });
              aplicado += r.importe;
            }
            anfitrion.aviso(`Factura emitida. Aplicados ${eur(aplicado)} de anticipos (asiento 438 a 430).`, "ok");
          } catch (e) {
            anfitrion.aviso(`Factura emitida, pero no se pudo aplicar el anticipo: ${(e as Error).message}`, "err");
          }
          props.alGuardar(id);
          return;
        }
      } else if (props.tipo === "presupuesto") {
        const cuerpo = { clienteId, diasValidez: validez, lineas: lineasFijas, conceptosDocumento: conceptosDoc, impuesto: comando.impuesto };
        id = props.id ? (await api.put<{ id: string }>(`/presupuestos/${props.id}`, cuerpo)).id : (await api.post<{ id: string }>("/presupuestos", cuerpo)).id;
      } else {
        const cuerpo = { clienteId, fecha, lineas: lineasFijas, conceptosDocumento: conceptosDoc, impuesto: comando.impuesto };
        id = props.id ? (await api.put<{ id: string }>(`/pedidos-venta/${props.id}`, cuerpo)).id : (await api.post<{ id: string }>("/pedidos-venta", cuerpo)).id;
      }
      anfitrion.aviso(props.tipo === "factura" ? "Factura emitida." : "Documento guardado.", "ok");
      props.alGuardar(id);
    } catch (e) {
      anfitrion.aviso((e as Error).message, "err");
    } finally {
      setGuardando(false);
      setConfirmar(false);
    }
  }

  const titulo = rectificativa
    ? `Rectificativa de la factura ${props.semilla?.rectificaNumero ?? ""}`
    : `${props.id ? "Editar" : "Nuevo"} ${NOMBRE_TIPO[props.tipo]}`.replace("Nuevo factura", "Nueva factura");
  const puedeGuardar = !!calculo && !calculando && (!rectificativa || motivo.trim().length > 0);

  return (
    <div className="dx-editor">
      <div className="panel">
        <div className="panel-head">
          <h2>{titulo}</h2>
          <div style={{ display: "flex", gap: 8 }}>
            <button className="btn small secondary" onClick={props.alCancelar}>Cancelar</button>
            <button className="btn small" disabled={!puedeGuardar || guardando} onClick={() => (props.tipo === "factura" ? setConfirmar(true) : guardar())}>
              {props.tipo === "factura" ? "Emitir factura" : "Guardar"}
            </button>
          </div>
        </div>
        <div className="dx-cabecera">
          <div className="dx-cab-campos">
            <SelectorTercero terceros={clientes} valor={clienteId} alCambiar={setClienteId} etiqueta="Cliente" deshabilitado={rectificativa || (props.tipo === "pedido" && !!props.id && false)} />
            <div className="dx-fila">
              {props.tipo !== "presupuesto" && (
                <div>
                  <label>{props.tipo === "factura" ? "Fecha de emisión" : "Fecha del pedido"}</label>
                  <input type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} />
                </div>
              )}
              {props.tipo === "presupuesto" && (
                <div>
                  <label>Validez</label>
                  <select value={validez} onChange={(e) => setValidez(Number(e.target.value))}>
                    {[15, 30, 60, 90].map((d) => <option key={d} value={d}>{d} días</option>)}
                  </select>
                </div>
              )}
              {ambos && !rectificativa && (
                <div>
                  <label>Territorio de la operación</label>
                  <select value={territorio} onChange={(e) => cambiarTerritorio(e.target.value as "Iva" | "Igic")}>
                    <option value="Iva">Península y Baleares · IVA</option>
                    <option value="Igic">Canarias · IGIC</option>
                  </select>
                </div>
              )}
              {props.tipo === "factura" && series.length > 0 && (
                <div>
                  <label>Serie</label>
                  <select value={serie} onChange={(e) => setSerie(e.target.value)}>
                    <option value="">La del cliente</option>
                    {series.map((s) => <option key={s} value={s}>{s}</option>)}
                  </select>
                </div>
              )}
              {props.tipo === "factura" && !rectificativa && (
                <>
                  <div>
                    <label>Forma de pago</label>
                    <select value={formaPagoId} onChange={(e) => setFormaPagoId(e.target.value)}>
                      <option value="">— Vencimiento a mano —</option>
                      {formas.map((f) => <option key={f.id} value={f.id}>{f.nombre}</option>)}
                    </select>
                  </div>
                  {!formaPagoId && (
                    <div>
                      <label>Vencimiento</label>
                      <select value={dias} onChange={(e) => setDias(Number(e.target.value))}>
                        {[0, 15, 30, 45, 60, 90].map((d) => <option key={d} value={d}>{d ? `${d} días` : "Contado"}</option>)}
                      </select>
                    </div>
                  )}
                </>
              )}
              {props.tipo === "factura" && (
                <div>
                  <label>Retención IRPF %</label>
                  <input type="number" step="0.01" value={irpf ?? ""} placeholder={String(cliente?.porcentajeIrpfDefecto ?? 0)} onChange={(e) => setIrpf(e.target.value === "" ? null : Number(e.target.value))} />
                </div>
              )}
            </div>
            {props.tipo === "factura" && !rectificativa && (
              <label className="dx-check">
                <input type="checkbox" checked={recargo} onChange={(e) => setRecargo(e.target.checked)} /> Recargo de equivalencia
              </label>
            )}
            {rectificativa && (
              <div>
                <label>Motivo de la rectificación (obligatorio)</label>
                <input value={motivo} onChange={(e) => setMotivo(e.target.value)} placeholder="Devolución, error en precio…" />
              </div>
            )}
          </div>
          <div className="dx-ficha">
            {cliente ? (
              <>
                <strong>{cliente.nombre}</strong>
                <div className="muted">{[cliente.nifFiscal, cliente.poblacion, cliente.provincia].filter(Boolean).join(" · ")}</div>
                {cliente.limiteRiesgo != null && <div className="muted">Límite de riesgo {eur(cliente.limiteRiesgo)}</div>}
                {cliente.tarifaId && <div className="muted">Con tarifa de precios propia</div>}
                {cliente.recargoEquivalencia && <div className="muted">En recargo de equivalencia</div>}
                {calculo?.avisoRiesgo && <div className="dx-aviso">⚠ {calculo.avisoRiesgo}</div>}
                {facturados.length > 0 && (
                  <div className="dx-anticipo">
                    <div>🧾 Anticipos facturados pendientes de descontar: <strong>{eur(facturados.reduce((t, a) => t + (a.disponibleBase ?? 0), 0))}</strong> de base ({facturados.map((a) => a.facturaNumero).join(", ")}).</div>
                    <label className="dx-check">
                      <input type="checkbox" checked={descontar} onChange={(e) => setDescontar(e.target.checked)} />
                      Descontar en esta factura (línea negativa con su base e IVA; hasta la base de la factura)
                    </label>
                  </div>
                )}
                {disponibleAnticipos > 0 && (
                  <div className="dx-anticipo">
                    <div>💶 Tiene <strong>{eur(disponibleAnticipos)}</strong> en {anticipos.length === 1 ? "un anticipo pendiente" : `${anticipos.length} anticipos pendientes`} de aplicar.</div>
                    <label className="dx-check">
                      <input type="checkbox" checked={aplicarAnticipos} onChange={(e) => setAplicarAnticipos(e.target.checked)} />
                      Aplicarlo al emitir{calculo ? ` (${eur(Math.min(disponibleAnticipos, calculo.total))})` : ""}
                    </label>
                  </div>
                )}
              </>
            ) : (
              <span className="muted">Elige un cliente: se aplican su tarifa, su forma de pago, su recargo y sus conceptos.</span>
            )}
          </div>
        </div>
      </div>

      <div className="panel">
        <Rejilla modo="venta" lineas={lineas} alCambiar={setLineas} calculos={calculos} ivas={ivasTerritorio} catalogo={rectificativa ? [] : catalogo} sugeridos={sugeridos} alElegirArticulo={elegirArticulo} />
        {!rectificativa && catalogo.length > 0 && (
          <div style={{ marginTop: 10 }}>
            <EditorConceptos catalogo={catalogo} lista={conceptosDoc} alCambiar={(l) => setConceptosDoc(l ?? [])} documento />
          </div>
        )}
      </div>

      <div className="dx-pie">
        <div className="dx-estado">
          {calculando && <span className="muted">Calculando…</span>}
          {!calculando && errorCalculo && <span className="dx-rojo">{errorCalculo}</span>}
          {calculo?.mencionFiscal && <div className="muted" style={{ fontSize: 12 }}>{calculo.mencionFiscal}</div>}
        </div>
        <div className="panel dx-totales">
          {desglose.map(([codigo, x]) => (
            <div key={codigo} className="dx-tot"><span className="muted">{nombreIva(codigo)} · base {num2(x.base)}</span><span>{eur(x.cuota)}</span></div>
          ))}
          {calculo?.lineas.filter((l) => l.anticipoId).map((l) => (
            <div key={l.anticipoId} className="dx-tot dx-tot-anticipo"><span className="muted">{l.descripcion}</span><span>{eur(l.base + l.cuotaIva + l.cuotaRecargo)}</span></div>
          ))}
          <div className="dx-tot"><span className="muted">Base imponible</span><span>{eur(calculo?.baseImponible)}</span></div>
          <div className="dx-tot"><span className="muted">Impuestos</span><span>{eur(calculo?.cuotaIva)}</span></div>
          {!!calculo?.recargoTotal && <div className="dx-tot"><span className="muted">Recargo de equivalencia</span><span>{eur(calculo.recargoTotal)}</span></div>}
          {!!calculo?.retencionIrpf && <div className="dx-tot"><span className="muted">Retención IRPF ({num2(calculo.porcentajeIrpf)} %)</span><span>−{eur(calculo.retencionIrpf)}</span></div>}
          <div className="dx-tot dx-grande"><span>Total</span><span>{eur(calculo?.total)}</span></div>
          {calculo && coste > 0 && (
            <div className="dx-tot"><span className="muted">Coste · margen</span><span className={margen < 0 ? "dx-rojo" : "muted"}>{eur(coste)} · {eur(margen)} ({num2(calculo.baseImponible ? (margen / calculo.baseImponible) * 100 : 0)} %)</span></div>
          )}
        </div>
      </div>

      {confirmar && calculo && (
        <Dialogo titulo={rectificativa ? "Emitir la rectificativa" : "Emitir la factura"} alCerrar={() => setConfirmar(false)}
          acciones={<><button className="btn small secondary" onClick={() => setConfirmar(false)}>Revisar</button><button className="btn small" disabled={guardando} onClick={guardar}>Emitir {eur(calculo.total)}</button></>}>
          <p style={{ margin: 0 }}>
            Se emitirá una factura {rectificativa ? "rectificativa " : ""}de <strong>{eur(calculo.total)}</strong> a <strong>{cliente?.nombre}</strong> con fecha {fecha.split("-").reverse().join("/")}.
            Una factura emitida no se modifica: queda numerada y encadenada en VeriFactu; para corregirla hay que rectificarla o anularla.
          </p>
        </Dialogo>
      )}
    </div>
  );
}
