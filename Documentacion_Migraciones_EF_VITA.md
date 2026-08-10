# Documentaci├│n ÔÇö Creaci├│n y aplicaci├│n de migraciones en VITA

| | |
| --- | --- |
| **Proyecto** | V.I.T.A ÔÇö Virtual Interactive Training Academy |
| **├ümbito** | Backend ┬À Entity Framework Core ┬À PostgreSQL |
| **Arquitectura** | Proyecto ├║nico `Vita.Api` (capas por carpetas) |
| **Base de datos** | `academia_cursos` |
| **Audiencia** | Backend 1 (Identity/Auth) ┬À Backend 2 ┬À Base de datos ┬À L├¡der |
| **Relacionado** | `Documentacion_Backend_VITA.md` ┬À `DB/Script.sql` ┬À `DB/Modelo-Datos-Identity.md` |

---

## 1. Objetivo

Definir el procedimiento est├índar para **crear, revisar, aplicar y versionar** migraciones de Entity Framework Core en el backend VITA.

Las migraciones mantienen sincronizados:

```text
Entidades C#  ÔåÆ  VitaDbContext  ÔåÆ  Migraciones EF Core  ÔåÆ  PostgreSQL (academia_cursos)
```

**Decisi├│n del equipo:** el esquema de PostgreSQL se gestiona con migraciones EF Core (y `Database.Migrate()` al iniciar el API).  
`DB/Script.sql` (DBML) es **modelo de referencia / diagrama**, no el mecanismo diario para montar o actualizar la base.

---

## 2. ┬┐Esta forma es la correcta?

**S├¡.** Para ASP.NET Core + EF Core + PostgreSQL, las migraciones code-first son la forma recomendada y la que VITA debe usar.

| Opci├│n | ┬┐Usar en VITA? | Motivo |
| --- | --- | --- |
| Migraciones EF Core (`dotnet ef`) | **S├¡ ÔÇö est├índar** | Versionadas en Git, repetibles, alineadas a entidades |
| `Database.Migrate()` al arrancar | **S├¡ ÔÇö desarrollo / local** | Acuerdo L4: migraci├│n pendiente se aplica al iniciar `Vita.Api` |
| Ejecutar `Script.sql` / SQL manual a diario | **No** | Rompe el historial y desalinea al equipo |
| Multi-proyecto (`Infrastructure` / `Domain`) | **No (ahora)** | El backend es un **solo** proyecto `Vita.Api` |

No hace falta otra ÔÇ£manera mejorÔÇØ mientras no se cambie la arquitectura documentada. Lo incorrecto del borrador original era asumir `Vita.Infrastructure` / `Vita.Domain`; aqu├¡ se corrige a la estructura real.

---

## 3. Requisitos

Antes de crear migraciones, verificar:

| Herramienta | Comando |
| --- | --- |
| .NET SDK (proyecto en `net10.0`) | `dotnet --version` |
| PostgreSQL | `psql --version` |
| CLI de EF Core | `dotnet ef --version` |
| Git | `git --version` |

Si falta `dotnet ef`:

```bash
dotnet tool install --global dotnet-ef
```

Actualizar:

```bash
dotnet tool update --global dotnet-ef
```

En `Vita.Api.csproj` ya est├ín (o deben estar) los paquetes:

- `Microsoft.EntityFrameworkCore`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`

---

## 4. Estructura real del backend

Seg├║n `Documentacion_Backend_VITA.md` y el repositorio:

```text
Vita.sln
Vita.Api/
Ôö£ÔöÇÔöÇ Controllers/
Ôö£ÔöÇÔöÇ Services/
Ôö£ÔöÇÔöÇ Repositories/
Ôö£ÔöÇÔöÇ Entities/              ÔåÉ entidades de dominio / EF (incluye ApplicationUser)
Ôö£ÔöÇÔöÇ Dtos/
Ôö£ÔöÇÔöÇ Config/                ÔåÉ Identity, JWT, Swagger, registro de DbContext
Ôö£ÔöÇÔöÇ Migrations/            ÔåÉ generada por `dotnet ef migrations add`
Ôö£ÔöÇÔöÇ Program.cs
Ôö£ÔöÇÔöÇ appsettings.json
Ôö£ÔöÇÔöÇ appsettings.Development.json
ÔööÔöÇÔöÇ Vita.Api.csproj
DB/
Ôö£ÔöÇÔöÇ Script.sql             ÔåÉ DBML de referencia (no aplicar a mano)
ÔööÔöÇÔöÇ Modelo-Datos-Identity.md
```

| Qu├® | D├│nde en VITA |
| --- | --- |
| Proyecto con DbContext | `Vita.Api` |
| Proyecto de inicio (startup) | `Vita.Api` |
| Carpeta de migraciones | `Vita.Api/Migrations/` |
| Connection string / DI | `Program.cs` + `appsettings*.json` / User Secrets |

**No existen** en este repo: `Vita.Application`, `Vita.Domain`, `Vita.Infrastructure`.

---

## 5. Qu├® es una migraci├│n

Una migraci├│n es el conjunto de instrucciones que EF Core genera a partir de diferencias entre el modelo C# actual y el *snapshot* anterior. Describe c├│mo alterar PostgreSQL (crear tablas, columnas, ├¡ndices, FKs, etc.).

Ejemplo conceptual: si a una entidad se le agrega una propiedad, la migraci├│n puede incluir un `AddColumn`. As├¡ **no** se altera la tabla a mano en `psql`.

---

## 6. DbContext en VITA (Identity)

VITA usa **ASP.NET Core Identity**. El contexto no debe ser un `DbContext` vac├¡o gen├®rico: debe heredar de Identity e incluir el usuario de aplicaci├│n.

Ejemplo de forma esperada:

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class VitaDbContext : IdentityDbContext<ApplicationUser>
{
    public VitaDbContext(DbContextOptions<VitaDbContext> options)
        : base(options)
    {
    }

    // DbSet del dominio VITA (ejemplos)
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Leccion> Lecciones => Set<Leccion>();
    // ...
}
```

`ApplicationUser` extiende `IdentityUser` con los campos VITA (`Nombre`, `Apellido`, `FotoUrl`, `Biografia`, `Activo`, `CreatedAt`, etc.), persistidos en `AspNetUsers`.

**Regla L7:** las FK hacia usuario (`id_instructor`, `id_estudiante`, ÔÇª) son `string`, mismo tipo que `AspNetUsers.Id`.

La primera migraci├│n suele crear tablas Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, claims, tokens, ÔÇª) m├ís las tablas de dominio alineadas al modelo de `DB/Script.sql`.

---

## 7. Conexi├│n a PostgreSQL

Base de datos local acordada:

```text
Database=academia_cursos
```

Ejemplo de connection string (valores locales; **no subir contrase├▒as al repo**):

```text
Host=localhost;Port=5432;Database=academia_cursos;Username=vita_user;Password=***
```

Recomendado en desarrollo:

- User Secrets (`dotnet user-secrets`)
- Variables de entorno
- `appsettings.Development.json` (si el equipo acepta secretos solo locales y el archivo est├í ignorado o sin password real)

---

## 8. Procedimiento paso a paso

Trabajar desde la **ra├¡z de la soluci├│n** (donde est├í `Vita.sln`):

```bash
cd C:\Users\ESSA8\Documents\Proyecto_VITA\-V.I.T.A-Virtual-Interactive-Training-Academy-backend
```

### Paso 1 ÔÇö Cambiar el modelo

Modificar entidades en `Entities/`, Fluent API / `OnModelCreating` y `DbSet` en `VitaDbContext` si aplica.

### Paso 2 ÔÇö Crear la migraci├│n

```bash
dotnet ef migrations add NombreMigracion --project Vita.Api --startup-project Vita.Api
```

Ejemplos de nombre:

- `InitialCreate`
- `AgregarDescripcionCurso`
- `AddPermisosGranulares`

Par├ímetros en VITA:

| Par├ímetro | Valor | Significado |
| --- | --- | --- |
| `--project` | `Vita.Api` | Proyecto del DbContext y de la carpeta `Migrations/` |
| `--startup-project` | `Vita.Api` | Proyecto que arranca y provee configuraci├│n / DI |

### Paso 3 ÔÇö Revisar archivos generados

```text
Vita.Api/Migrations/
Ôö£ÔöÇÔöÇ YYYYMMDDHHMMSS_NombreMigracion.cs
Ôö£ÔöÇÔöÇ YYYYMMDDHHMMSS_NombreMigracion.Designer.cs
ÔööÔöÇÔöÇ VitaDbContextModelSnapshot.cs
```

Revisar sobre todo `Up` y `Down`. Prestar atenci├│n a `DropTable`, `DropColumn`, `AlterColumn`.

### Paso 4 ÔÇö Listar migraciones

```bash
dotnet ef migrations list --project Vita.Api --startup-project Vita.Api
```

### Paso 5 ÔÇö Aplicar a PostgreSQL

**Opci├│n A ÔÇö CLI (expl├¡cita):**

```bash
dotnet ef database update --project Vita.Api --startup-project Vita.Api
```

**Opci├│n B ÔÇö al iniciar el API (acuerdo del equipo):**

En `Program.cs`, tras construir la app (solo con cuidado en producci├│n):

```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<VitaDbContext>();
db.Database.Migrate();
```

Luego:

```bash
dotnet run --project Vita.Api
```

Flujo con `Migrate()`:

```text
dotnet run ÔåÆ Vita.Api ÔåÆ VitaDbContext ÔåÆ Database.Migrate() ÔåÆ migraciones pendientes ÔåÆ academia_cursos
```

En local pueden usarse A y/o B; lo importante es que **nadie** dependa de ejecutar SQL manual del DBML.

### Paso 6 ÔÇö Verificar en PostgreSQL

```bash
psql -U vita_user -d academia_cursos
```

```sql
\dt
\d cursos
SELECT * FROM "__EFMigrationsHistory";
```

> Si las tablas se mapearon con `ToTable("cursos")` (snake_case del modelo VITA), usar esos nombres. Identity suele crear `AspNetUsers`, etc., con la convenci├│n de Identity.

### Paso 7 ÔÇö Commit y push

Incluir siempre en Git:

- Archivos nuevos/modificados bajo `Vita.Api/Migrations/`
- Cambios en entidades / `VitaDbContext`

```bash
git status
git add Vita.Api/Migrations Vita.Api/Entities Vita.Api/Config
# commit seg├║n Conventional Commits del equipo, p. ej. feat(db): add initial migration
```

---

## 9. Crear vs aplicar

| Acci├│n | Comando | Efecto |
| --- | --- | --- |
| **Crear** | `dotnet ef migrations add ...` | Genera archivos C#; **no** cambia PostgreSQL por s├¡ solo |
| **Aplicar (CLI)** | `dotnet ef database update ...` | Ejecuta migraciones pendientes en la BD |
| **Aplicar (arranque)** | `Database.Migrate()` | Igual, al iniciar `Vita.Api` |

---

## 10. Eliminar una migraci├│n

Solo si **a├║n no** se aplic├│ a BD compartidas y **no** se subi├│ (o el equipo est├í de acuerdo):

```bash
dotnet ef migrations remove --project Vita.Api --startup-project Vita.Api
```

**No** borrar ni reescribir migraciones ya usadas por otros integrantes.

---

## 11. Flujo recomendado del equipo

```text
1. Alinear cambio con DB/Script.sql (si afecta el modelo)
2. Modificar entidad / DbContext / Fluent API
3. dotnet ef migrations add ...
4. Revisar Up/Down
5. Aplicar (database update y/o run con Migrate)
6. Verificar en PostgreSQL + __EFMigrationsHistory
7. Commit de migraciones + c├│digo
8. Push / PR (p. ej. hacia develop)
```

Responsable habitual de la **migraci├│n inicial + `Database.Migrate()`**: Backend 1 / tarea L4 (Santiago), coordinado con el modelo Identity de BD.

---

## 12. Buenas pr├ícticas VITA

1. **No** alterar tablas a mano en PostgreSQL para cambios de producto.  
2. **No** usar `DB/Script.sql` como script de despliegue diario.  
3. **S├¡** versionar `Migrations/` en el repositorio.  
4. Revisar migraciones destructivas antes de aplicar.  
5. Mantener FK a usuarios como `string`.  
6. Roles de producto v├¡a Identity (`Admin`, `Instructor`, `Estudiante`); `permisos` / `rol_permisos` solo si se acuerda granularidad extra.  
7. No commitear contrase├▒as ni connection strings con secretos reales.

---

## 13. Resumen de comandos (VITA ÔÇö proyecto ├║nico)

```bash
# Herramientas
dotnet --version
psql --version
dotnet ef --version

# Desde la ra├¡z de la soluci├│n (Vita.sln)
dotnet ef migrations list   --project Vita.Api --startup-project Vita.Api
dotnet ef migrations add    NombreMigracion --project Vita.Api --startup-project Vita.Api
dotnet ef database update   --project Vita.Api --startup-project Vita.Api
dotnet ef migrations remove --project Vita.Api --startup-project Vita.Api

dotnet run --project Vita.Api
```

PostgreSQL:

```text
\dt
\d nombre_tabla
SELECT * FROM "__EFMigrationsHistory";
```

---

## 14. Resultado esperado

```text
Vita.Api/
Ôö£ÔöÇÔöÇ Entities/          (modelo actualizado + ApplicationUser)
Ôö£ÔöÇÔöÇ Config/            (registro Identity + DbContext)
Ôö£ÔöÇÔöÇ Migrations/        (historial versionado)
ÔööÔöÇÔöÇ Program.cs         (opcional: Database.Migrate())

PostgreSQL ÔåÆ academia_cursos
Ôö£ÔöÇÔöÇ AspNet*            (Identity)
Ôö£ÔöÇÔöÇ tablas dominio VITA
ÔööÔöÇÔöÇ __EFMigrationsHistory
```

Con esto, el equipo mantiene el esquema de forma controlada, alineada a la arquitectura de **un solo proyecto** documentada en el backend, sin depender de SQL manual ni de una estructura multi-proyecto que VITA no usa.
