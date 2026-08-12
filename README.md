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

Requisitos: SDK .NET 10, PostgreSQL.

**Primera vez en cada PC** (base + secretos + arranque):

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\setup-dev.ps1
```

La primera línea permite ejecutar el script solo en esa ventana de PowerShell; Windows bloquea los `.ps1` por defecto. No cambia la configuración del sistema ni requiere permisos de administrador.

Detalle, ruta manual y solución de problemas: [`docs/EJECUTAR-EL-BACKEND.md`](docs/EJECUTAR-EL-BACKEND.md).

> **Nota:** `DB/Script.sql` es **material de referencia** del modelo de datos. **No** lo ejecutes para montar el entorno: el esquema lo generan las migraciones de EF Core (`db.Database.Migrate()`). Ejecutar el script SQL y las migraciones juntos provoca el conflicto de dueños (`postgres` vs `vita_user`).

Si ya tienes base y User Secrets configurados:

```bash
dotnet restore
dotnet run --project Vita.Api
```

**Obligatorio en cada PC** (Development): las contraseñas de seed van en User Secrets. Si faltan, el API arranca migraciones y luego falla con `Falta la contraseña de seed`. Ver sección [Seeds locales (User Secrets)](#seeds-locales-user-secrets) o la guía única.

Swagger (desarrollo):

```
http://localhost:5044/swagger
```

El puerto puede variar según `Properties/launchSettings.json`.

## Configuración local

Cada máquina que monte el backend debe tener, **fuera del Git**:

| Clave | Uso |
| --- | --- |
| `ConnectionStrings:Default` | PostgreSQL (`academia_cursos`) |
| `Jwt:Key` | Firma del token |
| `Seeds:DemoUsers:AdminPassword` | Password del usuario demo Admin |
| `Seeds:DemoUsers:InstructorPassword` | Password del usuario demo Instructor |
| `Seeds:DemoUsers:StudentPassword` | Password del usuario demo Estudiante |

No subas secretos al repositorio. Usa **User Secrets** (recomendado en Windows) o variables de entorno. Los valores concretos de las passwords demo están en la **tarjeta Trello de Seeds** (solo equipo), no en este README.

---

## Seeds locales (User Secrets)

En `Development`, después de `Database.Migrate()`, el API crea datos de prueba **si aún no existen**:

- Roles Identity: `Admin`, `Instructor`, `Estudiante`
- Un usuario demo por rol (emails públicos; **passwords solo en secretos locales**):
  - `admin@vita.local`
  - `instructor@vita.local`
  - `estudiante@vita.local`
- Catálogos, niveles y categorías de ejemplo

El seed es **idempotente**: reiniciar el API no duplica registros. **No corre en Production.**

Cada PC es independiente: clonar el repo **no** copia User Secrets. Hay que asignarlos en esa máquina.

### Paso a paso (PowerShell)

**1.** Abrir terminal en la raíz del repo (donde está `Vita.sln`).

**2.** Entrar al proyecto API (ahí está el `UserSecretsId`):

```powershell
cd Vita.Api
```

**3.** Asignar las tres contraseñas (reemplaza los placeholders por los valores de la tarjeta Trello):

```powershell
dotnet user-secrets set "Seeds:DemoUsers:AdminPassword" "<password-admin>"
dotnet user-secrets set "Seeds:DemoUsers:InstructorPassword" "<password-instructor>"
dotnet user-secrets set "Seeds:DemoUsers:StudentPassword" "<password-student>"
```

**4.** Comprobar que quedaron guardadas (no muestra el valor, solo las claves):

```powershell
dotnet user-secrets list
```

**5.** Volver a la raíz y arrancar:

```powershell
cd ..
dotnet run --project Vita.Api
```

**6.** Si aparece `Falta la contraseña de seed 'Seeds:DemoUsers:...'`, repetir el paso 3 en `Vita.Api` (misma carpeta del `.csproj`).

### Alternativa: variables de entorno (sesión actual)

Sirven solo mientras esa ventana de PowerShell esté abierta:

```powershell
$env:Seeds__DemoUsers__AdminPassword = "<password-admin>"
$env:Seeds__DemoUsers__InstructorPassword = "<password-instructor>"
$env:Seeds__DemoUsers__StudentPassword = "<password-student>"
dotnet run --project Vita.Api
```

### Reset de datos locales

1. Borrar la base `academia_cursos` (o la que uses en local).
2. Confirmar que los User Secrets siguen configurados (`dotnet user-secrets list` dentro de `Vita.Api`).
3. `dotnet run --project Vita.Api` → Migrate + seed otra vez.

---

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
- Guía seeds por PC: `docs/Configurar-seeds-locales.md`
- Guía única para levantar el backend: `docs/EJECUTAR-EL-BACKEND.md`
