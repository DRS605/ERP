/**
 * Gráfico del análisis en SVG, sin librerías: líneas cuando el eje es el tiempo; barras horizontales (ranking) si
 * no. Un solo eje; colores categóricos en orden fijo (el 9.º grupo y siguientes se pliegan en «Otros»), leyenda
 * siempre que haya más de una serie, el periodo anterior en gris discontinuo, y detalle al pasar el ratón.
 */
import { useMemo, useState } from "react";
import { clave, valor } from "./formato";
import type { Resultado, TipoDato } from "./tipos";

interface Serie { nombre: string; valores: (number | null)[]; anterior?: boolean }
interface Datos { categorias: string[]; etiquetas: string[]; series: Serie[]; tiempo: boolean; tipo: TipoDato; dimension?: string }

const MAX_SERIES = 7;
const esFecha = (tipo?: string) => tipo === "Fecha";

/** Convierte el resultado en categorías y series para una medida. */
export function datosGrafico(res: Resultado, medida: number): Datos | null {
  const m = res.medidas[medida];
  if (!m) return null;
  // En las medidas sumables, un hueco (sin registros) es un cero: la línea no se corta.
  const v0 = (v: number | null | undefined) => (v === null || v === undefined ? (m.aditiva ? 0 : null) : v);
  const nivel1 = res.filas.filter((f) => f.nivel === 1 && !f.resto);
  const dim0 = res.dimensiones[0];
  if (res.columna && res.valoresColumna.length) {
    const colTiempo = esFecha(res.columna.tipo);
    const filasTiempo = esFecha(dim0?.tipo);
    if (colTiempo || !filasTiempo) {
      // Eje = valores de la columna; series = filas de primer nivel (las mayores; el resto, «Otros»).
      const orden = [...nivel1].sort((a, b) => Math.abs(b.valores[medida] ?? 0) - Math.abs(a.valores[medida] ?? 0));
      const principales = orden.slice(0, MAX_SERIES);
      const otras = orden.slice(MAX_SERIES);
      const series: Serie[] = principales.map((f) => ({ nombre: clave(f.claves[0], dim0?.clave), valores: res.valoresColumna.map((_, ci) => v0(f.celdas?.[ci]?.[medida])) }));
      if (otras.length && m.aditiva) series.push({ nombre: `Otros (${otras.length})`, valores: res.valoresColumna.map((_, ci) => otras.reduce((s, f) => s + (f.celdas?.[ci]?.[medida] ?? 0), 0)) });
      return { categorias: res.valoresColumna.map((v) => v ?? ""), etiquetas: res.valoresColumna.map((v) => clave(v, res.columna!.clave)), series, tiempo: colTiempo, tipo: m.tipo, dimension: res.columna.clave };
    }

    // Filas en el tiempo y columnas por otra dimensión: series = valores de la columna.
    const totales = res.valoresColumna.map((_, ci) => ({ ci, t: Math.abs(res.filas[0]?.celdas?.[ci]?.[medida] ?? 0) })).sort((a, b) => b.t - a.t);
    const principales = totales.slice(0, MAX_SERIES).map((x) => x.ci);
    const series: Serie[] = principales.map((ci) => ({ nombre: clave(res.valoresColumna[ci], res.columna!.clave), valores: nivel1.map((f) => v0(f.celdas?.[ci]?.[medida])) }));
    const resto = totales.slice(MAX_SERIES).map((x) => x.ci);
    if (resto.length && m.aditiva) series.push({ nombre: `Otros (${resto.length})`, valores: nivel1.map((f) => resto.reduce((s, ci) => s + (f.celdas?.[ci]?.[medida] ?? 0), 0)) });
    return { categorias: nivel1.map((f) => f.claves[0] ?? ""), etiquetas: nivel1.map((f) => clave(f.claves[0], dim0?.clave)), series, tiempo: true, tipo: m.tipo, dimension: dim0?.clave };
  }

  if (!dim0) return null;
  const tiempo = esFecha(dim0.tipo);
  const filas = tiempo ? nivel1 : nivel1.slice(0, 15);
  const series: Serie[] = [{ nombre: res.desdeAnterior ? "Periodo actual" : m.nombre, valores: filas.map((f) => f.valores[medida]) }];
  if (res.desdeAnterior) series.push({ nombre: "Periodo anterior", valores: filas.map((f) => f.anteriores?.[medida] ?? null), anterior: true });
  return { categorias: filas.map((f) => f.claves[0] ?? ""), etiquetas: filas.map((f) => clave(f.claves[0], dim0.clave)), series, tiempo, tipo: m.tipo, dimension: dim0.clave };
}

const color = (i: number, s: Serie) => (s.anterior ? "var(--ax-anterior)" : `var(--ax-s${(i % 8) + 1})`);

function escala(valores: number[]) {
  const min = Math.min(0, ...valores), max = Math.max(0, ...valores);
  if (min === max) return { min: 0, max: 1, marcas: [0, 1] };
  const bruto = (max - min) / 4;
  const mag = Math.pow(10, Math.floor(Math.log10(bruto)));
  const paso = [1, 2, 2.5, 5, 10].map((k) => k * mag).find((p) => p >= bruto) ?? bruto;
  const a = Math.floor(min / paso) * paso, b = Math.ceil(max / paso) * paso;
  const marcas: number[] = [];
  for (let v = a; v <= b + paso / 2; v += paso) marcas.push(Math.round(v * 1e6) / 1e6);
  return { min: a, max: b, marcas };
}

const corto = (v: number) => {
  const a = Math.abs(v);
  if (a >= 1e6) return `${(v / 1e6).toLocaleString("es-ES", { maximumFractionDigits: 1 })} M`;
  if (a >= 1e3) return `${(v / 1e3).toLocaleString("es-ES", { maximumFractionDigits: 1 })} mil`;
  return v.toLocaleString("es-ES", { maximumFractionDigits: 2 });
};

export function Grafico(props: { datos: Datos; titulo: string }) {
  const { datos } = props;
  const [hover, setHover] = useState<number | null>(null);
  const todos = useMemo(() => datos.series.flatMap((s) => s.valores.filter((v): v is number => v !== null)), [datos]);
  if (!datos.categorias.length || !todos.length) return <div className="muted ax-vacio">No hay datos que dibujar.</div>;
  const e = escala(todos);
  const leyenda = datos.series.length > 1 && (
    <div className="ax-leyenda">
      {datos.series.map((s, i) => <span key={s.nombre}><i className={s.anterior ? "anterior" : ""} style={{ background: color(i, s) }} />{s.nombre}</span>)}
    </div>
  );
  const tooltip = hover !== null && (
    <div className="ax-tooltip">
      <strong>{datos.etiquetas[hover]}</strong>
      {datos.series.map((s, i) => <div key={s.nombre}><i style={{ background: color(i, s) }} />{s.nombre}<b>{valor(s.valores[hover], datos.tipo)}</b></div>)}
    </div>
  );

  if (datos.tiempo) {
    const W = 900, H = 280, ml = 64, mr = 16, mt = 12, mb = 34;
    const n = datos.categorias.length;
    const x = (i: number) => ml + (n === 1 ? (W - ml - mr) / 2 : (i * (W - ml - mr)) / (n - 1));
    const y = (v: number) => mt + ((e.max - v) * (H - mt - mb)) / (e.max - e.min);
    const cada = Math.max(1, Math.ceil(n / 12));
    return (
      <figure className="ax-grafico" aria-label={props.titulo}>
        {leyenda}
        <div className="ax-lienzo">
          <svg viewBox={`0 0 ${W} ${H}`} role="img" onMouseLeave={() => setHover(null)}>
            {e.marcas.map((m) => <g key={m}><line x1={ml} x2={W - mr} y1={y(m)} y2={y(m)} className={m === 0 ? "ax-cero" : "ax-rejilla"} /><text x={ml - 8} y={y(m) + 4} className="ax-eje" textAnchor="end">{corto(m)}</text></g>)}
            {datos.etiquetas.map((t, i) => (i % cada === 0 ? <text key={i} x={x(i)} y={H - 12} className="ax-eje" textAnchor="middle">{t.length > 14 ? t.slice(0, 13) + "…" : t}</text> : null))}
            {hover !== null && <line x1={x(hover)} x2={x(hover)} y1={mt} y2={H - mb} className="ax-cruz" />}
            {datos.series.map((s, si) => {
              const puntos = s.valores.map((v, i) => (v === null ? null : [x(i), y(v)] as const));
              const d = puntos.reduce((acc, p, i) => (p ? acc + `${acc && puntos[i - 1] ? "L" : "M"}${p[0].toFixed(1)},${p[1].toFixed(1)}` : acc), "");
              return (
                <g key={s.nombre}>
                  <path d={d} fill="none" stroke={color(si, s)} strokeWidth={2} strokeDasharray={s.anterior ? "5 4" : undefined} strokeLinejoin="round" strokeLinecap="round" />
                  {n <= 40 && puntos.map((p, i) => (p ? <circle key={i} cx={p[0]} cy={p[1]} r={hover === i ? 5 : 3.5} fill={color(si, s)} stroke="var(--surface,#fff)" strokeWidth={2} /> : null))}
                </g>
              );
            })}
            {datos.categorias.map((_, i) => <rect key={i} x={x(i) - (W - ml - mr) / Math.max(1, n - 1) / 2} y={mt} width={(W - ml - mr) / Math.max(1, n - 1)} height={H - mt - mb} fill="transparent" onMouseEnter={() => setHover(i)} />)}
          </svg>
          {tooltip}
        </div>
      </figure>
    );
  }

  // Ranking: barras horizontales (apiladas si hay varias series; el periodo anterior, barra fina gris).
  const apilado = datos.series.filter((s) => !s.anterior).length > 1;
  const sumas = datos.categorias.map((_, i) => datos.series.filter((s) => !s.anterior).reduce((t, s) => t + (s.valores[i] ?? 0), 0));
  const esc = apilado ? escala([...sumas, ...todos.filter((v) => v < 0)]) : e;
  const fila = 26, W = 900, ml = 200, mr = 90;
  const H = datos.categorias.length * fila + 26;
  const x = (v: number) => ml + ((v - esc.min) * (W - ml - mr)) / (esc.max - esc.min);
  return (
    <figure className="ax-grafico" aria-label={props.titulo}>
      {leyenda}
      <div className="ax-lienzo">
        <svg viewBox={`0 0 ${W} ${H}`} role="img" onMouseLeave={() => setHover(null)}>
          {esc.marcas.map((m) => <g key={m}><line x1={x(m)} x2={x(m)} y1={0} y2={H - 22} className={m === 0 ? "ax-cero" : "ax-rejilla"} /><text x={x(m)} y={H - 6} className="ax-eje" textAnchor="middle">{corto(m)}</text></g>)}
          {datos.etiquetas.map((t, i) => {
            const y0 = i * fila + 4;
            let acumulado = 0;
            const actuales = datos.series.map((s, si) => ({ s, si })).filter((x2) => !x2.s.anterior);
            const anterior = datos.series.find((s) => s.anterior);
            return (
              <g key={i} onMouseEnter={() => setHover(i)} className={hover === i ? "ax-activa" : undefined}>
                <rect x={0} y={y0 - 2} width={W} height={fila} fill="transparent" />
                <text x={ml - 8} y={y0 + 13} className="ax-etiqueta" textAnchor="end">{t.length > 28 ? t.slice(0, 27) + "…" : t}</text>
                {actuales.map(({ s, si }) => {
                  const v = s.valores[i] ?? 0;
                  const desde = apilado ? acumulado : 0;
                  acumulado += v;
                  const a = x(Math.min(desde, desde + v)), b = x(Math.max(desde, desde + v));
                  return <rect key={si} x={a} y={y0} width={Math.max(0, b - a - (apilado ? 2 : 0))} height={anterior ? 12 : 16} rx={3} fill={color(si, s)} />;
                })}
                {anterior && anterior.valores[i] !== null && (() => {
                  const v = anterior.valores[i] ?? 0;
                  const a = x(Math.min(0, v)), b = x(Math.max(0, v));
                  return <rect x={a} y={y0 + 14} width={Math.max(0, b - a)} height={4} rx={2} fill="var(--ax-anterior)" />;
                })()}
                <text x={x(apilado ? Math.max(0, sumas[i]) : Math.max(0, datos.series[0].valores[i] ?? 0)) + 6} y={y0 + 12} className="ax-valor">{corto(apilado ? sumas[i] : datos.series[0].valores[i] ?? 0)}</text>
              </g>
            );
          })}
        </svg>
        {tooltip}
      </div>
    </figure>
  );
}
