# VITA — Backend

API REST de **V.I.T.A** (Virtual Interactive Training Academy), plataforma de cursos online.

Proyecto 3 · CAMPUSLANDS

## Stack

| Capa | Tecnología |
| --- | --- |
| Runtime | .NET 10 |
| API | ASP.NET Core Web API (C#) |
| Persistencia | Entity Framework Core + Npgsql |
| Base de datos | PostgreSQL |
| Autenticación | ASP.NET Core Identity + JWT |
| Documentación | Swagger (Swashbuckle) |
| Arquitectura | Capas en un solo proyecto: Controllers → Services → Repositories |

## Roles

- `Admin`
- `Instructor`
- `Estudiante`

## Módulos

1. Autenticación
2. Usuarios (Admin)
3. Roles
4. Categorías
5. Cursos
6. Lecciones
7. Inscripciones
8. Reportes

## Prefijo API

```
/api
```

Autenticación en endpoints protegidos:

```
Authorization: Bearer <token>
```

## Estructura

```
Vita.sln
Vita.Api/
├── Controllers/     # Endpoints (presentación)
├── Services/        # Lógica de negocio
├── Repositories/    # Acceso a datos (EF Core)
├── Entities/        # Modelos / entidades
├── Dtos/            # Contratos de entrada/salida
├── Config/          # Identity, JWT, Swagger, DbContext
├── Program.cs
├── appsettings.json
└── Vita.Api.csproj
```

Las carpetas de capas arrancan vacías (con `.gitkeep`) hasta implementar cada módulo.

## Regla de dependencias

```
Controllers → Services → Repositories → DbContext
```

Controllers no acceden directamente a la persistencia.

## Cómo ejecutar

Requisitos: SDK .NET 10, PostgreSQL (cuando se conecte la BD).

```bash
dotnet restore
dotnet run --project Vita.Api
```

Swagger (desarrollo):

```
http://localhost:5044/swagger
```

El puerto puede variar según `Properties/launchSettings.json`.

## Variables / configuración

Pendiente de completar al implementar:

- Connection string de PostgreSQL
- Clave de firma JWT
- Políticas de Identity
- CORS hacia el frontend (`http://localhost:5173`)

No commitear secretos. Usar `appsettings.Development.json` (local) o user-secrets.

## Equipo

| Rol | Responsabilidad |
| --- | --- |
| Líder | Coordinación, PRs, integración |
| Backend 1 | Auth, Identity, JWT, middlewares |
| Backend 2 | Módulos 2–8 |
| Base de datos | Esquema, seeds, alineación con Identity |

## Convenciones

- Conventional Commits: `feat(auth): ...`, `fix(courses): ...`
- Ramas: `feature/<ID>-descripcion`
- Merge a `main` solo por PR
- Errores API: `{ "error": "...", "statusCode": 400 }`

## Documentación de referencia

- Documentación backend (contrato de endpoints)
- Script SQL / modelo ER
- Requerimientos Proyecto 3
