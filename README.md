# VITA — Backend

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=JSON%20web%20tokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

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

CORS hacia el frontend (`http://localhost:5173`) — pendiente


## Configuración local

Para ejecutar el API necesitas configurar localmente:

- La cadena de conexión: `ConnectionStrings:Default`
- La clave JWT: `Jwt:Key`
- Las contraseñas de los usuarios demo, descritas abajo

No subas secretos ni archivos de configuración local al repositorio. Usa User Secrets o variables de entorno.

## Seeds de desarrollo

En entorno `Development`, el API ejecuta las migraciones y luego crea datos de prueba si todavía no existen:

- Roles: `Admin`, `Instructor` y `Estudiante`
- Usuarios demo:
  - `admin@vita.local`
  - `instructor@vita.local`
  - `estudiante@vita.local`
- Catálogos, niveles y categorías de ejemplo

El seed es idempotente: puedes reiniciar el API sin que se dupliquen los registros.

### Configurar contraseñas demo

Desde la carpeta `Vita.Api`, configura las contraseñas con User Secrets:

```bash
dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password-admin>"
dotnet user-secrets set "Seeds:DemoUsers:InstructorPassword" "<password-instructor>"
dotnet user-secrets set "Seeds:DemoUsers:StudentPassword" "<password-student>"
```

También puedes usar variables de entorno:

```powershell
$env:Seeds__DemoUsers__AdminPassword = "<password-admin>"
$env:Seeds__DemoUsers__InstructorPassword = "<password-instructor>"
$env:Seeds__DemoUsers__StudentPassword = "<password-student>"
```

Si falta alguna de estas variables, el API mostrará un error indicando cuál debes configurar.

### Reiniciar los datos locales

Para empezar desde cero, elimina la base de datos local y ejecuta el API nuevamente.

Desde la raíz del repositorio:

```bash
dotnet run --project Vita.Api
```

O, si ya estás dentro de la carpeta `Vita.Api`:

```bash
dotnet run
```

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
