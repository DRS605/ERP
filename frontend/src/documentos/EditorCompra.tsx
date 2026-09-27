/**
 * Editor del pedido de compra: proveedor, líneas con artículo (precio de compra del artículo por su unidad de compra),
 * conceptos (pronto pago, portes de otro transportista, aranceles…) y el coste con que entrará cada línea en el almacén.
 */
import { useEffect, useMemo, useState } from "react";
import { useDocs } from "./contexto";
import { EditorConceptos, SelectorTercero } from "./Componentes";
import { Rejilla, lineaVacia, type CalculoLinea } from "./Rejilla";
import { conceptosDeSemilla } from "./EditorVenta";
import type { ConceptoCatalogo, ConceptoSolicitado, LineaEdicion, PedidoCompra, Producto, Tercero } from "./tipos";
import { eur, hoyIso, nuevaClave, useRetardado, useUltimaPeticion } from "./util";

export function EditorCompra(props: { id?: string | null; semilla?: PedidoCompra; alGuardar: (id: string) => void; alCancelar: () => void }) {
  const { api, anfitrion } = useDocs();
  const [proveedores, setProveedores] = useState<Tercero[]>([]);
  const [catalogo, setCatalogo] = useState<ConceptoCatalogo[]>([]);
  const [proveedorId, setProveedorId] = useState(props.semilla?.proveedorId ?? "");
  const [fecha, setFecha] = useState(props.semilla?.fecha ?? hoyIso());
  const [lineas, setLineas] = useState<LineaEdicion[]>([lineaVacia()]);
  const [conceptosDoc, setConceptosDoc] = useState<ConceptoSolicitado[]>([]);
  const [calculo, setCalculo] = useState<PedidoCompra | null>(null);
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);
  const ultima = useUltimaPeticion();

  useEffect(() => {
    api.get<Tercero[]>("/proveedores").then(setProveedores).catch(() => setProveedores([]));
    api.get<ConceptoCatalogo[]>("/conceptos-linea?ambito=Compras&activos=true").then(setCatalogo).catch(() => setCatalogo([]));
  }, [api]);

  useEffect(() => {
    const s = props.semilla;
    if (!s) return;
    const semilla = s.lineas.map((l) => ({ ...l, porcentajeDescuento: 0, codigoIva: "" }));
    const { porLinea, documento } = conceptosDeSemilla(semilla);
    const base: LineaEdicion[] = s.lineas.map((l, i) => ({ clave: nuevaClave(), productoId: l.productoId ?? null, descripcion: l.descripcion, cantidad: l.cantidad, precio: l.precioUnitario, dto: 0, iva: null, conceptos: porLinea[i] }));
    setLineas(base);
    setConceptosDoc(documento);
    Promise.all(base.map((l) => (l.productoId ? api.get<Producto>(`/productos/${l.productoId}`).catch(() => null) : Promise.resolve(null)))).then((ps) =>
      setLineas((ls) => ls.map((l, i) => (ps[i] ? { ...l, referencia: ps[i]!.referencia ?? ps[i]!.nombre, unidad: ps[i]!.unidadCompra || ps[i]!.unidad, stock: ps[i]!.stock, controlarStock: ps[i]!.controlarStock } : l))),
    );
  }, [props.semilla, api]);

  const proveedor = proveedores.find((p) => p.id === proveedorId);
  const validas = useMemo(() => lineas.map((l, i) => ({ l, i })).filter(({ l }) => l.descripcion.trim() && l.cantidad > 0), [lineas]);
  const comando = useMemo(
    () => ({
      proveedorId: proveedorId || null,
      proveedorTexto: proveedor?.nombre ?? (props.semilla?.proveedorTexto || null),
      solicitudOrigenId: props.id ? null : props.semilla?.solicitudOrigenId ?? null,
      fecha,
      conceptosDocumento: conceptosDoc,
      lineas: validas.map(({ l }) => ({ descripcion: l.descripcion.trim(), cantidad: l.cantidad, precioUnitario: l.precio ?? 0, productoId: l.productoId, ...(l.conceptos === undefined ? {} : { conceptos: l.conceptos }) })),
    }),
    [proveedorId, proveedor, fecha, conceptosDoc, validas, props.id, props.semilla],
  );
  const retardado = useRetardado(comando, 350);

  useEffect(() => {
    if (!retardado.proveedorId || retardado.lineas.length === 0) {
      setCalculo(null);
      setError(retardado.proveedorId ? "" : "Elige el proveedor.");
      return;
    }
    const vigente = ultima();
    api
      .post<PedidoCompra>("/compras/pedidos/simular", retardado)
      .then((p) => vigente() && (setCalculo(p), setError("")))
      .catch((e: Error) => vigente() && (setCalculo(null), setError(e.message)));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [retardado, api]);

  const calculos: (CalculoLinea | undefined)[] = useMemo(() => {
    const r: (CalculoLinea | undefined)[] = lineas.map(() => undefined);
    validas.forEach(({ i }, k) => {
      const c = calculo?.lineas[k];
      if (c) r[i] = { precio: c.precioUnitario, importe: c.importe, costeUnitarioEntrada: c.costeUnitarioEntrada, conceptos: c.conceptos };
    });
    return r;
  }, [calculo, lineas, validas]);
  const sugeridos = useMemo(() => {
    const r: Record<string, ConceptoSolicitado[]> = {};
    lineas.forEach((l, i) => (r[l.clave] = (calculos[i]?.conceptos ?? []).filter((c) => !c.repartido).map((c) => ({ conceptoId: c.conceptoId, valor: c.valor }))));
    return r;
  }, [lineas, calculos]);

  function elegirArticulo(clave: string, p: Producto) {
    const precio = p.precioCompraPorUnidadCompra ?? p.precioCompra;
    setLineas((ls) => ls.map((l) => (l.clave === clave ? { ...l, productoId: p.id, referencia: p.referencia ?? p.nombre, descripcion: p.nombre, precio, conceptos: undefined, unidad: p.unidadCompra || p.unidad, stock: p.stock, controlarStock: p.controlarStock } : l)));
  }

  async function guardar() {
    setGuardando(true);
    try {
      const r = props.id ? await api.put<{ id: string }>(`/compras/pedidos/${props.id}`, comando) : await api.post<{ id: string }>("/compras/pedidos", comando);
      anfitrion.aviso("Pedido de compra guardado.", "ok");
      props.alGuardar(r.id);
    } catch (e) {
      anfitrion.aviso((e as Error).message, "err");
    } finally {
      setGuardando(false);
    }
  }

  const costeConceptos = (calculo?.lineas ?? []).reduce((t, l) => t + l.costeConceptos, 0);
  return (
    <div className="dx-editor">
      <div className="panel">
        <div className="panel-head">
          <h2>{props.id ? `Editar pedido ${props.semilla?.numeroCompleto ?? ""}` : "Nuevo pedido de compra"}</h2>
          <div style={{ display: "flex", gap: 8 }}>
            <button className="btn small secondary" onClick={props.alCancelar}>Cancelar</button>
            <button className="btn small" disabled={!calculo || guardando} onClick={guardar}>Guardar</button>
          </div>
        </div>
        <div className="dx-cabecera">
          <div className="dx-cab-campos">
            <SelectorTercero terceros={proveedores} valor={proveedorId} alCambiar={setProveedorId} etiqueta="Proveedor" deshabilitado={!!props.id} />
            <div className="dx-fila">
              <div>
                <label>Fecha del pedido</label>
                <input type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} />
              </div>
            </div>
          </div>
          <div className="dx-ficha">
            {proveedor ? (
              <>
                <strong>{proveedor.nombre}</strong>
                <div className="muted">{[proveedor.nifFiscal, proveedor.poblacion, proveedor.pais].filter(Boolean).join(" · ")}</div>
              </>
            ) : (
              <span className="muted">Elige un proveedor: se aplican sus conceptos (pronto pago, portes…).</span>
            )}
          </div>
        </div>
      </div>
      <div className="panel">
        <Rejilla modo="compra" lineas={lineas} alCambiar={setLineas} calculos={calculos} ivas={[]} catalogo={catalogo} sugeridos={sugeridos} alElegirArticulo={elegirArticulo} />
        {catalogo.length > 0 && (
          <div style={{ marginTop: 10 }}>
            <EditorConceptos catalogo={catalogo} lista={conceptosDoc} alCambiar={(l) => setConceptosDoc(l ?? [])} documento />
          </div>
        )}
      </div>
      <div className="dx-pie">
        <div className="dx-estado">{error && <span className="dx-rojo">{error}</span>}</div>
        <div className="panel dx-totales">
          <div className="dx-tot dx-grande"><span>Total del pedido (sin impuestos)</span><span>{eur(calculo?.total)}</span></div>
          {costeConceptos !== 0 && <div className="dx-tot"><span className="muted">Costes añadidos al almacén</span><span>{eur(costeConceptos)}</span></div>}
          <div className="muted" style={{ fontSize: 12, marginTop: 6 }}>El impuesto se aplica al facturar el pedido.</div>
        </div>
      </div>
    </div>
  );
}
