import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Módulo de documentos (ventas y compras) que la interfaz clásica monta dentro de su vista: una librería ES con
// React incluido, servida por el host .NET en /app-docs/documentos.js.
export default defineConfig({
  plugins: [react()],
  define: { "process.env.NODE_ENV": JSON.stringify("production") },
  build: {
    outDir: "../src/AlxorCore.Api/wwwroot/app-docs",
    emptyOutDir: true,
    sourcemap: false,
    lib: { entry: "src/documentos/montar.tsx", formats: ["es"], fileName: () => "documentos.js" },
  },
});
