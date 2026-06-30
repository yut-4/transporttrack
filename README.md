# TransportTrack

CRUD Web API en ASP.NET Core con arquitectura por capas, repositorios, Entity Framework Core y SQLite.

## Estructura

```txt
TransportTrack.Domain
├── Core
│   └── BaseEntity.cs
└── Entities
    ├── Conductor.cs
    └── Vehiculo.cs

TransportTrack.Infrastructure
├── Context
│   └── TransportTrackContext.cs
├── Core
│   └── BaseRepository.cs
├── Exceptions
│   ├── ConductorException.cs
│   └── VehiculoException.cs
├── Interfaces
│   ├── IConductorRepository.cs
│   └── IVehiculoRepository.cs
├── Models
│   ├── ConductorDto.cs
│   ├── ConductorModel.cs
│   ├── VehiculoDto.cs
│   └── VehiculoModel.cs
└── Repositories
    ├── ConductorRepository.cs
    └── VehiculoRepository.cs

TransportTrack.Api
├── Controllers
│   ├── ConductoresController.cs
│   └── VehiculosController.cs
└── Program.cs
```

## Entidades

- Conductores
- Vehiculos

Cada vehiculo pertenece a un conductor.

## Ejecutar

```bash
cp TransportTrack.Api/appsettings.example.json TransportTrack.Api/appsettings.json
dotnet restore
dotnet run --project TransportTrack.Api
```

La base de datos SQLite se crea automaticamente como `transporttrack.db` al iniciar la API. El archivo `appsettings.json` no se sube al repositorio para evitar publicar claves o cadenas de conexion reales.

## Endpoints

- `GET /api/conductores`
- `GET /api/conductores/{id}`
- `POST /api/conductores`
- `PUT /api/conductores/{id}`
- `DELETE /api/conductores/{id}`
- `GET /api/vehiculos`
- `GET /api/vehiculos/{id}`
- `POST /api/vehiculos`
- `PUT /api/vehiculos/{id}`
- `DELETE /api/vehiculos/{id}`
