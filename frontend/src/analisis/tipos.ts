/** Tipos del análisis (espejo de los DTO de /analisis). */
export type TipoDato = "Texto" | "Fecha" | "Numero" | "Moneda" | "Porcentaje";

export interface Campo { clave: string; nombre: string; grupo: string; tipo: TipoDato; aditiva?: boolean; descripcion?: string | null }

export interface Dataset {
  clave: string; nombre: string; descripcion: string;
  dimensiones: Campo[]; medidas: Campo[];
  filasDefecto: string[]; medidasDefecto: string[]; detalle: boolean; vistaDocumento?: string | null;
}

export interface Filtro { dimension: string; operador: string; valores?: string[] }
export interface FiltroMedida { medida: string; operador: "mayor" | "menor" | "entre"; valor: number; hasta?: number | null }

export interface Consulta {
  dataset: string; filas?: string[]; columna?: string | null; medidas?: string[];
  filtros?: Filtro[]; desde?: string | null; hasta?: string | null; comparar?: string | null;
  ordenarPor?: string | null; ascendente?: boolean; limite?: number | null; filtrosMedida?: FiltroMedida[];
}

export interface Plantilla { clave: string; nombre: string; descripcion: string; periodo: string; consulta: Consulta; grafico?: string | null }

export interface Columna { clave: string; nombre: string; tipo: TipoDato; aditiva: boolean }

export interface Fila { nivel: number; claves: (string | null)[]; valores: (number | null)[]; anteriores?: (number | null)[] | null; celdas?: (number | null)[][] | null; resto: boolean }

export interface Resultado {
  dataset: string; titulo: string; desde?: string | null; hasta?: string | null; desdeAnterior?: string | null; hastaAnterior?: string | null;
  dimensiones: Columna[]; medidas: Columna[]; columna?: Columna | null; valoresColumna: (string | null)[]; filas: Fila[]; truncado: boolean;
}

export interface Detalle { columnas: Columna[]; filas: unknown[][]; ids: (string | null)[]; vistaDocumento?: string | null; truncado: boolean }

export interface InformeGuardado { id: string; nombre: string; dataset: string; definicion: string; compartido: boolean; favorito: boolean; propio: boolean; actualizadoEn: string }

/** Lo que se guarda de un informe: la consulta, el periodo relativo y el gráfico. */
export interface Definicion extends Consulta { periodo: string; grafico?: string | null; medidaGrafico?: string | null }
