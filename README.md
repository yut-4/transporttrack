# TransportTrack

CRUD Web API en ASP.NET Core con arquitectura por capas, repositorios, Entity Framework Core y SQLite.

## Capas

- `TransportTrack.Domain`: entidades (`Conductor`, `Vehiculo` y `Ruta`).
- `TransportTrack.Application`: contratos de servicios, DTOs, validaciones y lógica de negocio.
- `TransportTrack.Infrastructure`: contexto SQLite, repositorios y excepciones de persistencia.
- `TransportTrack.Api`: controladores HTTP e inyección de dependencias.

## Endpoints

- `api/Conductores`
- `api/Vehiculos`
- `api/Rutas`

Los DTOs de creación y actualización validan campos obligatorios, longitudes, rangos, teléfono y relaciones antes de persistir información.

## Ejecutar

Requiere .NET 8 SDK:

```bash
cp TransportTrack.Api/appsettings.example.json TransportTrack.Api/appsettings.json
dotnet restore TransportTrack.slnx
dotnet run --project TransportTrack.Api
```

La base de datos SQLite se crea automáticamente como `transporttrack.db` al iniciar la API. Los archivos de base de datos y configuración local están excluidos por `.gitignore`.
