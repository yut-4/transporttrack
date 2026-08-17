# TransportTrack

**Sistema de Gestión de Flotas de Transporte** - Proyecto final de Programación Orientada a Objetos.

Aplicación web completa para administrar **conductores**, **vehículos** y **rutas** con arquitectura distribuida, separación de capas y frontend moderno en React.

---

## Arquitectura

| Capa | Proyecto | Tecnología |
|------|----------|------------|
| **Presentación (Frontend)** | `TransportTrack.Web` | React 19 + TypeScript + Vite |
| **Presentación (API)** | `TransportTrack.Api` | ASP.NET Core 10 Web API |
| **Lógica de Negocio** | `TransportTrack.Application` | Servicios, interfaces, `ServiceBase<T>` |
| **Acceso a Datos** | `TransportTrack.Infrastructure` | EF Core, Repositorios, Contexto |
| **Dominio** | `TransportTrack.Domain` | Entidades puras, `BaseEntity` |

```
TransportTrack.Web (React)  ──HTTP/REST──►  TransportTrack.Api  ──►  TransportTrack.Application  ──►  TransportTrack.Infrastructure  ──►  TransportTrack.Domain
                                                                                                        │
                                                                                                        ▼
                                                                                                  SQLite (transporttrack.db)
```

---

## Funcionalidades

- **CRUD de Conductores**: nombre, licencia única, teléfono
- **CRUD de Vehículos**: placa única, marca, modelo, año, asignación a conductor
- **CRUD de Rutas**: origen, destino, conductor, vehículo, fechas
- **Dashboard** con estadísticas en tiempo real
- **Seguimiento GPS en tiempo real**: el conductor comparte su ubicación (geolocalización del navegador o Capacitor en Android), con mapa MapLibre, lista de conductores con estado y KPIs
- **Realtime con SignalR**: la API notifica cada nueva ubicación al panel admin (`/hubs/tracking`), con fallback a polling
- **Modo offline (outbox)**: los pings sin conexión se acumulan en IndexedDB y se sincronizan al recuperar conexión
- **Temas Light / Dark / System** con persistencia en `localStorage`
- **Validaciones de negocio** centralizadas en la capa de servicios
- **Soft delete** (eliminación lógica) en todas las entidades
- **Swagger/OpenAPI** para documentación de la API
- **UI responsive** con modal de formularios y bottom nav móvil

---

## Conceptos POO Aplicados

| Concepto | Implementación |
|----------|----------------|
| **Clases normales** | `Conductor`, `Vehiculo`, `Ruta` |
| **Clases abstractas** | `BaseEntity`, `BaseRepository<T>`, `ServiceBase<T>` |
| **Constructores** | 3 constructores por entidad (default, parámetros, completo) |
| **Sobrecarga (overloading)** | `ServiceBase<T>.DeleteAsync(int)` y `DeleteAsync(TEntity)` |
| **Herencia** | Entidades → `BaseEntity`; Servicios → `ServiceBase<T>` |
| **Polimorfismo** | Controladores dependen de interfaces |
| **Encapsulamiento** | Soft delete interno, DTOs, validaciones |

Documentación completa: [docs/requerimientos.md](docs/requerimientos.md) · [docs/diagrama-clases.md](docs/diagrama-clases.md) · [docs/presentacion.md](docs/presentacion.md) · [docs/tracking.md](docs/tracking.md)

---

## Cómo ejecutar

### Requisitos
- .NET 10 SDK (o superior)
- Node.js 18+ y npm

### 1. Backend (API)

```bash
cd TransportTrack.Api
dotnet run
```

- API: `http://localhost:5000`
- Health check: `http://localhost:5000/api/health` → `{"status":"healthy"}`
- Swagger UI: `http://localhost:5000/swagger`
- La base de datos SQLite (`transporttrack.db`) se crea automáticamente (migraciones EF Core)

### 2. Frontend (React)

```bash
cd TransportTrack.Web
cp .env.example .env   # opcional: define VITE_* según el entorno
npm install
npm run dev
```

- Frontend: `http://localhost:5173`
- El proxy de Vite redirige `/api/*` a `http://localhost:5000`

### Variables de entorno

| Variable | Descripción |
|----------|-------------|
| `VITE_API_BASE_URL` | URL base de la API. Vacía en dev (proxy Vite); en producción apunta a la API desplegada (ej: `https://api.midominio.com`) |
| `VITE_SIGNALR_URL` | URL del hub SignalR (mismo host que la API). Vacía = usar polling como fallback |
| `VITE_MAP_STYLE_URL` | URL del style JSON de MapLibre (ej: `https://demotiles.maplibre.org/style.json`) |
| `VITE_ENABLE_GPS_SIMULATOR` | `true` para usar el simulador de GPS (solo desarrollo/pruebas) |
| `Cors__AllowedOrigins__N` | Origen permitido por CORS (formato `key:value`, ej: `Cors__AllowedOrigins__0=https://miapp.netlify.app`). Por defecto se permiten `localhost:5173` y `localhost:3000` |

---

## Despliegue en Netlify

### Opción A: Deploy manual (arrastrar y soltar)

```bash
cd TransportTrack.Web
npm run build
```

Arrastra la carpeta `TransportTrack.Web/dist` a la consola de Netlify.

### Opción B: Conectar repositorio GitHub (sin configuración)

1. Sube el proyecto a GitHub
2. En Netlify: **Add new site → Import an existing project → GitHub**
3. Selecciona el repositorio y pulsa **Deploy** (no hay que configurar nada más)

La configuración está en [`netlify.toml`](netlify.toml) en la raíz del repo: Netlify construye `TransportTrack.Web` (base) con `npm run build`, publica `dist`, despliega la API serverless en `netlify/functions` (con enrutado propio `/api/*`) y añade el redirect SPA. Solo necesitas `npm` y el `package-lock.json` ya versionado.

### Demo full-Netlify (sin backend .NET)

El repositorio incluye una demo completa ejecutándose solo en Netlify:

- **API serverless**: `TransportTrack.Web/netlify/functions` implementa el mismo contrato REST que la API .NET (conductores, vehículos, rutas y tracking: emparejar dispositivo, sesiones, pings, live, historial) con datos semilla.
- **Almacenamiento dual**: usa **Netlify Database (Postgres)** automáticamente cuando está disponible; si no, cae a **Netlify Blobs** (zero-config). No hace falta configurar nada.
- **Sin SignalR**: el live se actualiza por polling (10s).
- El frontend usa URLs relativas (`/api/...`), que Netlify enruta directamente a la función serverless, sin variables de entorno. En local se fuerza el modo demo con `VITE_USE_NETLIFY_DEMO=true`.

```bash
cd TransportTrack.Web
VITE_USE_NETLIFY_DEMO=true npm run dev   # emula Functions + Blobs + Database localmente
```

> **Nota:** La demo Netlify está pensada para el demo público. Para producción se recomienda el backend .NET con SignalR y la base SQLite (ver sección Despliegue).
>
> **Nota:** Para que el GPS funcione en producción con la API .NET, configura en Netlify (Site settings → Environment variables) `VITE_API_BASE_URL` y `VITE_SIGNALR_URL` apuntando a la API desplegada, y agrega el dominio de Netlify en `Cors__AllowedOrigins__0` del backend. En producción, el frontend no debe apuntar a `localhost:5000`.

---

## Endpoints API

### Conductores `/api/conductores`
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/conductores` | Listar todos |
| GET | `/api/conductores/{id}` | Obtener por ID |
| POST | `/api/conductores` | Crear |
| PUT | `/api/conductores/{id}` | Actualizar |
| DELETE | `/api/conductores/{id}` | Eliminar |

### Vehículos `/api/vehiculos`
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/vehiculos` | Listar todos |
| GET | `/api/vehiculos/{id}` | Obtener por ID |
| POST | `/api/vehiculos` | Crear |
| PUT | `/api/vehiculos/{id}` | Actualizar |
| DELETE | `/api/vehiculos/{id}` | Eliminar |

### Rutas `/api/rutas`
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/rutas` | Listar todas |
| GET | `/api/rutas/{id}` | Obtener por ID |
| POST | `/api/rutas` | Crear |
| PUT | `/api/rutas/{id}` | Actualizar |
| DELETE | `/api/rutas/{id}` | Eliminar |

### Tracking `/api/tracking`
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/tracking/devices/pair` | Emparejar dispositivo (requiere `X-Installation-Id`) |
| GET | `/api/tracking/conductores/{conductorId}/active-session` | Sesión activa del conductor |
| POST | `/api/tracking/sessions` | Iniciar sesión de seguimiento |
| POST | `/api/tracking/sessions/{sessionId}/stop` | Finalizar sesión |
| POST | `/api/tracking/pings` | Enviar un ping de ubicación |
| POST | `/api/tracking/pings/batch` | Enviar lote de pings (offline/outbox) |
| GET | `/api/tracking/live` | Ubicaciones recientes (con estado moving/stopped/stale/offline) |
| GET | `/api/tracking/conductores/{id}/latest` | Última ubicación de un conductor |
| GET | `/api/tracking/conductores/{id}/history` | Historial de ubicaciones de un conductor |

Los pings usan el header `X-Installation-Id` (obligatorio) para vincular el dispositivo con el conductor. Rate limiting: 20 pings/min por dispositivo, 5 emparejamientos/min.

---

## Docker

```bash
docker build -t transporttrack .
docker run -p 5000:80 -e "ASPNETCORE_URLS=http://+:80" transporttrack
```

El contenedor ejecuta las migraciones de la base de datos al iniciar.

---

## Aplicación móvil (Capacitor)

El frontend también se puede empaquetar como app Android/iOS con Capacitor (la configuración está en `TransportTrack.Web/capacitor.config.ts`, appId `com.transporttrack.app`). El proveedor de GPS usa automáticamente el plugin `@capacitor/geolocation` cuando corre dentro de la app y `navigator.geolocation` en el navegador.

```bash
cd TransportTrack.Web
npm run build
npx cap add android        # genera la carpeta android/ (una vez)
npx cap copy android
npx cap open android       # abre Android Studio para compilar/instalar
```

La app necesita los permisos de ubicación (`ACCESS_FINE_LOCATION` / `ACCESS_COARSE_LOCATION`) en `AndroidManifest.xml`, y en producción apuntar `VITE_API_BASE_URL` / `VITE_SIGNALR_URL` a la API desplegada.

---

## Pruebas

### Backend (xUnit)

```bash
dotnet test
```

Cubre emparejamiento de dispositivos, sesiones, validación de pings, servicio de ubicaciones y los endpoints del `TrackingController` (usando `WebApplicationFactory`).

### E2E (Playwright)

```bash
cd TransportTrack.Web
npx playwright test
```

Levanta la API y el frontend automáticamente. Cubre el tema Light/Dark/System y el flujo completo de GPS (conductor transmite, admin ve el marker moverse).

---

## Estructura del Repositorio

```
transporttrack/
├── TransportTrack.Domain/          # Entidades, BaseEntity abstracta
│   ├── Core/BaseEntity.cs
│   └── Entities/ (Conductor, Vehiculo, Ruta)
├── TransportTrack.Infrastructure/  # EF Core, Repositorios, Contexto, DTOs
│   ├── Context/TransportTrackContext.cs
│   ├── Core/BaseRepository.cs
│   ├── Interfaces/
│   ├── Models/ (DTOs y Models de entrada)
│   └── Repositories/
├── TransportTrack.Application/     # Servicios, lógica de negocio
│   ├── Core/ServiceBase.cs
│   ├── Interfaces/
│   └── Services/
├── TransportTrack.Api/             # Controladores REST, DI, Swagger, CORS, SignalR
│   ├── Controllers/
│   ├── Hubs/
│   └── Program.cs
├── TransportTrack.Web/             # Frontend React + TypeScript + Vite
│   ├── src/
│   │   ├── api/client.ts
│   │   ├── components/
│   │   ├── features/tracking/      # GPS, SignalR, outbox, mapa, páginas conductor/admin
│   │   ├── theme/                  # Tema Light/Dark/System (CSS variables)
│   │   ├── types/
│   │   └── App.tsx
│   ├── e2e/                        # Pruebas Playwright (tema + flujo GPS)
│   └── netlify.toml
├── TransportTrack.Tests/           # Tests xUnit del backend
├── docs/                           # Documentación del proyecto
│   ├── requerimientos.md
│   ├── diagrama-clases.md
│   └── presentacion.md
└── README.md
```

---

## Herramientas de Desarrollo

| Herramienta | Comando |
|-------------|---------|
| Build .NET | `dotnet build` |
| Ejecutar API | `dotnet run --project TransportTrack.Api` |
| Test backend | `dotnet test` |
| Install frontend | `cd TransportTrack.Web && npm install` |
| Dev frontend | `cd TransportTrack.Web && npm run dev` |
| Build frontend | `cd TransportTrack.Web && npm run build` |
| Lint frontend | `cd TransportTrack.Web && npm run lint` |
| Test E2E | `cd TransportTrack.Web && npx playwright test` |

---

## Autor

Proyecto académico - Programación Orientada a Objetos