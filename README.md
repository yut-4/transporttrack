# TransportTrack

CRUD Web API en ASP.NET Core con Controllers, DTOs, Entity Framework Core y SQLite.

## Entidades

- Conductores
- Vehiculos

Cada vehiculo pertenece a un conductor.

## Ejecutar

```bash
cp appsettings.example.json appsettings.json
dotnet restore
dotnet run
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
