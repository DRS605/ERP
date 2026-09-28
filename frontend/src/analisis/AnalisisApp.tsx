/**
 * Análisis de datos: galería de informes (predefinidos y guardados) y diseñador. Se eligen el conjunto de datos,
 * las dimensiones en filas (jerarquía con subtotales) y en columnas (tabla dinámica), las medidas, el periodo
 * (relativo, para que el informe guardado esté siempre al día), la comparación, filtros y los N primeros. El
 * resultado se recalcula al cambiar cualquier cosa; cada fila permite ver sus registros, filtrar, excluir o desglosar.
 */
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { crearApi, type Api } from "../lib/api";
import { datosGrafico, Grafico } from "./Grafico";
import { clave, csv, PERIODOS, rango, valor, variacion } from "./formato";
import type { Campo, Consulta, Dataset, Definicion, Detalle, Fila, Filtro, FiltroMedida, InformeGuardado, Plantilla, Resultado } from "./tipos";

export interface AnfitrionAnalisis {
  token: () => string | null;
  aviso: (mensaje: string, tipo?: "ok" | "err" | "") => void;
  /** Abre un documento de la interfaz (facturas, gastos, pedidosventa) por su id. */
  abrirDocumento?: (vista: string, id: string) => void;
}

interface Catalogo { datasets: Dataset[]; plantillas: Plantilla[] }

const OPERADORES: [string, string][] = [["en", "es"], ["no_en", "no es"], ["contiene", "contiene"], ["empieza", "empieza por"], ["desde", "desde"], ["hasta", "hasta"], ["vacio", "está vacío"], ["no_vacio", "no está vacío"]];
const nombreOperador = (o: string) => OPERADORES.find((x) => x[0] === o)?.[1] ?? o;

function definicionDe(c: Consulta, periodo: string, grafico?: string | null): Definicion {
  return { filtros: [], filtrosMedida: [], comparar: null, limite: null, columna: null, ordenarPor: null, ascendente: false, ...c, periodo, grafico: grafico ?? null };
}

export function AnalisisApp(props: { anfitrion: AnfitrionAnalisis; inicial?: { informeId?: string; plantilla?: string } }) {
  const { anfitrion } = props;
  const api = useMemo<Api>(() => crearApi((e, i) => fetch(e, i), anfitrion.token), [anfitrion]);
  const [catalogo, setCatalogo] = useState<Catalogo | null>(null);
  const [informes, setInformes] = useState<InformeGuardado[]>([]);
  const [def, setDef] = useState<Definicion | null>(null);
  const [actual, setActual] = useState<InformeGuardado | null>(null);
  const [error, setError] = useState("");

  const cargarInformes = useCallback(() => api.get<InformeGuardado[]>("/analisis/informes").then(setInformes).catch(() => setInformes([])), [api]);
  useEffect(() => {
    api.get<Catalogo>("/analisis/catalogo").then((c) => {
      setCatalogo(c);
      const p = props.inicial?.plantilla && c.plantillas.find((x) => x.clave === props.inicial!.plantilla);
      if (p) setDef(definicionDe(p.consulta, p.periodo, p.grafico));
    }).catch((e: Error) => setError(e.message));
    cargarInformes();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [api]);

  const abrir = (i: InformeGuardado) => {
    try {
      setDef(JSON.parse(i.definicion) as Definicion);
      setActual(i);
    } catch {
      anfitrion.aviso("La definición del informe no es válida.", "err");
    }
  };

  if (error) return <div className="panel"><p className="muted">{error}</p></div>;
  if (!catalogo) return <div className="muted">Cargando…</div>;
  if (!def) {
    return <Galeria catalogo={catalogo} informes={informes} alAbrir={abrir}
      alPlantilla={(p) => { setActual(null); setDef(definicionDe(p.consulta, p.periodo, p.grafico)); }}
      alNuevo={(d) => { setActual(null); setDef(definicionDe({ dataset: d.clave, filas: d.filasDefecto, medidas: d.medidasDefecto }, "este_anio")); }}
      alBorrar={async (i) => { try { await api.del(`/analisis/informes/${i.id}`); anfitrion.aviso("Informe borrado.", "ok"); cargarInformes(); } catch (e) { anfitrion.aviso((e as Error).message, "err"); } }} />;
  }

  const dataset = catalogo.datasets.find((d) => d.clave === def.dataset);
  if (!dataset) return <div className="panel"><p className="muted">Ese conjunto de datos no está disponible.</p><button className="btn small" onClick={() => setDef(null)}>Volver</button></div>;
  return <Disenador api={api} anfitrion={anfitrion} dataset={dataset} datasets={catalogo.datasets} def={def} setDef={setDef} actual={actual}
    alGuardado={(i) => { setActual(i); cargarInformes(); }} alVolver={() => { setDef(null); setActual(null); }} />;
}

function Galeria(props: { catalogo: Catalogo; informes: InformeGuardado[]; alAbrir: (i: InformeGuardado) => void; alPlantilla: (p: Plantilla) => void; alNuevo: (d: Dataset) => void; alBorrar: (i: InformeGuardado) => void }) {
  const nombreDs = (c: string) => props.catalogo.datasets.find((d) => d.clave === c)?.nombre ?? c;
  return (
    <div className="ax-raiz">
      {props.informes.length > 0 && (
        <div className="panel">
          <div className="panel-head"><h2>Mis informes</h2></div>
          <div className="ax-tarjetas">
            {props.informes.map((i) => (
              <div key={i.id} className="ax-tarjeta" onClick={() => props.alAbrir(i)}>
                <div className="ax-tarjeta-tit">{i.favorito ? "★ " : ""}{i.nombre}</div>
                <div className="muted">{nombreDs(i.dataset)}{i.compartido ? " · compartido" : ""}{i.propio ? "" : " · de otro usuario"}</div>
                {i.propio && <button className="dx-enlace ax-borrar" title="Borrar" onClick={(e) => { e.stopPropagation(); if (confirm(`¿Borrar el informe «${i.nombre}»?`)) props.alBorrar(i); }}>Borrar</button>}
              </div>
            ))}
          </div>
        </div>
      )}
      <div className="panel">
        <div className="panel-head"><h2>Informes listos para usar</h2></div>
        <div className="ax-tarjetas">
          {props.catalogo.plantillas.map((p) => (
            <div key={p.clave} className="ax-tarjeta" onClick={() => props.alPlantilla(p)}>
              <div className="ax-tarjeta-tit">{p.nombre}</div>
              <div className="muted">{p.descripcion}</div>
              <div className="ax-etiqueta-ds">{nombreDs(p.consulta.dataset)}</div>
            </div>
          ))}
        </div>
      </div>
      <div className="panel">
        <div className="panel-head"><h2>Nuevo análisis</h2></div>
        <div className="ax-tarjetas">
          {props.catalogo.datasets.map((d) => (
            <div key={d.clave} className="ax-tarjeta ax-nuevo" onClick={() => props.alNuevo(d)}>
              <div className="ax-tarjeta-tit">+ {d.nombre}</div>
              <div className="muted">{d.descripcion}</div>
              <div className="ax-etiqueta-ds">{d.dimensiones.length} dimensiones · {d.medidas.length} medidas</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

function Disenador(props: {
  api: Api; anfitrion: AnfitrionAnalisis; dataset: Dataset; datasets: Dataset[]; def: Definicion; setDef: (d: Definicion) => void;
  actual: InformeGuardado | null; alGuardado: (i: InformeGuardado) => void; alVolver: () => void;
}) {
  const { api, anfitrion, dataset, def, setDef } = props;
  const [res, setRes] = useState<Resultado | null>(null);
  const [cargando, setCargando] = useState(false);
  const [error, setError] = useState("");
  const [plegadas, setPlegadas] = useState<Set<string>>(new Set());
  const [menu, setMenu] = useState<{ fila: Fila; x: number; y: number } | null>(null);
  const [detalle, setDetalle] = useState<{ titulo: string; datos: Detalle | null; error?: string } | null>(null);
  const [guardar, setGuardar] = useState<"guardar" | "como" | null>(null);
  const [verGrafico, setVerGrafico] = useState(def.grafico !== "no");
  const peticion = useRef(0);

  const fechas = def.periodo === "personalizado" ? { desde: def.desde ?? null, hasta: def.hasta ?? null } : rango(def.periodo);
  const consulta = useMemo<Consulta>(() => ({
    dataset: def.dataset, filas: def.filas, columna: def.columna || null, medidas: def.medidas, filtros: def.filtros,
    filtrosMedida: def.filtrosMedida, desde: fechas.desde, hasta: fechas.hasta,
    comparar: fechas.desde && fechas.hasta ? def.comparar || null : null, ordenarPor: def.ordenarPor || null, ascendente: !!def.ascendente, limite: def.limite || null,
  }), [def, fechas.desde, fechas.hasta]);

  useEffect(() => {
    const n = ++peticion.current;
    setCargando(true);
    const t = setTimeout(() => {
      api.post<Resultado>("/analisis/consulta", consulta)
        .then((r) => { if (n === peticion.current) { setRes(r); setError(""); } })
        .catch((e: Error) => { if (n === peticion.current) setError(e.message); })
        .finally(() => { if (n === peticion.current) setCargando(false); });
    }, 250);
    return () => clearTimeout(t);
  }, [api, consulta]);

  const cambiar = (c: Partial<Definicion>) => setDef({ ...def, ...c });
  const dim = (k: string) => dataset.dimensiones.find((d) => d.clave === k);
  const filas = def.filas ?? [];
  const medidas = def.medidas ?? [];
  const libres = dataset.dimensiones.filter((d) => !filas.includes(d.clave) && d.clave !== def.columna);
  const grupos = [...new Set(dataset.dimensiones.map((d) => d.grupo))];

  // Filtros que identifican una fila (sus claves), para ver sus registros o desglosarla.
  const filtrosDeFila = (f: Fila): Filtro[] => f.claves.map((k, i) => ({ dimension: filas[i], operador: "en", valores: [k ?? ""] }));

  async function verRegistros(f: Fila | null, columnaValor?: string | null) {
    const extra = f ? filtrosDeFila(f) : [];
    if (columnaValor !== undefined && def.columna) extra.push({ dimension: def.columna, operador: "en", valores: [columnaValor ?? ""] });
    const titulo = f && f.nivel > 0 ? f.claves.map((k, i) => clave(k, filas[i])).join(" › ") : "Todos los registros";
    setDetalle({ titulo, datos: null });
    try {
      const d = await api.post<Detalle>("/analisis/detalle", { dataset: def.dataset, filtros: [...(def.filtros ?? []), ...extra], desde: fechas.desde, hasta: fechas.hasta, limite: 1000 });
      setDetalle({ titulo, datos: d });
    } catch (e) {
      setDetalle({ titulo, datos: null, error: (e as Error).message });
    }
  }

  async function exportar(formato: "xlsx" | "csv") {
    if (!res) return;
    const nombres = res.dimensiones.map((d) => d.nombre);
    const cabecera: { titulo: string; tipo: string }[] = nombres.map((n) => ({ titulo: n, tipo: "texto" }));
    const tipoX = (t: string) => (t === "Moneda" ? "moneda" : t === "Porcentaje" ? "porcentaje" : t === "Numero" ? "numero" : "texto");
    if (res.columna) {
      res.valoresColumna.forEach((v) => res.medidas.forEach((m) => cabecera.push({ titulo: `${clave(v, res.columna!.clave)} · ${m.nombre}`, tipo: tipoX(m.tipo) })));
      res.medidas.forEach((m) => cabecera.push({ titulo: `Total · ${m.nombre}`, tipo: tipoX(m.tipo) }));
    } else {
      res.medidas.forEach((m) => {
        cabecera.push({ titulo: m.nombre, tipo: tipoX(m.tipo) });
        if (res.desdeAnterior) { cabecera.push({ titulo: `${m.nombre} (anterior)`, tipo: tipoX(m.tipo) }); cabecera.push({ titulo: `${m.nombre} Δ %`, tipo: "porcentaje" }); }
      });
    }
    const cuerpo = res.filas.filter((f) => f.nivel > 0).map((f) => {
      const fila: (string | number | null)[] = nombres.map((_, i) => (i < f.claves.length ? (f.resto && i === 0 ? f.claves[0] : clave(f.claves[i], res.dimensiones[i].clave)) : i === f.claves.length ? "Total" : ""));
      if (res.columna) {
        res.valoresColumna.forEach((_, ci) => res.medidas.forEach((_m, mi) => fila.push(f.celdas?.[ci]?.[mi] ?? null)));
        res.medidas.forEach((_m, mi) => fila.push(f.valores[mi]));
      } else {
        res.medidas.forEach((_m, mi) => {
          fila.push(f.valores[mi]);
          if (res.desdeAnterior) { fila.push(f.anteriores?.[mi] ?? null); const v = variacion(f.valores[mi], f.anteriores?.[mi]); fila.push(v === null ? null : v / 100); }
        });
      }
      return fila;
    });
    const titulo = props.actual?.nombre ?? res.titulo;
    if (formato === "xlsx") {
      try {
        const token = anfitrion.token();
        // El total general va en la fila de totales del propio Excel (fórmulas SUMA en las columnas sumables).
        const r = await fetch("/exportar/xlsx", {
          method: "POST",
          headers: { "Content-Type": "application/json", ...(token ? { Authorization: "Bearer " + token } : {}) },
          body: JSON.stringify({ titulo, columnas: cabecera, filas: cuerpo.filter((_f, i) => res.filas[i + 1].nivel === res.dimensiones.length || res.filas[i + 1].resto) }),
        });
        if (r.ok) { descargar(await r.blob(), `${nombreFichero(titulo)}.xlsx`); return; }
        if (r.status !== 404) throw new Error(`No se pudo generar el Excel (HTTP ${r.status}).`);
      } catch (e) {
        anfitrion.aviso((e as Error).message, "err");
        return;
      }
    }
    const lineas = [csv(cabecera.map((c) => c.titulo)), ...cuerpo.map(csv)];
    descargar(new Blob(["﻿" + lineas.join("\r\n")], { type: "text/csv;charset=utf-8" }), `${nombreFichero(titulo)}.csv`);
  }

  const medidaGrafico = Math.max(0, medidas.indexOf(def.medidaGrafico ?? "")) ;
  const datos = res ? datosGrafico(res, medidaGrafico) : null;
  const total = res?.filas[0];

  return (
    <div className="ax-disenador">
      <aside className="ax-lateral panel">
        <div className="ax-lat-cab">
          <button className="btn small secondary" onClick={props.alVolver}>← Informes</button>
          <select value={def.dataset} onChange={(e) => { const d = props.datasets.find((x) => x.clave === e.target.value)!; setDef(definicionDe({ dataset: d.clave, filas: d.filasDefecto, medidas: d.medidasDefecto }, def.periodo)); }}>
            {props.datasets.map((d) => <option key={d.clave} value={d.clave}>{d.nombre}</option>)}
          </select>
        </div>

        <Seccion titulo="Periodo">
          <select value={def.periodo} onChange={(e) => cambiar({ periodo: e.target.value, desde: fechas.desde, hasta: fechas.hasta })}>
            {PERIODOS.map(([k, n]) => <option key={k} value={k}>{n}</option>)}
          </select>
          {def.periodo === "personalizado" && (
            <div className="ax-dos"><input type="date" value={def.desde ?? ""} onChange={(e) => cambiar({ desde: e.target.value || null })} /><input type="date" value={def.hasta ?? ""} onChange={(e) => cambiar({ hasta: e.target.value || null })} /></div>
          )}
          <select value={def.comparar ?? ""} disabled={!fechas.desde} onChange={(e) => cambiar({ comparar: e.target.value || null })} title={fechas.desde ? "" : "Elige un periodo con fechas para comparar"}>
            <option value="">Sin comparar</option><option value="anio_anterior">Comparar con el año anterior</option><option value="periodo_anterior">Comparar con el periodo anterior</option>
          </select>
        </Seccion>

        <Seccion titulo="Filas (agrupar por)">
          {filas.map((k, i) => (
            <div key={k} className="ax-chip-fila">
              <span>{i + 1}. {dim(k)?.nombre ?? k}</span>
              <button className="dx-icono" disabled={i === 0} title="Subir" onClick={() => { const n = [...filas]; [n[i - 1], n[i]] = [n[i], n[i - 1]]; cambiar({ filas: n }); }}>↑</button>
              <button className="dx-icono" title="Quitar" onClick={() => cambiar({ filas: filas.filter((x) => x !== k), ordenarPor: def.ordenarPor === k ? null : def.ordenarPor })}>✕</button>
            </div>
          ))}
          {filas.length < 4 && <SelectorDimension grupos={grupos} dimensiones={libres} texto="+ Añadir nivel" alElegir={(k) => cambiar({ filas: [...filas, k] })} />}
        </Seccion>

        <Seccion titulo="Columnas (tabla dinámica)">
          <select value={def.columna ?? ""} onChange={(e) => cambiar({ columna: e.target.value || null })}>
            <option value="">Sin columnas</option>
            {grupos.map((g) => <optgroup key={g} label={g}>{dataset.dimensiones.filter((d) => d.grupo === g && !filas.includes(d.clave)).map((d) => <option key={d.clave} value={d.clave}>{d.nombre}</option>)}</optgroup>)}
          </select>
        </Seccion>

        <Seccion titulo="Medidas">
          <div className="ax-medidas">
            {dataset.medidas.map((m) => (
              <label key={m.clave} className="dx-check" title={m.descripcion ?? ""}>
                <input type="checkbox" checked={medidas.includes(m.clave)} onChange={(e) => cambiar({ medidas: e.target.checked ? [...medidas, m.clave] : medidas.filter((x) => x !== m.clave) })} /> {m.nombre}
              </label>
            ))}
          </div>
        </Seccion>

        <Seccion titulo="Filtros">
          <Filtros api={api} def={def} dataset={dataset} fechas={fechas} alCambiar={(f) => cambiar({ filtros: f })} />
          <FiltrosMedida dataset={dataset} filtros={def.filtrosMedida ?? []} alCambiar={(f) => cambiar({ filtrosMedida: f })} />
        </Seccion>

        <Seccion titulo="Orden y límite">
          <select value={def.ordenarPor ?? ""} onChange={(e) => cambiar({ ordenarPor: e.target.value || null })}>
            <option value="">Por la primera medida (o por fecha)</option>
            {medidas.map((k) => <option key={k} value={k}>Por {dataset.medidas.find((m) => m.clave === k)?.nombre}</option>)}
            {filas.map((k) => <option key={k} value={k}>Por {dim(k)?.nombre} (alfabético)</option>)}
          </select>
          <div className="ax-dos">
            <select value={def.ascendente ? "asc" : "desc"} onChange={(e) => cambiar({ ascendente: e.target.value === "asc" })}><option value="desc">De mayor a menor</option><option value="asc">De menor a mayor</option></select>
            <select value={def.limite ?? ""} onChange={(e) => cambiar({ limite: e.target.value ? Number(e.target.value) : null })}><option value="">Todos</option>{[5, 10, 20, 50, 100].map((n) => <option key={n} value={n}>Los {n} primeros</option>)}</select>
          </div>
        </Seccion>
      </aside>

      <section className="ax-principal">
        <div className="panel">
          <div className="panel-head">
            <div>
              <h2>{props.actual?.nombre ?? res?.titulo ?? dataset.nombre}</h2>
              <div className="muted ax-subtitulo">
                {fechas.desde ? `${clave(fechas.desde)} – ${clave(fechas.hasta)}` : "Todo el histórico"}
                {res?.desdeAnterior ? ` · frente a ${clave(res.desdeAnterior)} – ${clave(res.hastaAnterior)}` : ""}
                {(def.filtros?.length ?? 0) + (def.filtrosMedida?.length ?? 0) > 0 ? ` · ${(def.filtros?.length ?? 0) + (def.filtrosMedida?.length ?? 0)} filtro(s)` : ""}
                {cargando ? " · calculando…" : ""}
              </div>
            </div>
            <div className="dx-acciones">
              <button className="btn small secondary" onClick={() => setVerGrafico(!verGrafico)}>{verGrafico ? "Ocultar gráfico" : "Ver gráfico"}</button>
              <button className="btn small secondary" disabled={!res} onClick={() => verRegistros(null)}>Registros</button>
              <button className="btn small secondary" disabled={!res} onClick={() => exportar("xlsx")}>Excel</button>
              <button className="btn small secondary" disabled={!res} onClick={() => exportar("csv")}>CSV</button>
              <button className="btn small secondary" disabled={!res} onClick={() => imprimir(props.actual?.nombre ?? res?.titulo ?? "")}>Imprimir</button>
              {props.actual?.propio && <button className="btn small secondary" onClick={() => setGuardar("como")}>Guardar como…</button>}
              <button className="btn small" onClick={() => setGuardar(props.actual?.propio ? "guardar" : "como")}>{props.actual?.propio ? "Guardar" : "Guardar informe"}</button>
            </div>
          </div>
          {error && <div className="dx-aviso">⚠ {error}</div>}
          {res?.truncado && <div className="dx-aviso">⚠ Hay demasiados grupos: se muestran los primeros. Filtra o quita un nivel.</div>}
          {res && total && (
            <div className="ax-kpis">
              {res.medidas.map((m, i) => {
                const v = variacion(total.valores[i], total.anteriores?.[i]);
                return (
                  <div key={m.clave} className={`ax-kpi ${i === medidaGrafico ? "activo" : ""}`} onClick={() => cambiar({ medidaGrafico: m.clave })} title="Dibujar esta medida">
                    <small>{m.nombre}</small>
                    <strong>{valor(total.valores[i], m.tipo) || "—"}</strong>
                    {res.desdeAnterior && <span className={v === null ? "muted" : v >= 0 ? "ax-sube" : "ax-baja"}>{v === null ? "sin datos anteriores" : `${v >= 0 ? "▲" : "▼"} ${Math.abs(v).toLocaleString("es-ES")} % · antes ${valor(total.anteriores?.[i], m.tipo)}`}</span>}
                  </div>
                );
              })}
            </div>
          )}
          {verGrafico && datos && res && res.filas.length > 1 && <Grafico datos={datos} titulo={res.titulo} />}
        </div>

        {res && <Tabla res={res} plegadas={plegadas} setPlegadas={setPlegadas} alMenu={(fila, x, y) => setMenu({ fila, x, y })} alCelda={(f, cv) => verRegistros(f, cv)} />}
      </section>

      {menu && (
        <div className="ax-capa" onClick={() => setMenu(null)}>
          <div className="ax-menu" style={{ left: Math.min(menu.x, window.innerWidth - 260), top: Math.min(menu.y, window.innerHeight - 320) }} onClick={(e) => e.stopPropagation()}>
            <div className="ax-menu-tit">{menu.fila.claves.map((k, i) => clave(k, filas[i])).join(" › ")}</div>
            <button onClick={() => { setMenu(null); verRegistros(menu.fila); }}>Ver los registros</button>
            <button onClick={() => { setMenu(null); cambiar({ filtros: [...(def.filtros ?? []), ...filtrosDeFila(menu.fila)] }); }}>Filtrar: solo esto</button>
            <button onClick={() => { const i = menu.fila.nivel - 1; setMenu(null); cambiar({ filtros: [...(def.filtros ?? []), { dimension: filas[i], operador: "no_en", valores: [menu.fila.claves[i] ?? ""] }] }); }}>Excluir «{clave(menu.fila.claves[menu.fila.nivel - 1], filas[menu.fila.nivel - 1])}»</button>
            <div className="ax-menu-sub">Desglosar por…</div>
            <div className="ax-menu-lista">
              {libres.map((d) => (
                <button key={d.clave} onClick={() => { setMenu(null); cambiar({ filtros: [...(def.filtros ?? []), ...filtrosDeFila(menu.fila)], filas: [d.clave] }); }}>{d.nombre}</button>
              ))}
            </div>
          </div>
        </div>
      )}

      {detalle && <VentanaDetalle detalle={detalle} alCerrar={() => setDetalle(null)} anfitrion={anfitrion} />}
      {guardar && (
        <DialogoGuardar inicial={guardar === "guardar" ? props.actual : null} sugerido={props.actual?.nombre ?? res?.titulo ?? ""}
          alCerrar={() => setGuardar(null)}
          alGuardar={async (nombre, compartido, favorito) => {
            const cuerpo = { nombre, dataset: def.dataset, definicion: JSON.stringify({ ...def, grafico: verGrafico ? def.grafico ?? "si" : "no" }), compartido, favorito };
            try {
              const i = guardar === "guardar" && props.actual ? await api.put<InformeGuardado>(`/analisis/informes/${props.actual.id}`, cuerpo) : await api.post<InformeGuardado>("/analisis/informes", cuerpo);
              anfitrion.aviso("Informe guardado.", "ok");
              setGuardar(null);
              props.alGuardado(i);
            } catch (e) {
              anfitrion.aviso((e as Error).message, "err");
            }
          }} />
      )}
    </div>
  );
}

function Seccion(props: { titulo: string; children: React.ReactNode }) {
  return <div className="ax-seccion"><div className="ax-seccion-tit">{props.titulo}</div>{props.children}</div>;
}

function SelectorDimension(props: { grupos: string[]; dimensiones: Campo[]; texto: string; alElegir: (k: string) => void }) {
  return (
    <select value="" onChange={(e) => e.target.value && props.alElegir(e.target.value)}>
      <option value="">{props.texto}</option>
      {props.grupos.map((g) => {
        const ds = props.dimensiones.filter((d) => d.grupo === g);
        return ds.length ? <optgroup key={g} label={g}>{ds.map((d) => <option key={d.clave} value={d.clave}>{d.nombre}</option>)}</optgroup> : null;
      })}
    </select>
  );
}

function Filtros(props: { api: Api; def: Definicion; dataset: Dataset; fechas: { desde: string | null; hasta: string | null }; alCambiar: (f: Filtro[]) => void }) {
  const filtros = props.def.filtros ?? [];
  const [nuevo, setNuevo] = useState<Filtro | null>(null);
  const [texto, setTexto] = useState("");
  const [valores, setValores] = useState<{ valor: string | null; registros: number }[]>([]);
  const nombre = (k: string) => props.dataset.dimensiones.find((d) => d.clave === k)?.nombre ?? k;
  const conLista = nuevo && ["en", "no_en"].includes(nuevo.operador);

  useEffect(() => {
    if (!nuevo || !conLista) return;
    const q = new URLSearchParams({ dimension: nuevo.dimension, texto });
    if (props.fechas.desde) q.set("desde", props.fechas.desde);
    if (props.fechas.hasta) q.set("hasta", props.fechas.hasta);
    const t = setTimeout(() => props.api.get<{ valor: string | null; registros: number }[]>(`/analisis/${props.def.dataset}/valores?${q}`).then(setValores).catch(() => setValores([])), 200);
    return () => clearTimeout(t);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [nuevo?.dimension, nuevo?.operador, texto]);

  return (
    <div className="ax-filtros">
      {filtros.map((f, i) => (
        <div key={i} className="ax-filtro">
          <span><b>{nombre(f.dimension)}</b> {nombreOperador(f.operador)} {(f.valores ?? []).map((v) => clave(v, f.dimension)).join(", ")}</span>
          <button className="dx-icono" title="Quitar" onClick={() => props.alCambiar(filtros.filter((_, j) => j !== i))}>✕</button>
        </div>
      ))}
      {!nuevo && <SelectorDimension grupos={[...new Set(props.dataset.dimensiones.map((d) => d.grupo))]} dimensiones={props.dataset.dimensiones} texto="+ Filtrar por…" alElegir={(k) => { setNuevo({ dimension: k, operador: "en", valores: [] }); setTexto(""); }} />}
      {nuevo && (
        <div className="ax-nuevo-filtro">
          <div className="ax-dos">
            <b>{nombre(nuevo.dimension)}</b>
            <select value={nuevo.operador} onChange={(e) => setNuevo({ ...nuevo, operador: e.target.value, valores: [] })}>{OPERADORES.map(([k, n]) => <option key={k} value={k}>{n}</option>)}</select>
          </div>
          {conLista && (
            <>
              <input placeholder="Buscar valores…" value={texto} onChange={(e) => setTexto(e.target.value)} autoFocus />
              <div className="ax-valores">
                {valores.map((v) => {
                  const k = v.valor ?? "";
                  const marcado = (nuevo.valores ?? []).includes(k);
                  return (
                    <label key={k} className="dx-check">
                      <input type="checkbox" checked={marcado} onChange={() => setNuevo({ ...nuevo, valores: marcado ? (nuevo.valores ?? []).filter((x) => x !== k) : [...(nuevo.valores ?? []), k] })} />
                      <span>{clave(v.valor, nuevo.dimension)}</span><small className="muted">{v.registros.toLocaleString("es-ES")}</small>
                    </label>
                  );
                })}
                {!valores.length && <div className="muted">Sin valores.</div>}
              </div>
            </>
          )}
          {nuevo && ["contiene", "empieza", "desde", "hasta"].includes(nuevo.operador) && (
            <input placeholder={nuevo.operador === "desde" || nuevo.operador === "hasta" ? "Valor (p. ej. 2026-03 o 6)" : "Texto"} value={(nuevo.valores ?? [])[0] ?? ""} onChange={(e) => setNuevo({ ...nuevo, valores: [e.target.value] })} autoFocus />
          )}
          <div className="ax-dos">
            <button className="btn small secondary" onClick={() => setNuevo(null)}>Cancelar</button>
            <button className="btn small" disabled={!["vacio", "no_vacio"].includes(nuevo.operador) && !(nuevo.valores ?? []).some((v) => conLista || v.trim())}
              onClick={() => { props.alCambiar([...filtros, nuevo]); setNuevo(null); }}>Aplicar</button>
          </div>
        </div>
      )}
    </div>
  );
}

function FiltrosMedida(props: { dataset: Dataset; filtros: FiltroMedida[]; alCambiar: (f: FiltroMedida[]) => void }) {
  const [nuevo, setNuevo] = useState<FiltroMedida | null>(null);
  const nombre = (k: string) => props.dataset.medidas.find((m) => m.clave === k)?.nombre ?? k;
  return (
    <div className="ax-filtros">
      {props.filtros.map((f, i) => (
        <div key={i} className="ax-filtro">
          <span><b>{nombre(f.medida)}</b> {f.operador === "mayor" ? ">" : f.operador === "menor" ? "<" : "entre"} {f.valor.toLocaleString("es-ES")}{f.operador === "entre" ? ` y ${(f.hasta ?? 0).toLocaleString("es-ES")}` : ""}</span>
          <button className="dx-icono" title="Quitar" onClick={() => props.alCambiar(props.filtros.filter((_, j) => j !== i))}>✕</button>
        </div>
      ))}
      {!nuevo && (
        <select value="" onChange={(e) => e.target.value && setNuevo({ medida: e.target.value, operador: "mayor", valor: 0 })}>
          <option value="">+ Filtrar por una cifra…</option>
          {props.dataset.medidas.map((m) => <option key={m.clave} value={m.clave}>{m.nombre}</option>)}
        </select>
      )}
      {nuevo && (
        <div className="ax-nuevo-filtro">
          <div className="ax-dos">
            <b>{nombre(nuevo.medida)}</b>
            <select value={nuevo.operador} onChange={(e) => setNuevo({ ...nuevo, operador: e.target.value as FiltroMedida["operador"] })}><option value="mayor">mayor que</option><option value="menor">menor que</option><option value="entre">entre</option></select>
          </div>
          <div className="ax-dos">
            <input type="number" value={nuevo.valor} onChange={(e) => setNuevo({ ...nuevo, valor: Number(e.target.value) })} />
            {nuevo.operador === "entre" && <input type="number" value={nuevo.hasta ?? 0} onChange={(e) => setNuevo({ ...nuevo, hasta: Number(e.target.value) })} />}
          </div>
          <div className="muted ax-ayuda">Se aplica al último nivel de las filas (p. ej. clientes con ventas &gt; 10.000 €).</div>
          <div className="ax-dos">
            <button className="btn small secondary" onClick={() => setNuevo(null)}>Cancelar</button>
            <button className="btn small" onClick={() => { props.alCambiar([...props.filtros, nuevo]); setNuevo(null); }}>Aplicar</button>
          </div>
        </div>
      )}
    </div>
  );
}

const claveFila = (f: Fila) => `${f.nivel}|${f.claves.join("\u001f")}`;

function Tabla(props: { res: Resultado; plegadas: Set<string>; setPlegadas: (s: Set<string>) => void; alMenu: (f: Fila, x: number, y: number) => void; alCelda: (f: Fila, columnaValor?: string | null) => void }) {
  const { res, plegadas } = props;
  const n = res.dimensiones.length;
  const total = res.filas[0];
  const comparar = !!res.desdeAnterior && !res.columna;

  // Filas visibles: las de un grupo plegado se ocultan.
  const visibles: Fila[] = [];
  let ocultarHasta: number | null = null;
  for (const f of res.filas.slice(1)) {
    if (ocultarHasta !== null && f.nivel > ocultarHasta) continue;
    ocultarHasta = null;
    visibles.push(f);
    if (plegadas.has(claveFila(f)) && f.nivel < n) ocultarHasta = f.nivel;
  }

  // Barras de datos en la primera medida: proporcionales al máximo de su nivel.
  const maximos = new Map<number, number>();
  for (const f of res.filas) maximos.set(f.nivel, Math.max(maximos.get(f.nivel) ?? 0, Math.abs(f.valores[0] ?? 0)));

  const plegarTodo = (plegar: boolean) => props.setPlegadas(new Set(plegar ? res.filas.filter((f) => f.nivel > 0 && f.nivel < n).map(claveFila) : []));
  const celda = (v: number | null | undefined, tipo: Resultado["medidas"][number]["tipo"], barra?: number) => (
    <td className={`num ${v !== null && v !== undefined && v < 0 ? "ax-neg" : ""}`}>
      {barra !== undefined && barra > 0 && <span className="ax-barra" style={{ width: `${Math.min(100, barra * 100)}%` }} />}
      <span className="ax-num">{valor(v, tipo)}</span>
    </td>
  );
  const delta = (a: number | null | undefined, b: number | null | undefined) => {
    const v = variacion(a, b);
    return <td className={`num ${v === null ? "muted" : v >= 0 ? "ax-sube" : "ax-baja"}`}>{v === null ? "—" : `${v >= 0 ? "+" : ""}${v.toLocaleString("es-ES")} %`}</td>;
  };

  return (
    <div className="panel ax-tabla-panel">
      <div className="ax-tabla-herr">
        <span className="muted">{res.filas.filter((f) => f.nivel === n && n > 0).length.toLocaleString("es-ES")} grupos{n > 1 ? " · pulsa ▸ para plegar" : ""} · pulsa una fila para verla, filtrarla o desglosarla</span>
        {n > 1 && <span><button className="dx-enlace" onClick={() => plegarTodo(true)}>Plegar todo</button><button className="dx-enlace" onClick={() => plegarTodo(false)}>Desplegar todo</button></span>}
      </div>
      <div className="ax-scroll">
        <table className="ax-tabla" data-totales="no" data-pro="1">
          <thead>
            {res.columna ? (
              <>
                <tr>
                  <th rowSpan={2}>{res.dimensiones.map((d) => d.nombre).join(" › ") || "Total"}</th>
                  {res.valoresColumna.map((v) => <th key={v ?? ""} colSpan={res.medidas.length} className="ax-th-col">{clave(v, res.columna!.clave)}</th>)}
                  <th colSpan={res.medidas.length} className="ax-th-col ax-th-total">Total</th>
                </tr>
                <tr>{[...res.valoresColumna, "__total"].flatMap((v) => res.medidas.map((m) => <th key={`${v}-${m.clave}`} className="num">{m.nombre}</th>))}</tr>
              </>
            ) : (
              <tr>
                <th>{res.dimensiones.map((d) => d.nombre).join(" › ") || "Total"}</th>
                {res.medidas.flatMap((m) => comparar
                  ? [<th key={m.clave} className="num">{m.nombre}</th>, <th key={m.clave + "a"} className="num ax-th-ant">Anterior</th>, <th key={m.clave + "d"} className="num ax-th-ant">Δ %</th>]
                  : [<th key={m.clave} className="num">{m.nombre}</th>])}
              </tr>
            )}
          </thead>
          <tbody>
            {visibles.map((f) => {
              const k = claveFila(f);
              const plegable = f.nivel < n && !f.resto;
              const max = maximos.get(f.nivel) || 1;
              return (
                <tr key={k} className={`ax-n${f.nivel} ${f.nivel < n ? "ax-subtotal" : ""} ${f.resto ? "ax-resto" : ""}`}>
                  <td className="ax-dim" style={{ paddingLeft: 10 + (f.nivel - 1) * 18 }}>
                    {plegable ? <button className="ax-plegar" onClick={() => { const s = new Set(plegadas); if (s.has(k)) s.delete(k); else s.add(k); props.setPlegadas(s); }}>{plegadas.has(k) ? "▸" : "▾"}</button> : <span className="ax-plegar-hueco" />}
                    <span className={f.resto ? "muted" : "ax-dim-texto"} onClick={(e) => !f.resto && props.alMenu(f, e.clientX, e.clientY)}>
                      {f.resto ? f.claves[0] : clave(f.claves[f.nivel - 1], res.dimensiones[f.nivel - 1]?.clave)}
                    </span>
                  </td>
                  {res.columna
                    ? [...res.valoresColumna.map((cv, ci) => res.medidas.map((m, mi) => <td key={`${ci}-${mi}`} className={`num ax-clic ${(f.celdas?.[ci]?.[mi] ?? 0) < 0 ? "ax-neg" : ""}`} onClick={() => !f.resto && props.alCelda(f, cv)}>{valor(f.celdas?.[ci]?.[mi], m.tipo)}</td>)),
                      ...res.medidas.map((m, mi) => <td key={`t-${mi}`} className="num ax-col-total">{valor(f.valores[mi], m.tipo)}</td>)]
                    : res.medidas.map((m, mi) => comparar
                      ? [celda(f.valores[mi], m.tipo, mi === 0 ? Math.abs(f.valores[0] ?? 0) / max : undefined), <td key={mi + "a"} className="num muted">{valor(f.anteriores?.[mi], m.tipo)}</td>, delta(f.valores[mi], f.anteriores?.[mi])]
                      : celda(f.valores[mi], m.tipo, mi === 0 ? Math.abs(f.valores[0] ?? 0) / max : undefined))}
                </tr>
              );
            })}
          </tbody>
          {total && (
            <tfoot>
              <tr>
                <td>Total</td>
                {res.columna
                  ? [...res.valoresColumna.map((_, ci) => res.medidas.map((m, mi) => <td key={`${ci}-${mi}`} className="num">{valor(total.celdas?.[ci]?.[mi], m.tipo)}</td>)), ...res.medidas.map((m, mi) => <td key={`t${mi}`} className="num ax-col-total">{valor(total.valores[mi], m.tipo)}</td>)]
                  : res.medidas.map((m, mi) => comparar
                    ? [<td key={mi} className="num">{valor(total.valores[mi], m.tipo)}</td>, <td key={mi + "a"} className="num">{valor(total.anteriores?.[mi], m.tipo)}</td>, delta(total.valores[mi], total.anteriores?.[mi])]
                    : <td key={mi} className="num">{valor(total.valores[mi], m.tipo)}</td>)}
              </tr>
            </tfoot>
          )}
        </table>
      </div>
    </div>
  );
}

function VentanaDetalle(props: { detalle: { titulo: string; datos: Detalle | null; error?: string }; alCerrar: () => void; anfitrion: AnfitrionAnalisis }) {
  const { datos } = props.detalle;
  const sumables = datos?.columnas.map((c) => c.tipo === "Moneda" || c.tipo === "Numero") ?? [];
  return (
    <div className="ax-capa" onClick={props.alCerrar}>
      <div className="ax-ventana panel" onClick={(e) => e.stopPropagation()}>
        <div className="panel-head">
          <h2>Registros · {props.detalle.titulo}</h2>
          <button className="btn small secondary" onClick={props.alCerrar}>Cerrar</button>
        </div>
        {props.detalle.error && <div className="dx-aviso">⚠ {props.detalle.error}</div>}
        {!datos && !props.detalle.error && <div className="muted">Cargando…</div>}
        {datos && (
          <>
            <div className="muted ax-ayuda">{datos.filas.length.toLocaleString("es-ES")} registro(s){datos.truncado ? " (se muestran los 1.000 más recientes)" : ""}{datos.vistaDocumento && props.anfitrion.abrirDocumento ? " · pulsa una fila para abrir el documento" : ""}</div>
            <div className="ax-scroll ax-scroll-detalle">
              <table className="ax-tabla" data-totales="no" data-pro="1">
                <thead><tr>{datos.columnas.map((c) => <th key={c.clave} className={c.tipo === "Texto" || c.tipo === "Fecha" ? "" : "num"}>{c.nombre}</th>)}</tr></thead>
                <tbody>
                  {datos.filas.map((f, i) => (
                    <tr key={i} className={datos.ids[i] && datos.vistaDocumento && props.anfitrion.abrirDocumento ? "ax-clic" : ""}
                      onClick={() => { const id = datos.ids[i]; if (id && datos.vistaDocumento && props.anfitrion.abrirDocumento) props.anfitrion.abrirDocumento(datos.vistaDocumento, id); }}>
                      {f.map((v, j) => {
                        const c = datos.columnas[j];
                        return <td key={j} className={c.tipo === "Texto" || c.tipo === "Fecha" ? "" : "num"}>{c.tipo === "Fecha" ? clave(v as string) : typeof v === "number" ? valor(v, c.tipo) : String(v ?? "")}</td>;
                      })}
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr>{datos.columnas.map((c, j) => <td key={j} className="num">{j === 0 ? "Total" : sumables[j] && c.nombre !== "Precio" && c.nombre !== "Coste unitario" ? valor(datos.filas.reduce((s, f) => s + (typeof f[j] === "number" ? (f[j] as number) : 0), 0), c.tipo) : ""}</td>)}</tr>
                </tfoot>
              </table>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

function DialogoGuardar(props: { inicial: InformeGuardado | null; sugerido: string; alCerrar: () => void; alGuardar: (nombre: string, compartido: boolean, favorito: boolean) => void }) {
  const [nombre, setNombre] = useState(props.inicial?.nombre ?? props.sugerido);
  const [compartido, setCompartido] = useState(props.inicial?.compartido ?? false);
  const [favorito, setFavorito] = useState(props.inicial?.favorito ?? false);
  return (
    <div className="ax-capa" onClick={props.alCerrar}>
      <div className="ax-dialogo panel" onClick={(e) => e.stopPropagation()}>
        <div className="panel-head"><h2>{props.inicial ? "Guardar informe" : "Guardar como informe nuevo"}</h2></div>
        <label>Nombre</label>
        <input value={nombre} onChange={(e) => setNombre(e.target.value)} autoFocus maxLength={120} />
        <label className="dx-check"><input type="checkbox" checked={compartido} onChange={(e) => setCompartido(e.target.checked)} /> Compartido con toda la empresa</label>
        <label className="dx-check"><input type="checkbox" checked={favorito} onChange={(e) => setFavorito(e.target.checked)} /> Favorito (sale el primero)</label>
        <p className="muted ax-ayuda">Se guarda el periodo relativo (p. ej. «este año»), así el informe siempre está al día.</p>
        <div className="ax-dos">
          <button className="btn small secondary" onClick={props.alCerrar}>Cancelar</button>
          <button className="btn small" disabled={!nombre.trim()} onClick={() => props.alGuardar(nombre.trim(), compartido, favorito)}>Guardar</button>
        </div>
      </div>
    </div>
  );
}

function nombreFichero(t: string) {
  return (t || "analisis").toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "").replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 60) || "analisis";
}

function descargar(blob: Blob, nombre: string) {
  const u = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = u;
  a.download = nombre;
  a.click();
  setTimeout(() => URL.revokeObjectURL(u), 60000);
}

/** Imprime el resultado (título, KPI, gráfico y tabla) en una ventana aparte, sin el menú ni el diseñador. */
function imprimir(titulo: string) {
  const principal = document.querySelector(".ax-principal");
  if (!principal) return;
  const v = window.open("", "_blank");
  if (!v) return;
  const estilos = [...document.querySelectorAll("style")].map((s) => s.outerHTML).join("");
  v.document.write(`<!doctype html><html><head><meta charset="utf-8"><title>${titulo.replace(/</g, "&lt;")}</title>${estilos}<style>body{background:#fff;padding:16px}.dx-acciones,.ax-tabla-herr button,.ax-plegar{display:none!important}.ax-scroll{overflow:visible!important;max-height:none!important}</style></head><body>${principal.outerHTML}</body></html>`);
  v.document.close();
  v.focus();
  setTimeout(() => v.print(), 300);
}
