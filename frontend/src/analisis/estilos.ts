/** Estilos del análisis (paneles, botones y pastillas los pone la interfaz anfitriona). */
export const ESTILOS_ANALISIS = `
.ax-raiz { display:flex; flex-direction:column; gap:16px; }
.ax-raiz .panel, .ax-disenador .panel { margin:0; }
:root { --ax-s1:#2a78d6; --ax-s2:#eb6834; --ax-s3:#1baf7a; --ax-s4:#eda100; --ax-s5:#e87ba4; --ax-s6:#008300; --ax-s7:#4a3aa7; --ax-s8:#e34948; --ax-anterior:#9a9892; --ax-rejilla:#e6e9ee; --ax-barra:rgba(42,120,214,.12); }
:root[data-theme="dark"] { --ax-s1:#3987e5; --ax-s2:#d95926; --ax-s3:#199e70; --ax-s4:#c98500; --ax-s5:#d55181; --ax-s6:#008300; --ax-s7:#9085e9; --ax-s8:#e66767; --ax-anterior:#7a7973; --ax-rejilla:#232a36; --ax-barra:rgba(57,135,229,.20); }
.ax-tarjetas { display:grid; grid-template-columns:repeat(auto-fill, minmax(250px, 1fr)); gap:12px; }
.ax-tarjeta { position:relative; border:1px solid var(--line); border-radius:12px; padding:14px 14px 12px; cursor:pointer; background:var(--surface,#fff); transition:border-color .15s, box-shadow .15s; font-size:13px; line-height:1.45; }
.ax-tarjeta:hover { border-color:var(--accent); box-shadow:0 4px 14px rgba(8,145,178,.12); }
.ax-tarjeta-tit { font-weight:700; margin-bottom:4px; color:var(--ink); }
.ax-nuevo .ax-tarjeta-tit { color:var(--accent); }
.ax-etiqueta-ds { margin-top:8px; font-size:11.5px; color:var(--accent); font-weight:600; }
.ax-borrar { position:absolute; top:10px; right:8px; }
.ax-disenador { display:grid; grid-template-columns:290px minmax(0,1fr); gap:16px; align-items:start; }
.ax-lateral { position:sticky; top:12px; max-height:calc(100vh - 110px); overflow:auto; padding:14px !important; }
.ax-disenador .dx-check, .ax-dialogo .dx-check { display:flex; align-items:center; gap:8px; cursor:pointer; color:var(--ink); margin:6px 0 0; font-size:13px; }
.ax-disenador input[type=checkbox], .ax-dialogo input[type=checkbox] { width:auto !important; margin:0 !important; flex:none; }
.dx-icono { border:1px solid var(--line); background:var(--surface,#fff); border-radius:8px; width:28px; height:28px; cursor:pointer; margin-left:3px; color:var(--muted); font-weight:700; }
.dx-icono:hover { color:var(--accent); border-color:var(--accent); }
.dx-enlace { border:none; background:none; color:var(--accent); cursor:pointer; font-weight:600; padding:0 4px; font-size:12.5px; }
.dx-aviso { margin:8px 0; color:#b45309; font-weight:600; }
.dx-acciones { display:flex; flex-wrap:wrap; gap:6px; justify-content:flex-end; }
.ax-principal .panel-head { align-items:flex-start; gap:12px; }
.ax-lateral select, .ax-lateral input:not([type=checkbox]) { width:100%; padding:7px 9px; font-size:13px; border-radius:8px; margin:0 0 6px; }
.ax-lat-cab { display:flex; flex-direction:column; gap:8px; margin-bottom:6px; }
.ax-seccion { border-top:1px solid var(--line); padding:10px 0 4px; }
.ax-seccion-tit { font-size:11.5px; text-transform:uppercase; letter-spacing:.04em; color:var(--muted); font-weight:700; margin-bottom:8px; }
.ax-dos { display:flex; gap:6px; align-items:center; }
.ax-dos > * { flex:1; }
.ax-chip-fila { display:flex; align-items:center; gap:2px; background:var(--accent-soft); border-radius:8px; padding:3px 4px 3px 9px; margin-bottom:5px; font-size:13px; }
.ax-chip-fila span { flex:1; }
.ax-chip-fila .dx-icono { width:24px; height:24px; }
.ax-medidas { display:flex; flex-direction:column; gap:0; }
.ax-medidas .dx-check { margin-top:4px !important; font-size:13px; }
.ax-filtros { display:flex; flex-direction:column; gap:5px; margin-bottom:6px; }
.ax-filtro { display:flex; align-items:flex-start; gap:4px; border:1px solid var(--line); border-radius:8px; padding:5px 5px 5px 9px; font-size:12.5px; }
.ax-filtro span { flex:1; }
.ax-nuevo-filtro { border:1px dashed var(--accent); border-radius:10px; padding:8px; display:flex; flex-direction:column; gap:6px; }
.ax-valores { max-height:200px; overflow:auto; }
.ax-valores .dx-check { margin-top:3px !important; font-size:12.5px; }
.ax-valores .dx-check span { flex:1; }
.ax-ayuda { font-size:11.5px; margin:4px 0; }
.ax-principal { display:flex; flex-direction:column; gap:16px; min-width:0; }
.ax-subtitulo { font-size:12.5px; margin-top:3px; }
.ax-kpis { display:grid; grid-template-columns:repeat(auto-fit, minmax(170px, 1fr)); gap:10px; margin:6px 0 12px; }
.ax-kpi { border:1px solid var(--line); border-radius:12px; padding:10px 12px; cursor:pointer; display:flex; flex-direction:column; gap:2px; }
.ax-kpi.activo { border-color:var(--accent); box-shadow:inset 0 0 0 1px var(--accent); }
.ax-kpi small { color:var(--muted); font-size:12px; }
.ax-kpi strong { font-size:20px; font-variant-numeric:tabular-nums; color:var(--ink); }
.ax-kpi span { font-size:12px; }
.ax-sube { color:var(--pos,#0f9d58); }
.ax-baja { color:var(--neg,#e0533d); }
.ax-neg { color:var(--neg,#e0533d); }
.ax-grafico { margin:0; }
.ax-lienzo { position:relative; }
.ax-grafico svg { width:100%; height:auto; display:block; }
.ax-rejilla { stroke:var(--ax-rejilla); stroke-width:1; }
.ax-cero { stroke:var(--muted); stroke-width:1; opacity:.6; }
.ax-cruz { stroke:var(--muted); stroke-width:1; stroke-dasharray:3 3; }
.ax-eje { fill:var(--muted); font-size:11px; }
.ax-etiqueta { fill:var(--ink); font-size:12px; }
.ax-valor { fill:var(--muted); font-size:11px; font-variant-numeric:tabular-nums; }
.ax-activa .ax-etiqueta { font-weight:700; }
.ax-leyenda { display:flex; flex-wrap:wrap; gap:6px 16px; font-size:12px; color:var(--ink); margin-bottom:6px; }
.ax-leyenda i, .ax-tooltip i { display:inline-block; width:10px; height:10px; border-radius:3px; margin-right:6px; vertical-align:-1px; }
.ax-leyenda i.anterior { height:3px; vertical-align:3px; }
.ax-tooltip { position:absolute; top:8px; right:8px; pointer-events:none; background:var(--surface,#fff); border:1px solid var(--line); border-radius:10px; box-shadow:0 8px 24px rgba(15,23,42,.14); padding:8px 10px; font-size:12px; min-width:180px; }
.ax-tooltip div { display:flex; align-items:center; gap:4px; margin-top:3px; color:var(--muted); }
.ax-tooltip b { margin-left:auto; color:var(--ink); font-variant-numeric:tabular-nums; }
.ax-vacio { padding:24px; text-align:center; }
.ax-tabla-panel { padding:0 !important; overflow:hidden; }
.ax-tabla-herr { display:flex; justify-content:space-between; gap:10px; padding:10px 14px; font-size:12.5px; border-bottom:1px solid var(--line); flex-wrap:wrap; }
.ax-scroll { overflow:auto; max-height:70vh; }
.ax-scroll-detalle { max-height:60vh; }
.ax-tabla { width:100%; border-collapse:separate; border-spacing:0; font-size:13px; }
.ax-tabla th { position:sticky; top:0; z-index:1; background:var(--surface,#fff); font-size:11.5px; text-transform:uppercase; letter-spacing:.03em; color:var(--muted); padding:8px 10px; text-align:left; border-bottom:1px solid var(--line); white-space:nowrap; }
.ax-tabla thead tr:nth-child(2) th { top:31px; }
.ax-tabla th.num, .ax-tabla td.num { text-align:right; }
.ax-th-col { text-align:center !important; border-left:1px solid var(--line); }
.ax-th-total, .ax-col-total { background:rgba(148,163,184,.08); font-weight:600; }
.ax-th-ant { color:var(--muted); font-weight:500; }
.ax-tabla td { padding:6px 10px; border-bottom:1px solid var(--line); white-space:nowrap; font-variant-numeric:tabular-nums; position:relative; }
.ax-tabla tfoot td { position:sticky; bottom:0; background:var(--surface,#fff); font-weight:700; border-top:2px solid var(--ink); border-bottom:none; }
.ax-subtotal td { font-weight:600; background:rgba(148,163,184,.06); }
.ax-n1.ax-subtotal td { background:rgba(8,145,178,.07); }
.ax-resto td { font-style:italic; }
.ax-dim { max-width:420px; overflow:hidden; text-overflow:ellipsis; }
.ax-dim-texto { cursor:pointer; }
.ax-dim-texto:hover { color:var(--accent); text-decoration:underline; }
.ax-plegar { border:none; background:none; cursor:pointer; color:var(--muted); width:18px; padding:0; margin-right:4px; font-size:12px; }
.ax-plegar-hueco { display:inline-block; width:22px; }
.ax-barra { position:absolute; right:10px; top:5px; bottom:5px; background:var(--ax-barra); border-radius:0 4px 4px 0; }
.ax-num { position:relative; }
.ax-clic { cursor:pointer; }
.ax-clic:hover td, td.ax-clic:hover { background:var(--accent-soft); }
.ax-capa { position:fixed; inset:0; z-index:200; background:rgba(15,23,42,.25); }
.ax-menu { position:fixed; width:250px; background:var(--surface,#fff); border:1px solid var(--line); border-radius:12px; box-shadow:0 16px 40px rgba(15,23,42,.2); padding:6px; display:flex; flex-direction:column; }
.ax-menu button { text-align:left; border:none; background:none; padding:8px 10px; border-radius:8px; cursor:pointer; font-size:13px; color:var(--ink); }
.ax-menu button:hover { background:var(--accent-soft); color:var(--accent); }
.ax-menu-tit { font-weight:700; font-size:12.5px; padding:6px 10px 8px; border-bottom:1px solid var(--line); margin-bottom:4px; }
.ax-menu-sub { font-size:11px; text-transform:uppercase; color:var(--muted); padding:8px 10px 2px; letter-spacing:.04em; }
.ax-menu-lista { max-height:170px; overflow:auto; display:flex; flex-direction:column; }
.ax-ventana { position:fixed; left:50%; top:6vh; transform:translateX(-50%); width:min(1100px, 94vw); max-height:88vh; overflow:auto; }
.ax-dialogo { position:fixed; left:50%; top:18vh; transform:translateX(-50%); width:min(440px, 94vw); }
.ax-dialogo label { display:block; margin:10px 0 5px; font-size:12.5px; color:var(--muted); }
.ax-dialogo input[type=text], .ax-dialogo input:not([type]) { width:100%; }
@media (max-width: 980px) { .ax-disenador { grid-template-columns:1fr; } .ax-lateral { position:static; max-height:none; } }
`;
