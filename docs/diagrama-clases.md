# TransportTrack - Diagrama de Clases

## Diagrama UML General (Notación Mermaid)

```mermaid
classDiagram
    %% ==================== DOMAIN LAYER ====================
    class BaseEntity {
        <<abstract>>
        +int Id
        +bool IsDeleted
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        #BaseEntity()
        #BaseEntity(DateTime createdAt)
    }

    class Conductor {
        +string Nombre
        +string Licencia
        +string Telefono
        +ICollection~Vehiculo~ Vehiculos
        +Conductor()
        +Conductor(string nombre, string licencia, string telefono)
        +Conductor(int id, string nombre, string licencia, string telefono, DateTime createdAt, bool isDeleted)
    }

    class Vehiculo {
        +string Placa
        +string Marca
        +string Modelo
        +int Anio
        +int ConductorId
        +Conductor? Conductor
        +Vehiculo()
        +Vehiculo(string placa, string marca, string modelo, int anio, int conductorId)
        +Vehiculo(int id, string placa, string marca, string modelo, int anio, int conductorId, DateTime createdAt, bool isDeleted)
    }

    class Ruta {
        +string Origen
        +string Destino
        +int ConductorId
        +Conductor? Conductor
        +int VehiculoId
        +Vehiculo? Vehiculo
        +DateTime FechaSalida
        +DateTime? FechaLlegada
        +Ruta()
        +Ruta(string origen, string destino, int conductorId, int vehiculoId)
        +Ruta(int id, string origen, string destino, int conductorId, int vehiculoId, DateTime fechaSalida, DateTime? fechaLlegada, DateTime? createdAt, bool isDeleted)
    }

    BaseEntity <|-- Conductor
    BaseEntity <|-- Vehiculo
    BaseEntity <|-- Ruta

    Conductor "1" -- "*" Vehiculo : tiene
    Conductor "1" -- "*" Ruta : conduce
    Vehiculo "1" -- "*" Ruta : recorre

    %% ==================== INFRASTRUCTURE LAYER ====================
    class TransportTrackContext {
        +DbSet~Conductor~ Conductores
        +DbSet~Vehiculo~ Vehiculos
        +DbSet~Ruta~ Rutas
        +OnModelCreating(ModelBuilder)
    }

    class BaseRepository~TEntity~ {
        <<abstract>>
        #TransportTrackContext Context
        #DbSet~TEntity~ DbSet
        +BaseRepository(TransportTrackContext)
        +Task~List~TEntity~~ GetAllAsync()
        +Task~TEntity?~ GetByIdAsync(int id)
        +Task AddAsync(TEntity entity)
        +Task UpdateAsync(TEntity entity)
        +Task DeleteAsync(TEntity entity)
    }

    class IConductorRepository {
        <<interface>>
        +Task~List~Conductor~~ GetAllAsync()
        +Task~Conductor?~ GetByIdAsync(int id)
        +Task~Conductor~ AddAsync(ConductorModel model)
        +Task UpdateAsync(int id, ConductorModel model)
        +Task DeleteAsync(int id)
    }

    class IVehiculoRepository {
        <<interface>>
        +Task~List~Vehiculo~~ GetAllAsync()
        +Task~Vehiculo?~ GetByIdAsync(int id)
        +Task~Vehiculo~ AddAsync(VehiculoModel model)
        +Task UpdateAsync(int id, VehiculoModel model)
        +Task DeleteAsync(int id)
    }

    class IRutaRepository {
        <<interface>>
        +Task~List~Ruta~~ GetAllAsync()
        +Task~Ruta?~ GetByIdAsync(int id)
        +Task~Ruta~ AddAsync(RutaModel model)
        +Task UpdateAsync(int id, RutaModel model)
        +Task DeleteAsync(int id)
    }

    class ConductorRepository {
        +ConductorRepository(TransportTrackContext)
        +Task~Conductor~ AddAsync(ConductorModel model)
        +Task UpdateAsync(int id, ConductorModel model)
        +Task DeleteAsync(int id)
    }

    class VehiculoRepository {
        +VehiculoRepository(TransportTrackContext)
        +Task~List~Vehiculo~~ GetAllAsync()  // Override con Include
        +Task~Vehiculo?~ GetByIdAsync(int id) // Override con Include
        +Task~Vehiculo~ AddAsync(VehiculoModel model)
        +Task UpdateAsync(int id, VehiculoModel model)
        +Task DeleteAsync(int id)
    }

    class RutaRepository {
        +RutaRepository(TransportTrackContext)
        +Task~List~Ruta~~ GetAllAsync()  // Override con Includes
        +Task~Ruta?~ GetByIdAsync(int id) // Override con Includes
        +Task~Ruta~ AddAsync(RutaModel model)
        +Task UpdateAsync(int id, RutaModel model)
        +Task DeleteAsync(int id)
    }

    BaseRepository~Conductor~ <|-- ConductorRepository
    BaseRepository~Vehiculo~ <|-- VehiculoRepository
    BaseRepository~Ruta~ <|-- RutaRepository

    IConductorRepository <|.. ConductorRepository
    IVehiculoRepository <|.. VehiculoRepository
    IRutaRepository <|.. RutaRepository

    TransportTrackContext --> Conductor
    TransportTrackContext --> Vehiculo
    TransportTrackContext --> Ruta

    %% DTOs y Models (Infrastructure/Models)
    class ConductorDto {
        +int Id
        +string Nombre
        +string Licencia
        +string Telefono
    }

    class ConductorModel {
        +string Nombre
        +string Licencia
        +string Telefono
    }

    class VehiculoDto {
        +int Id
        +string Placa
        +string Marca
        +string Modelo
        +int Anio
        +int ConductorId
        +string? NombreConductor
    }

    class VehiculoModel {
        +string Placa
        +string Marca
        +string Modelo
        +int Anio
        +int ConductorId
    }

    class RutaDto {
        +int Id
        +string Origen
        +string Destino
        +int ConductorId
        +string? NombreConductor
        +int VehiculoId
        +string? PlacaVehiculo
        +DateTime FechaSalida
        +DateTime? FechaLlegada
    }

    class RutaModel {
        +string Origen
        +string Destino
        +int ConductorId
        +int VehiculoId
        +DateTime? FechaSalida
        +DateTime? FechaLlegada
    }

    %% Excepciones
    class ConductorException {
        +ConductorException(string message)
    }

    class VehiculoException {
        +VehiculoException(string message)
    }

    class RutaException {
        +RutaException(string message)
    }

    %% ==================== APPLICATION LAYER ====================
    class ServiceBase~TEntity~ {
        <<abstract>>
        #BaseRepository~TEntity~ Repository
        +ServiceBase(BaseRepository~TEntity~)
        +Task~List~TEntity~~ GetAllAsync()
        +Task~TEntity?~ GetByIdAsync(int id)
        +Task~TEntity~ AddAsync(TEntity entity)
        +Task UpdateAsync(TEntity entity)
        +Task DeleteAsync(int id)       // SOBRECARGA 1
        +Task DeleteAsync(TEntity entity) // SOBRECARGA 2
        +Task~bool~ ExistsAsync(int id)
    }

    class IConductorService {
        <<interface>>
        +Task~List~Conductor~~ GetAllAsync()
        +Task~Conductor?~ GetByIdAsync(int id)
        +Task~Conductor~ CreateAsync(ConductorModel model)
        +Task UpdateAsync(int id, ConductorModel model)
        +Task DeleteAsync(int id)
    }

    class IVehiculoService {
        <<interface>>
        +Task~List~Vehiculo~~ GetAllAsync()
        +Task~Vehiculo?~ GetByIdAsync(int id)
        +Task~Vehiculo~ CreateAsync(VehiculoModel model)
        +Task UpdateAsync(int id, VehiculoModel model)
        +Task DeleteAsync(int id)
    }

    class IRutaService {
        <<interface>>
        +Task~List~Ruta~~ GetAllAsync()
        +Task~Ruta?~ GetByIdAsync(int id)
        +Task~Ruta~ CreateAsync(RutaModel model)
        +Task UpdateAsync(int id, RutaModel model)
        +Task DeleteAsync(int id)
    }

    class ConductorService {
        -IConductorRepository _conductorRepository
        +ConductorService(IConductorRepository)
        +Task~Conductor~ CreateAsync(ConductorModel model)
        +Task UpdateAsync(int id, ConductorModel model)
    }

    class VehiculoService {
        -IVehiculoRepository _vehiculoRepository
        +VehiculoService(IVehiculoRepository)
        +Task~Vehiculo~ CreateAsync(VehiculoModel model)
        +Task UpdateAsync(int id, VehiculoModel model)
    }

    class RutaService {
        -IRutaRepository _rutaRepository
        +RutaService(IRutaRepository)
        +Task~Ruta~ CreateAsync(RutaModel model)
        +Task UpdateAsync(int id, RutaModel model)
    }

    ServiceBase~Conductor~ <|-- ConductorService
    ServiceBase~Vehiculo~ <|-- VehiculoService
    ServiceBase~Ruta~ <|-- RutaService

    IConductorService <|.. ConductorService
    IVehiculoService <|.. VehiculoService
    IRutaService <|.. RutaService

    ConductorService --> IConductorRepository
    VehiculoService --> IVehiculoRepository
    RutaService --> IRutaRepository

    %% ==================== API LAYER ====================
    class ConductoresController {
        -IConductorService _conductorService
        +ConductoresController(IConductorService)
        +ActionResult GetConductores()
        +ActionResult GetConductor(int id)
        +ActionResult CrearConductor(ConductorModel)
        +ActionResult ActualizarConductor(int, ConductorModel)
        +ActionResult EliminarConductor(int)
        -ConductorDto ToDto(Conductor)
    }

    class VehiculosController {
        -IVehiculoService _vehiculoService
        +VehiculosController(IVehiculoService)
        +ActionResult GetVehiculos()
        +ActionResult GetVehiculo(int id)
        +ActionResult CrearVehiculo(VehiculoModel)
        +ActionResult ActualizarVehiculo(int, VehiculoModel)
        +ActionResult EliminarVehiculo(int)
        -VehiculoDto ToDto(Vehiculo)
    }

    class RutasController {
        -IRutaService _rutaService
        -IConductorService _conductorService
        -IVehiculoService _vehiculoService
        +RutasController(IRutaService, IConductorService, IVehiculoService)
        +ActionResult GetRutas()
        +ActionResult GetRuta(int id)
        +ActionResult CrearRuta(RutaModel)
        +ActionResult ActualizarRuta(int, RutaModel)
        +ActionResult EliminarRuta(int)
        -RutaDto ToDto(Ruta)
    }

    ConductoresController --> IConductorService
    VehiculosController --> IVehiculoService
    RutasController --> IRutaService
    RutasController --> IConductorService
    RutasController --> IVehiculoService

    %% ==================== FRONTEND (React) ====================
    class App {
        +conductores: ConductorDto[]
        +vehiculos: VehiculoDto[]
        +rutas: RutaDto[]
        +activeTab: string
        +modalOpen: boolean
        +modalType: EntityType
        +editingId: number?
        +editingData: Record~string, unknown~?
        +loadData()
        +openModal(type, id?)
        +closeModal()
        +handleSubmit(data)
        +handleDelete(type, id)
        +handleEdit(type, id)
    }

    class Header {
        +stats: StatInfo[]
    }

    class Tabs {
        +activeTab: string
        +setActiveTab: (tab) => void
    }

    class Modal {
        +open: boolean
        +onClose: () => void
        +entityType: EntityType
        +entityId: number?
        +conductors: {id, nombre}[]
        +vehiculos: {id, placa, modelo}[]
        +initialData: Record~string, unknown~?
        +onSubmit: (data) => void
    }

    class ConductorSection {
        +conductores: ConductorDto[]
        +onOpenModal
        +onEdit
        +onDelete
    }

    class VehiculoSection {
        +vehiculos: VehiculoDto[]
        +onOpenModal
        +onEdit
        +onDelete
    }

    class RutaSection {
        +rutas: RutaDto[]
        +onOpenModal
        +onEdit
        +onDelete
    }

    class conductorApi {
        +getAll()
        +getById(id)
        +create(data)
        +update(id, data)
        +delete(id)
    }

    class vehiculoApi {
        +getAll()
        +getById(id)
        +create(data)
        +update(id, data)
        +delete(id)
    }

    class rutaApi {
        +getAll()
        +getById(id)
        +create(data)
        +update(id, data)
        +delete(id)
    }

    App --> Header
    App --> Tabs
    App --> Modal
    App --> ConductorSection
    App --> VehiculoSection
    App --> RutaSection
    App --> conductorApi
    App --> vehiculoApi
    App --> rutaApi

    ConductorSection --> ConductorDto
    VehiculoSection --> VehiculoDto
    RutaSection --> RutaDto
```

---

## Relaciones Principales Explicadas

### 1. Herencia (Domain Layer)
```
BaseEntity (abstract)
    ▲
    │
    ├── Conductor
    ├── Vehiculo
    └── Ruta
```
- `BaseEntity` define campos comunes: `Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`
- Constructores protegidos aseguran inicialización correcta
- `UpdatedAt` es nullable para auditoría opcional

### 2. Asociación (Domain Layer)
```
Conductor "1" ────── "*" Vehiculo
    │                   ▲
    │                   │ ConductorId (FK)
    ▼                   │
Conductor "1" ────── "*" Ruta
    │                   ▲
    │                   │ ConductorId (FK)
    ▼                   │
Vehiculo "1" ─────── "*" Ruta
                      │ VehiculoId (FK)
```

### 3. Patrón Repository (Infrastructure Layer)
```
IConductorRepository (interface)
        ▲
        │ implements
        │
ConductorRepository ──► BaseRepository<Conductor>
        │
        │ uses
        ▼
    TransportTrackContext
```
- `BaseRepository<TEntity>`: Implementación genérica de CRUD con soft delete
- Repositorios concretos: Overrides de `GetAllAsync()`/`GetByIdAsync()` con `Include()` para navegación

### 4. Patrón Service (Application Layer)
```
ServiceBase<TEntity> (abstract)
    ▲
    │ inherits
    │
ConductorService ◄── IConductorService (interface)
VehiculoService  ◄── IVehiculoService (interface)
RutaService      ◄── IRutaService (interface)
```
- `ServiceBase` proporciona operaciones genéricas + **sobrecarga de métodos** (`DeleteAsync`)
- Servicios concretos: Lógica de negocio, validaciones, transformación Model → Entity

### 5. API Controllers (Presentation Layer)
```
ConductoresController ──► IConductorService
VehiculosController   ──► IVehiculoService
RutasController       ──► IRutaService
                        IConductorService (para validación dropdown)
                        IVehiculoService (para validación dropdown)
```
- Controladores delgados: Solo HTTP → Service → DTO
- Manejo de excepciones mapeado a códigos HTTP

### 6. Frontend Components (React)
```
App (State Management)
    ├── Header (stats)
    ├── Tabs (navigation)
    ├── ConductorSection (CRUD table)
    ├── VehiculoSection (CRUD table)
    ├── RutaSection (CRUD table)
    └── Modal (create/edit form)
```
- `App`: Estado global, data fetching, event handlers
- `Modal`: Componente genérico reutilizable para las 3 entidades
- `*Section`: Componentes de presentación (tablas + acciones)

---

## Sobrecargas de Métodos (Overloading) Identificadas

| Clase | Método | Sobrecargas |
|-------|--------|-------------|
| `ServiceBase<T>` | `DeleteAsync` | 1. `DeleteAsync(int id)` 2. `DeleteAsync(TEntity entity)` |
| `BaseRepository<T>` | `GetByIdAsync` | Override en `VehiculoRepository` y `RutaRepository` con `Include()` |
| `BaseRepository<T>` | `GetAllAsync` | Override en `VehiculoRepository` y `RutaRepository` con `Include()` |
| `Conductor` | Constructores | 3 constructores (default, parámetros, copia completa) |
| `Vehiculo` | Constructores | 3 constructores |
| `Ruta` | Constructores | 3 constructores |

---

## Clases Abstractas

| Clase | Capa | Propósito |
|-------|------|-----------|
| `BaseEntity` | Domain | Campos comunes + constructores protegidos para entidades |
| `BaseRepository<T>` | Infrastructure | CRUD genérico con soft delete + EF Core |
| `ServiceBase<T>` | Application | Operaciones de servicio genéricas + sobrecargas |

---

## Notas de Implementación

1. **Inyección de Dependencias**: Registrada en `Program.cs` como `Scoped`
2. **Validaciones**: Data Annotations en entidades + validaciones de negocio en Services
3. **Soft Delete**: `IsDeleted` filtro global en `BaseRepository.GetAllAsync()`
4. **CORS**: Configurado para `http://localhost:5173` (Vite dev server)
5. **Proxy Vite**: `/api/*` → `http://localhost:5000` (API backend)