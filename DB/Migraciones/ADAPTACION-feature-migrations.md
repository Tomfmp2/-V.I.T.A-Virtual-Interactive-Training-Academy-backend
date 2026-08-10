# Adaptación — `feature/migrations` → `develop` (sin borrar lo funcional)

## Decisión

**No mergear** `feature/migrations` sobre `develop`.

Motivo: esa rama es un camino paralelo (otro DbContext / otra migración inicial) incompatible con Identity/JWT/register ya en `develop`.

La forma correcta **ya está en `develop`**:

```text
InitialIdentity  →  AspNet* + Usuario
AddDomainEntities → categorias, cursos, lecciones, inscripciones, catálogos
```

Eso conserva auth de Daniel y el dominio versionado encima, sin reescribir historial.

## Qué se preserva de `feature/migrations` (sin borrar)

Todo lo funcional de Santiago queda **archivado como referencia**, no como migración activa de EF:

| Origen en la feature | Destino en esta adaptación |
| --- | --- |
| `CrearModeloInicialVita` (.cs + Designer + Snapshot) | `DB/Migraciones/archivo-feature-migrations/` |
| `CrearModeloInicialVita.md` | `DB/Migraciones/CrearModeloInicialVita.md` |
| `VitaDbContext.cs` | `DB/Migraciones/archivo-feature-migrations/VitaDbContext.referencia.cs` |
| `ProgresoLeccion.cs` | `DB/Migraciones/archivo-feature-migrations/ProgresoLeccion.referencia.cs` |
| `DB/Script.sql` | `DB/Script.sql` |
| `Documentacion_Migraciones_EF_VITA.md` | raíz del repo |

> **Importante:** los `.cs` del archivo **no** van en `Vita.Api/Migrations/`. No deben ejecutarse con `dotnet ef` en `develop`. Son evidencia/historial del trabajo que sí funcionó en la feature.

## Cadena oficial de migraciones (activa)

Solo estas viven en `Vita.Api/Migrations/`:

1. `20260810155411_InitialIdentity`
2. `20260810211253_AddDomainEntities`

Al hacer `dotnet run`, `Database.Migrate()` aplica esa cadena sobre `ApplicationDbContext`.

## Qué NO se trae a runtime

- `VitaDbContext` / `ApplicationUser` como contexto activo
- `CrearModeloInicialVita` como migración EF en el proyecto
- Sustitución de `Usuario`, JWT, register o seed de roles

## Si más adelante hace falta `progreso_lecciones`

Usar la referencia archivada y generar una migración **nueva** sobre `develop`:

```bash
# 1) Portar entidad a Vita.Api/Entities/ProgresoLeccion.cs (FK string → Usuario)
# 2) DbSet + Fluent API en ApplicationDbContext
# 3) dotnet ef migrations add AddProgresoLecciones --project Vita.Api --startup-project Vita.Api
```

No reactivar `CrearModeloInicialVita`.

## Veredicto de merge

| Acción | ¿Sí/No? |
| --- | --- |
| Merge directo `feature/migrations` → `develop` | **No** |
| Usar `develop` (InitialIdentity + AddDomainEntities) como base oficial | **Sí** |
| Conservar artefactos de la feature en `DB/Migraciones/archivo-...` | **Sí** |
| Cerrar/archivar `feature/migrations` tras merge de esta PR | Recomendado |

## Responsables

- **Santiago:** la feature original queda preservada aquí; no hace falta re-mergearla.
- **Tomas:** revisar esta PR de adaptación y mergear a `develop` si está de acuerdo.
- **Daniel:** sin cambios de Identity requeridos por este archivo.
