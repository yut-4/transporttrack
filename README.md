# TransportTrack 🚚

**Sistema de Gestión de Flotas de Transporte** - Proyecto final de Programación Orientada a Objetos.

Aplicación web completa para administrar **conductores**, **vehículos** y **rutas** con arquitectura distribuida, separación de capas y frontend moderno en React.

---

## 🏗️ Arquitectura

| Capa | Proyecto | Tecnología |
|------|----------|------------|
| **Presentación (Frontend)** | `TransportTrack.Web` | React 19 + TypeScript + Vite |
| **Presentación (API)** | `TransportTrack.Api` | ASP.NET Core 8 Web API |
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

## ✨ Funcionalidades

- **CRUD de Conductores**: nombre, licencia única, teléfono
- **CRUD de Vehículos**: placa única, marca, modelo, año, asignación a conductor
- **CRUD de Rutas**: origen, destino, conductor, vehículo, fechas
- **Dashboard** con estadísticas en tiempo real
- **Validaciones de negocio** centralizadas en la capa de servicios
- **Soft delete** (eliminación lógica) en todas las entidades
- **Swagger/OpenAPI** para documentación de la API
- **UI responsive** con modal de formularios

---

## 📐 Conceptos POO Aplicados

| Concepto | Implementación |
|----------|----------------|
| **Clases normales** | `Conductor`, `Vehiculo`, `Ruta` |
| **Clases abstractas** | `BaseEntity`, `BaseRepository<T>`, `ServiceBase<T>` |
| **Constructores** | 3 constructores por entidad (default, parámetros, completo) |
| **Sobrecarga (overloading)** | `ServiceBase<T>.DeleteAsync(int)` y `DeleteAsync(TEntity)` |
| **Herencia** | Entidades → `BaseEntity`; Servicios → `ServiceBase<T>` |
| **Polimorfismo** | Controladores dependen de interfaces |
| **Encapsulamiento** | Soft delete interno, DTOs, validaciones |

Documentación completa: [docs/requerimientos.md](docs/requerimientos.md) · [docs/diagrama-clases.md](docs/diagrama-clases.md) · [docs/presentacion.md](docs/presentacion.md)

---

## 🚀 Cómo ejecutar

### Requisitos
- .NET 10 SDK (o superior)
- Node.js 18+ y npm

### 1. Backend (API)

```bash
cp TransportTrack.Api/appsettings.example.json TransportTrack.Api/appsettings.json
cd TransportTrack.Api
dotnet run
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- La base de datos SQLite (`transporttrack.db`) se crea automáticamente

### 2. Frontend (React)

```bash
cd TransportTrack.Web
npm install
npm run dev
```

- Frontend: `http://localhost:5173`
- El proxy de Vite redirige `/api/*` a `http://localhost:5000`

---

## ☁️ Despliegue en Netlify

### Opción A: Deploy manual (arrastrar y soltar)

```bash
cd TransportTrack.Web
npm run build
```

Arrastra la carpeta `TransportTrack.Web/dist` a la consola de Netlify.

### Opción B: Conectar repositorio GitHub

1. Sube el proyecto a GitHub
2. En Netlify: **New site from Git**
3. Build command: `npm run build` (en `TransportTrack.Web`)
4. Publish directory: `dist`

La configuración ya está incluida en [`TransportTrack.Web/netlify.toml`](TransportTrack.Web/netlify.toml) con el redirect SPA incluido.

> **Nota:** En producción, el frontend debe apuntar a la URL de la API desplegada (no `localhost:5000`). Edita la `baseURL` en `TransportTrack.Web/src/api/client.ts` o usa variables de entorno `VITE_API_URL`.

---

## 📡 Endpoints API

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

---

## 🗂️ Estructura del Repositorio

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
├── TransportTrack.Api/             # Controladores REST, DI, Swagger, CORS
│   ├── Controllers/
│   └── Program.cs
├── TransportTrack.Web/             # Frontend React + TypeScript + Vite
│   ├── src/
│   │   ├── api/client.ts
│   │   ├── components/
│   │   ├── types/
│   │   └── App.tsx
│   └── netlify.toml
├── docs/                           # Documentación del proyecto
│   ├── requerimientos.md
│   ├── diagrama-clases.md
│   └── presentacion.md
└── README.md
```

---

## 🛠️ Herramientas de Desarrollo

| Herramienta | Comando |
|-------------|---------|
| Build .NET | `dotnet build` |
| Ejecutar API | `dotnet run --project TransportTrack.Api` |
| Install frontend | `cd TransportTrack.Web && npm install` |
| Dev frontend | `cd TransportTrack.Web && npm run dev` |
| Build frontend | `cd TransportTrack.Web && npm run build` |
| Lint frontend | `cd TransportTrack.Web && npm run lint` |

---

## 👨‍💻 Autor

Proyecto académico - Programación Orientada a Objetos