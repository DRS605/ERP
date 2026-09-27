/** Enrutado interno del módulo: lista, editor o vista de cada tipo de documento. */
import { useMemo, useRef, useState } from "react";
import { Contexto, crearApiAnfitrion, type Anfitrion, type Ruta } from "./contexto";
import { Listado } from "./Listados";
import { EditorVenta, type SemillaVenta } from "./EditorVenta";
import { EditorCompra } from "./EditorCompra";
import { VistaFactura, VistaPedidoCompra, VistaPedidoVenta, VistaPresupuesto } from "./Vistas";
import { EditorGasto, VistaGasto } from "./Gastos";
import type { Gasto, PedidoCompra } from "./tipos";

export function DocumentosApp(props: { anfitrion: Anfitrion; inicial: Ruta }) {
  const [ruta, setRuta] = useState<Ruta>(props.inicial);
  const visitas = useRef(0);
  const [visita, setVisita] = useState(0);
  const api = useMemo(() => crearApiAnfitrion(props.anfitrion), [props.anfitrion]);
  const navegar = (r: Ruta) => {
    setRuta(r);
    setVisita(++visitas.current);
    window.scrollTo({ top: 0 });
  };
  const ctx = { api, anfitrion: props.anfitrion, ruta, navegar };
  // Cada navegación monta la pantalla de nuevo (estado limpio), también al volver a la misma.
  const clave = `${ruta.tipo}-${ruta.pantalla}-${ruta.id ?? ""}-${visita}`;

  let pantalla;
  if (ruta.pantalla === "lista") pantalla = <Listado tipo={ruta.tipo} />;
  else if (ruta.pantalla === "vista" && ruta.id) {
    pantalla =
      ruta.tipo === "factura" ? <VistaFactura id={ruta.id} />
      : ruta.tipo === "presupuesto" ? <VistaPresupuesto id={ruta.id} />
      : ruta.tipo === "pedido" ? <VistaPedidoVenta id={ruta.id} />
      : ruta.tipo === "gasto" ? <VistaGasto id={ruta.id} />
      : <VistaPedidoCompra id={ruta.id} />;
  } else if (ruta.tipo === "gasto") {
    pantalla = <EditorGasto id={ruta.id} semilla={ruta.semilla as Gasto | undefined}
      alGuardar={(id) => navegar({ tipo: "gasto", pantalla: "vista", id })}
      alCancelar={() => navegar(ruta.id ? { tipo: "gasto", pantalla: "vista", id: ruta.id } : { tipo: "gasto", pantalla: "lista" })} />;
  } else if (ruta.tipo === "compra") {
    pantalla = <EditorCompra id={ruta.id} semilla={ruta.semilla as PedidoCompra | undefined}
      alGuardar={(id) => navegar({ tipo: "compra", pantalla: "vista", id })}
      alCancelar={() => navegar(ruta.id ? { tipo: "compra", pantalla: "vista", id: ruta.id } : { tipo: "compra", pantalla: "lista" })} />;
  } else {
    const tipo = ruta.tipo;
    pantalla = <EditorVenta tipo={tipo} id={ruta.id} semilla={ruta.semilla as SemillaVenta | undefined}
      alGuardar={(id) => navegar({ tipo, pantalla: "vista", id })}
      alCancelar={() => navegar(ruta.id ? { tipo, pantalla: "vista", id: ruta.id } : { tipo, pantalla: "lista" })} />;
  }
  return <Contexto.Provider value={ctx}><div className="dx-raiz" key={clave}>{pantalla}</div></Contexto.Provider>;
}
