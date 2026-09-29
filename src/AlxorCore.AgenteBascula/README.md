# Agente de báscula

Servicio local que lee el indicador de la báscula y expone el último peso para la pantalla de pesadas de ALXOR
(Agro → Recepciones → pesada → **Leer báscula**).

- **Conexión:** por TCP (`Bascula:Tcp = host:puerto`, lo habitual con un conversor serie-Ethernet) o por el puerto
  serie (`Bascula:Puerto`, `Baudios`, `Paridad`, `BitsDatos`). Si el indicador solo envía bajo petición, pon el texto en
  `Bascula:Peticion` (p. ej. `SI\r\n` en Mettler Toledo SICS).
- **Tramas:** Dini Argeo, A&D, Gram (`ST,GS,+0001234kg`), Mettler Toledo SICS (`S S   12.345 kg`), y genéricas con el
  número y la unidad (kg, g, t). Inestable (`US`, `S D`) no se usa; sobrecarga o error no dan peso.
- **API local:** `GET http://localhost:5199/peso` → `{ bascula, kilos, estable, leidaEn, antiguedadSegundos }`;
  `GET /estado` → conexión y último error. Reconecta solo si se corta.
- **Arranque:** `dotnet AlxorCore.AgenteBascula.dll` (o publícalo como servicio de Windows o systemd). En el navegador,
  «Agente de báscula…» cambia la dirección si no es `http://localhost:5199`.
