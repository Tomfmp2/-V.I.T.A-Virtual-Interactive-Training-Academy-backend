# VITA — Backend

API REST de **V.I.T.A** (Virtual Interactive Training Academy), plataforma de cursos online.

Proyecto 3 · CAMPUSLANDS

## Stack

| Capa | Tecnología |
| --- | --- |
| API | ASP.NET Core Web API (C#) |
| Persistencia | Entity Framework Core + Npgsql |
| Base de datos | PostgreSQL |
| Autenticación | ASP.NET Core Identity + JWT |
| Documentación | Swagger (Swashbuckle) |
| Arquitectura | Capas: Controllers → Services → Repositories |

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

## Estructura prevista

```
Vita.Api/
├── Controllers/
├── Services/
├── Repositories/
├── Entities/
├── Dtos/
├── Config/
└── Program.cs
```

## Cómo ejecutar

Pendiente de scaffold del proyecto.

```bash
dotnet restore
dotnet run --project Vita.Api
```

Swagger (desarrollo):

```
https://localhost:<puerto>/swagger
```

## Variables / configuración

- Connection string de PostgreSQL
- Clave de firma JWT
- Políticas de Identity (password, email único)

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
