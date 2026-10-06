// Service worker de la app de planta: guarda la app para abrirla sin conexión. Los datos y la cola de volcados los
// guarda la propia app (localStorage); las llamadas a la API nunca se sirven de la caché.
const CACHE = "planta-v2";
const APP = ["/planta/index.html", "/planta/manifest.json", "/planta/icono.svg"];

self.addEventListener("install", e => {
  e.waitUntil(caches.open(CACHE).then(c => c.addAll(APP)).then(() => self.skipWaiting()));
});

self.addEventListener("activate", e => {
  e.waitUntil(caches.keys().then(ks => Promise.all(ks.filter(k => k !== CACHE).map(k => caches.delete(k)))).then(() => self.clients.claim()));
});

self.addEventListener("fetch", e => {
  const url = new URL(e.request.url);
  if (e.request.method !== "GET" || url.origin !== location.origin || !url.pathname.startsWith("/planta/")) return;
  // Red primero (para tener siempre la última versión) y la caché si no hay conexión.
  e.respondWith(fetch(e.request).then(r => {
    if (r.ok) { const copia = r.clone(); caches.open(CACHE).then(c => c.put(e.request, copia)); }
    return r;
  }).catch(() => caches.match(e.request, { ignoreSearch: true }).then(r => r || caches.match("/planta/index.html"))));
});
