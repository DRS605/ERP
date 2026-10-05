# Seguro de crédito

Base (Tesorería), rutas `/seguro-credito`, tablas `tesoreria.poliza_seguro`, `clasificacion_seguro` (con su historial
`cambio_clasificacion`) y `aviso_impago`.

- **Póliza**: aseguradora, número, cobertura (% del impagado que indemniza), plazo para avisar de un impago (días desde el
  vencimiento), vigencia y si se **bloquea la venta sin cobertura**.
- **Clasificación** de cada cliente: se solicita (importe y fecha) y se anota lo que comunica la aseguradora
  (concedida, reducida si es menos de lo pedido, denegada o anulada) con su fecha de efecto y, si es temporal, su
  vencimiento. El historial queda con cada cambio; un cambio no puede tener efecto anterior al último.
- **Al vender**: al facturar o confirmar un pedido a crédito (sin cobro automático) con una póliza vigente, si el cliente
  no tiene clasificación vigente, o su riesgo (pendiente de cobro y de facturar) más esta venta pasa de lo concedido, se
  avisa de cuánto queda sin cobertura (en el aviso de riesgo del documento). Si la póliza bloquea, no se hace
  (`seguro.sin_cobertura`) salvo con el permiso para forzar el riesgo.
- **Cartera asegurada** (`GET /seguro-credito/clasificaciones`): por cliente, lo concedido hoy, su riesgo, lo que queda
  sin cobertura y lo indemnizable (lo cubierto × la cobertura de la póliza).
- **Impagos a avisar** (`GET /seguro-credito/impagos`): las facturas vencidas con pendiente de clientes asegurados sin aviso
  abierto, con el límite para avisar (vencimiento + plazo de la póliza): en plazo, avisar ya (15 días o menos) o fuera de
  plazo.
- **Avisos de impago**: se anota el aviso (fecha y número de siniestro) y se cierra como cobrado, indemnizado (con el
  importe) o retirado.
