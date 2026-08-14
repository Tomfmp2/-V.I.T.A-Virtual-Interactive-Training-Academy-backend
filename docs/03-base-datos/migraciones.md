# Migraciones EF Core — VITA

> **Basado en rama `develop`** · **Fecha:** 13/08/2026
> Resumen del flujo de migraciones. Documentación extendida: [`Documentacion_Migraciones_EF_VITA.md`](../../Documentacion_Migraciones_EF_VITA.md)

---

## Flujo activo

```
Entidades C# → ApplicationDbContext → Migraciones EF Core → academia_cursos (PostgreSQL)
```

El esquema se gestiona **siempre** con migraciones EF Core. `DB/Script.sql` es solo referencia/diagrama, no se ejecuta manualmente.

---

## Cadena de migraciones activa (`develop`)

| Timestamp | Migración | Contenido |
| --- | --- | --- |
| `20260810155411` | `InitialIdentity` | Tablas de ASP.NET Core Identity: `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, claims, tokens |
| `20260810211253` | `AddDomainEntities` | Tablas de dominio VITA: `categorias`, `niveles`, `estados_curso`, `tipos_leccion`, `estados_inscripcion`, `cursos`, `lecciones`, `inscripciones` |

Archivos en `Vita.Api/Migrations/`:
```
20260810155411_InitialIdentity.cs
20260810211253_AddDomainEntities.cs
ApplicationDbContextModelSnapshot.cs
```

Al arrancar el API, `Database.Migrate()` en `Program.cs` aplica automáticamente cualquier migración pendiente.

---

## Acuerdo del equipo

| Regla | Detalle |
| --- | --- |
| Mecanismo | Migraciones EF Core (`dotnet ef`) — estándar |
| Aplicación | `Database.Migrate()` al iniciar `Vita.Api` (acuerdo dev) |
| Script SQL | Solo referencia / diseño, **no** mecanismo de despliegue |
| Arquitectura | Proyecto único `Vita.Api` — no existe `Vita.Infrastructure` ni `Vita.Domain` |

---

## Comandos de referencia

Ejecutar desde la **raíz del repo** (donde está `Vita.sln`):

```bash
# Listar migraciones y su estado
dotnet ef migrations list --project Vita.Api --startup-project Vita.Api

# Crear una nueva migración
dotnet ef migrations add NombreMigracion --project Vita.Api --startup-project Vita.Api

# Aplicar migraciones pendientes (CLI)
dotnet ef database update --project Vita.Api --startup-project Vita.Api

# Eliminar la última migración (si aún no se aplicó a BD compartidas)
dotnet ef migrations remove --project Vita.Api --startup-project Vita.Api
```

---

## Flujo para agregar una migración

1. Modificar entidad en `Vita.Api/Entities/`.
2. Actualizar `DbSet` o Fluent API en `ApplicationDbContext` si aplica.
3. `dotnet ef migrations add NombreMigracion --project Vita.Api --startup-project Vita.Api`
4. Revisar el método `Up` y `Down` generados.
5. `dotnet run --project Vita.Api` → `Migrate()` aplica la migración al arrancar.
6. Verificar en PostgreSQL:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

7. Commit de los archivos de migración + entidades modificadas.

---

## Convenciones de nombre

Usar PascalCase descriptivo en inglés o español:

```
InitialIdentity
AddDomainEntities
AddCourseProgressTable
AgregarCampoFotoUrl
```

---

## Buenas prácticas

- Versionar `Vita.Api/Migrations/` en el repo.
- Mantener FK a `AspNetUsers.Id` como `string`.
- Revisar migraciones destructivas antes de aplicar en entornos compartidos.
- No ejecutar `DB/Script.sql` como despliegue.
- No alterar tablas a mano en PostgreSQL para cambios de producto.
- No commitear passwords ni connection strings reales.

---

## Ver también

- Documentación extendida: [`Documentacion_Migraciones_EF_VITA.md`](../../Documentacion_Migraciones_EF_VITA.md)
- Modelo de datos: [`modelo-identity.md`](modelo-identity.md)
- Entidades: `Vita.Api/Entities/`
- DbContext: `Vita.Api/Data/ApplicationDbContext.cs`
