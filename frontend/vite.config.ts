/// <reference types="vitest/config" />
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

// La SPA se sirve bajo /app por el propio host .NET (wwwroot/app), conviviendo con la interfaz
// clásica en la raíz durante la transición. En desarrollo, /api y /auth se redirigen al backend.
export default defineConfig({
  plugins: [react()],
  base: "/app/",
  build: {
    outDir: "../src/AlxorCore.Api/wwwroot/app",
    emptyOutDir: true,
    sourcemap: false,
  },
  server: {
    proxy: {
      "/api": "http://localhost:5080",
      "/auth": "http://localhost:5080",
      "/empresas": "http://localhost:5080",
      "/facturas": "http://localhost:5080",
      "/clientes": "http://localhost:5080",
      "/productos": "http://localhost:5080",
    },
  },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: ["./src/test/setup.ts"],
  },
});
