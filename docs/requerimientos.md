# TransportTrack - Documento de Requerimientos

## 1. Información General del Proyecto

**Nombre del Sistema:** TransportTrack - Sistema de Gestión de Flotas
**Versión:** 1.0
**Fecha:** Agosto 2026
**Tecnologías:** .NET 10 (Backend), React 19 + TypeScript + Vite (Frontend), SQLite (Base de Datos)

---

## 2. Descripción del Problema

Las empresas de transporte y logística necesitan una herramienta centralizada para gestionar sus recursos operativos: conductores, vehículos y rutas. Actualmente, muchas utilizan hojas de cálculo o sistemas dispares que generan:
- Duplicidad de datos
- Dificultad para consultar información en tiempo real
- Falta de trazabilidad en las asignaciones
- Problemas de integridad referencial (vehículos sin conductor asignado, rutas con recursos inexistentes)

---

## 3. Objetivos del Sistema

### Objetivo General
Desarrollar una aplicación web completa para la gestión integral de flotas de transporte que permita administrar conductores, vehículos y rutas con arquitectura distribuida y separación de capas.

### Objetivos Específicos
1. **Gestión de Conductores:** CRUD completo con validación de licencia única
2. **Gestión de Vehículos:** CRUD completo con validación de placa única y asignación a conductores
3. **Gestión de Rutas:** CRUD completo vinculando conductores y vehículos con fechas
4. **Dashboard Unificado:** Vista consolidada con estadísticas en tiempo real
5. **Arquitectura Limpia:** Separación en 4 capas (Domain, Infrastructure, Application, API)
6. **Frontend Moderno:** React con TypeScript, Vite, despliegue en Netlify

---

## 4. Actores del Sistema

| Actor | Descripción | Permisos |
|-------|-------------|----------|
| Administrador de Flotas | Usuario principal que gestiona recursos | CRUD completo en todas las entidades |
| Sistema Externo | Otros sistemas que consumen la API | Solo lectura (GET) mediante API REST |

---

## 5. Requerimientos Funcionales

### RF-01: Gestión de Conductores
| ID | Requerimiento | Prioridad |
|----|---------------|-----------|
| RF-01.1 | Crear conductor (nombre, licencia única, teléfono) | Alta |
| RF-01.2 | Listar conductores con paginación | Alta |
| RF-01.3 | Buscar conductor por ID | Alta |
| RF-01.4 | Actualizar datos de conductor | Alta |
| RF-01.5 | Eliminar conductor (validar que no tenga vehículos asignados) | Alta |
| RF-01.6 | Validar unicidad de licencia | Alta |

### RF-02: Gestión de Vehículos
| ID | Requerimiento | Prioridad |
|----|---------------|-----------|
| RF-02.1 | Crear vehículo (placa única, marca, modelo, año, conductor) | Alta |
| RF-02.2 | Listar vehículos con datos del conductor | Alta |
| RF-02.3 | Buscar vehículo por ID | Alta |
| RF-02.4 | Actualizar datos de vehículo | Alta |
| RF-02.5 | Eliminar vehículo | Alta |
| RF-02.6 | Validar unicidad de placa | Alta |
| RF-02.7 | Validar existencia de conductor asignado | Alta |

### RF-03: Gestión de Rutas
| ID | Requerimiento | Prioridad |
|----|---------------|-----------|
| RF-03.1 | Crear ruta (origen, destino, conductor, vehículo, fechas) | Alta |
| RF-03.2 | Listar rutas con datos completos | Alta |
| RF-03.3 | Buscar ruta por ID | Alta |
| RF-03.4 | Actualizar ruta | Alta |
| RF-03.5 | Eliminar ruta | Alta |
| RF-03.6 | Validar existencia de conductor y vehículo | Alta |

### RF-04: Dashboard y Reportes
| ID | Requerimiento | Prioridad |
|----|---------------|-----------|
| RF-04.1 | Mostrar contador de conductores activos | Media |
| RF-04.2 | Mostrar contador de vehículos registrados | Media |
| RF-04.3 | Mostrar contador de rutas activas | Media |
| RF-04.3 | Navegación por pestañas (Dashboard/Conductores/Vehículos/Rutas) | Media |

---

## 6. Requerimientos No Funcionales

| ID | Requerimiento | Descripción |
|----|---------------|-------------|
| RNF-01 | **Arquitectura** | Arquitectura por capas (Domain, Infrastructure, Application, API) |
| RNF-02 | **Base de Datos** | SQLite con Entity Framework Core 10 |
| RNF-03 | **API** | RESTful con Swagger/OpenAPI |
| RNF-04 | **Frontend** | React 19 + TypeScript + Vite |
| RNF-05 | **Despliegue Frontend** | Compatible con Netlify (SPA) |
| RNF-06 | **CORS** | Configurado para desarrollo local (localhost:5173) |
| RNF-07 | **Validaciones** | Data Annotations + validaciones de negocio en capa Application |
| RNF-08 | **Soft Delete** | Eliminación lógica (IsDeleted) en todas las entidades |
| RNF-09 | **POO** | Herencia, clases abstractas, constructores, sobrecarga de métodos |
| RNF-10 | **Responsive** | UI adaptable a móviles (< 600px) |

---

## 7. Modelo de Datos

### Entidades Principales

#### Conductor
- `Id` (PK, int, auto-increment)
- `Nombre` (string, required, max 100)
- `Licencia` (string, required, unique, max 20)
- `Telefono` (string, max 20)
- `IsDeleted` (bool, soft delete)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime?)

#### Vehiculo
- `Id` (PK, int, auto-increment)
- `Placa` (string, required, unique, max 20)
- `Marca` (string, required, max 60)
- `Modelo` (string, required, max 60)
- `Anio` (int, required, 1900-2100)
- `ConductorId` (FK → Conductor.Id)
- `IsDeleted` (bool, soft delete)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime?)

#### Ruta
- `Id` (PK, int, auto-increment)
- `Origen` (string, required, max 100)
- `Destino` (string, required, max 100)
- `ConductorId` (FK → Conductor.Id)
- `VehiculoId` (FK → Vehiculo.Id)
- `FechaSalida` (DateTime)
- `FechaLlegada` (DateTime?, nullable)
- `IsDeleted` (bool, soft delete)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime?)

---

## 8. Endpoints API

### Conductores (`/api/conductores`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/conductores` | Listar todos |
| GET | `/api/conductores/{id}` | Obtener por ID |
| POST | `/api/conductores` | Crear nuevo |
| PUT | `/api/conductores/{id}` | Actualizar |
| DELETE | `/api/conductores/{id}` | Eliminar (soft) |

### Vehículos (`/api/vehiculos`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/vehiculos` | Listar todos (con conductor) |
| GET | `/api/vehiculos/{id}` | Obtener por ID |
| POST | `/api/vehiculos` | Crear nuevo |
| PUT | `/api/vehiculos/{id}` | Actualizar |
| DELETE | `/api/vehiculos/{id}` | Eliminar (soft) |

### Rutas (`/api/rutas`)
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/rutas` | Listar todas (con conductor y vehículo) |
| GET | `/api/rutas/{id}` | Obtener por ID |
| POST | `/api/rutas` | Crear nueva |
| PUT | `/api/rutas/{id}` | Actualizar |
| DELETE | `/api/rutas/{id}` | Eliminar (soft) |

---

## 9. Arquitectura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                        FRONTEND (React)                     │
│  ┌─────────┐  ┌────────┐  ┌──────────┐  ┌───────────────┐  │
│  │ Header  │  │ Tabs   │  │ Sections │  │ Modal (CRUD)  │  │
│  └─────────┘  └────────┘  └──────────┘  └───────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP/REST + JSON
┌──────────────────────────▼──────────────────────────────────┐
│                      BACKEND (.NET 10 API)                   │
│  ┌──────────────┐  ┌─────────────────┐  ┌───────────────┐  │
│  │ Controllers  │──│  Services       │──│  Repositories │  │
│  │ (API Layer)  │  │ (App Layer)     │  │ (Infra Layer) │  │
│  └──────────────┘  └─────────────────┘  └───────────────┘  │
│         │                   │                    │           │
│         ▼                   ▼                    ▼           │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              DOMAIN LAYER (Entidades)                │  │
│  │  Conductor  │  Vehiculo  │  Ruta  │  BaseEntity     │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │ EF Core
┌──────────────────────────▼──────────────────────────────────┐
│                     DATABASE (SQLite)                       │
└─────────────────────────────────────────────────────────────┘
```

### Separación de Capas

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Domain** | `TransportTrack.Domain` | Entidades puras, reglas de negocio núcleo, `BaseEntity` abstracta |
| **Infrastructure** | `TransportTrack.Infrastructure` | Acceso a datos (EF Core), Repositorios, Contexto BD, DTOs/Models |
| **Application** | `TransportTrack.Application` | Servicios de aplicación, lógica de negocio, interfaces de servicios, `ServiceBase` abstracta |
| **API** | `TransportTrack.Api` | Controladores REST, configuración DI, Swagger, CORS |
| **Frontend** | `TransportTrack.Web` | React SPA, consumo API, UI/UX |

---

## 10. Conceptos POO Aplicados

| Concepto | Implementación |
|----------|----------------|
| **Clases Normales** | `Conductor`, `Vehiculo`, `Ruta` (entidades de dominio) |
| **Clases Abstractas** | `BaseEntity` (Domain), `ServiceBase<T>` (Application) |
| **Constructores** | Constructores por defecto, con parámetros, y de copia en todas las entidades |
| **Herencia** | Entidades heredan de `BaseEntity`; Servicios heredan de `ServiceBase<T>` |
| **Sobrecarga (Overloading)** | `ServiceBase.DeleteAsync(int id)` y `DeleteAsync(TEntity entity)`; `BaseRepository.GetByIdAsync` sobrecargado en repositorios derivados |
| **Polimorfismo** | Repositorios implementan interfaces; Servicios implementan interfaces |
| **Encapsulamiento** | Propiedades con validaciones (Data Annotations), soft delete interno |
| **Inyección de Dependencias** | Registros scoped en `Program.cs` |

---

## 11. Despliegue

### Backend (API)
```bash
cd TransportTrack.Api
dotnet run --environment Development
# Se ejecuta en http://localhost:5000 / https://localhost:5001
# Swagger UI disponible en /swagger
```

### Frontend (Desarrollo)
```bash
cd TransportTrack.Web
npm install
npm run dev
# Se ejecuta en http://localhost:5173
# Proxy configurado hacia http://localhost:5000
```

### Frontend (Producción - Netlify)
```bash
cd TransportTrack.Web
npm run build
# Genera carpeta dist/ lista para Netlify
```

**Configuración Netlify (`netlify.toml`):**
```toml
[build]
  command = "npm run build"
  publish = "dist"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

---

## 12. Validaciones y Reglas de Negocio

1. **Conductor:** Licencia única, no eliminar si tiene vehículos asignados
2. **Vehículo:** Placa única, conductor debe existir y no estar eliminado
3. **Ruta:** Conductor y vehículo deben existir y no estar eliminados
4. **Soft Delete:** Todas las entidades usan `IsDeleted` en lugar de borrado físico
5. **Timestamps:** `CreatedAt` automático, `UpdatedAt` opcional para auditoría

---

## 13. Supuestos y Restricciones

- Base de datos SQLite local (archivo `transporttrack.db`)
- Un conductor puede tener múltiples vehículos
- Un vehículo pertenece a un solo conductor
- Una ruta vincula exactamente un conductor y un vehículo
- No hay autenticación/autorización en esta versión (MVP)
- CORS configurado solo para desarrollo local

---

## 14. Pruebas de Aceptación

| Escenario | Resultado Esperado |
|-----------|-------------------|
| Crear conductor con licencia duplicada | Error 400 "Ya existe un conductor..." |
| Eliminar conductor con vehículos | Error 400 "No se puede eliminar..." |
| Crear vehículo con placa duplicada | Error 400 "Ya existe un vehículo..." |
| Asignar vehículo a conductor inexistente | Error 400 "El conductor indicado no existe" |
| Crear ruta con recursos inexistentes | Error 400 correspondiente |
| Frontend carga dashboard | Muestra 3 tarjetas con contadores |
| Frontend CRUD conductor | Modal abre, valida, guarda, actualiza tabla |
| Frontend CRUD vehículo | Modal con dropdown de conductores |
| Frontend CRUD ruta | Modal con dropdowns de conductor y vehículo |

---

**Fin del Documento de Requerimientos**