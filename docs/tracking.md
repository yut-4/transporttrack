# TransportTrack - Seguimiento GPS en Tiempo Real

Documentación de la arquitectura de seguimiento en tiempo real implementada en el sistema de gestión de flotas.

---

## Visión General

El módulo de tracking permite que un **conductor** comparta su ubicación GPS desde el navegador (o desde una app Android con Capacitor) y que el **panel admin** vea en un mapa MapLibre la posición en tiempo real de todos los conductores activos, con su estado (en movimiento / detenido / sin datos).

```
Conductor (Browser/Android)          API (.NET + SignalR)                Admin (Browser)
        │                                     │                               │
  watchPosition ──► outbox (IndexedDB)        │                               │
        │                                     │                               │
  POST /api/tracking/pings/batch ──────────────► valida + persiste            │
        │                                     │                               │
        │                          SignalR "LocationUpdate" ──────────────────► mapa
        │                                     │                               │
        │                          (fallback) GET /api/tracking/live ─────────►
```

---

## Entidades de Dominio

| Entidad | Propósito |
|---------|-----------|
| `TrackingDevice` | Identifica un dispositivo (navegador/app) vinculado a un conductor por `InstallationId` |
| `TrackingSession` | Período de seguimiento de un conductor (estados `active` / `completed`) |
| `LocationPing` | Una ubicación puntual del conductor (lat, lng, precisión, velocidad, timestamp) |

Relaciones:

- `Conductor 1─N TrackingDevice` (un dispositivo por instalación)
- `Conductor 1─N TrackingSession` (una sola sesión activa a la vez)
- `TrackingSession 1─N LocationPing`

---

## Flujo del Conductor

1. **Emparejamiento**: el dispositivo genera un `InstallationId` (persistido en `localStorage`) y llama a `POST /api/tracking/devices/pair` con `conductorId`.
2. **Sesión**: `POST /api/tracking/sessions` inicia el seguimiento (verifica que no exista una sesión activa).
3. **Pings**: `navigator.geolocation.watchPosition` entrega posiciones. Cada ping se valida y se envía en lote a `POST /api/tracking/pings/batch` con el header `X-Installation-Id`.

### Control de frecuencia (throttling)

Para no saturar la API ni la batería:

- Se envía una posición solo si pasaron **≥ 10 s** desde el último envío **y** el conductor se movió **≥ 15 m**.
- Si no hay movimiento, se envía un **heartbeat** cada **60 s** para mantener la sesión "activa".
- Cada ping lleva un `clientPingId` (UUID) que hace la operación **idempotente** (la API ignora duplicados).

### Modo offline (outbox)

Si no hay conexión, los pings se guardan en **IndexedDB** (`outbox`). Al volver la conexión, se vacían en lotes de hasta 100. La API responde con `accepted`/`duplicated`/`rejected` por ping, y el frontend descarta los confirmados.

### Simulador de GPS

Con `VITE_ENABLE_GPS_SIMULATOR=true` el frontend genera posiciones simuladas (útil para demos y pruebas en desarrollo). Usa el mismo pipeline de throttling y outbox.

---

## API (TrackingService)

| Endpoint | Descripción |
|----------|-------------|
| `POST /api/tracking/devices/pair` | Emparejar dispositivo con conductor |
| `GET /api/tracking/conductores/{id}/active-session` | Sesión activa (o null) |
| `POST /api/tracking/sessions` | Iniciar sesión |
| `POST /api/tracking/sessions/{id}/stop` | Finalizar sesión |
| `POST /api/tracking/pings` | Enviar 1 ping |
| `POST /api/tracking/pings/batch` | Enviar lote (outbox) |
| `GET /api/tracking/live` | Ubicaciones recientes (5 min) con estado |
| `GET /api/tracking/conductores/{id}/latest` | Última ubicación |
| `GET /api/tracking/conductores/{id}/history` | Historial |

Reglas de validación de un ping (input no confiable):

- `latitude` entre -90 y 90, `longitude` entre -180 y 180
- `accuracyMeters >= 0`, `speedMetersPerSecond >= 0`, `headingDegrees` en [0, 360]
- La sesión debe existir, estar activa y pertenecer al mismo conductor que el dispositivo
- `clientPingId` no repetido (duplicados se ignoran, respuesta idempotente)

### Estados de ubicación en `/live`

| Estado | Regla |
|--------|-------|
| `moving` | última posición hace < 90 s y velocidad > 0 |
| `stopped` | última posición hace < 90 s y velocidad = 0 |
| `stale` | última posición hace 90 s - 5 min |
| `offline` | última posición hace > 5 min |

### Rate limiting

- `tracking-device`: 20 pings/min por dispositivo
- `pair`: 5 emparejamientos/min

---

## Realtime (SignalR)

- Hub: `/hubs/tracking`
- El cliente llama `GetLive()` para obtener el estado inicial (lista de `DriverLocationDto`).
- La API hace broadcast de `LocationUpdate` (UNA ubicación `DriverLocation`) tras cada ping aceptado.
- El admin mergea por `conductorId` en el cliente (sin estado global).
- Si `VITE_SIGNALR_URL` está vacío (o SignalR falla), el frontend usa **polling** de `GET /api/tracking/live` cada 10 s.

---

## Frontend (React)

| Archivo | Rol |
|---------|-----|
| `features/tracking/types.ts` | Tipos (LocationSample, DriverLocation, estados) |
| `features/tracking/api.ts` | Cliente HTTP + `X-Installation-Id` |
| `features/tracking/locationProvider.ts` | Browser y Capacitor (Geolocation) |
| `features/tracking/outbox.ts` | Cola IndexedDB offline |
| `features/tracking/trackingHub.ts` | Cliente SignalR |
| `features/tracking/useLiveTracking.ts` | Hook admin (SignalR + polling fallback) |
| `features/tracking/useDriverTracking.ts` | Hook conductor (watchPosition + throttle + outbox) |
| `features/tracking/TrackingMap.tsx` | Mapa MapLibre con markers |
| `features/tracking/GpsPage.tsx` | Vista admin (lista + mapa + KPIs) |
| `features/tracking/DriverTrackingPage.tsx` | Vista conductor (`/tracking`) |

Rutas: `/tracking` (conductor) y el tab **GPS** del panel admin.

---

## Seguridad y notas

- La API solo acepta pings de dispositivos emparejados (`X-Installation-Id`), nunca datos anónimos.
- La base de datos SQLite se crea con migraciones EF Core al iniciar la API.
- En producción, el backend debe desplegarse con HTTPS y el frontend configurado con `VITE_API_BASE_URL` / `VITE_SIGNALR_URL` (ver `README.md`).