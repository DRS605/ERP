import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Análisis de datos (diseñador de informes) que la interfaz clásica monta en su vista: librería ES con React
// incluido, servida por el host .NET en /app-analisis/analisis.js.
export default defineConfig({
  plugins: [react()],
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  build: {
    outDir: "../src/AlxorCore.Api/wwwroot/app-analisis",
    emptyOutDir: true,
    sourcemap: false,
    lib: { entry: "src/analisis/montar.tsx", formats: ["es"], fileName: () => "analisis.js" },
  },
});
