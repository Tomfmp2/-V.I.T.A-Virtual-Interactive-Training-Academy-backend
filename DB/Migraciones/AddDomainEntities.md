# Migración AddDomainEntities

| | |
| --- | --- |
| **Migración** | `AddDomainEntities` |
| **Archivo** | `Vita.Api/Migrations/20260810211253_AddDomainEntities.cs` |
| **Rama** | `feature/add-domain-entities` |
| **Base** | `origin/develop` |
| **DbContext** | `ApplicationDbContext` (`IdentityDbContext<Usuario>`) |
| **Fecha** | 2026-08-10 |

---

## Objetivo

Agregar el modelo de dominio VITA (catálogos + cursos + lecciones + inscripciones) sobre el trabajo de Identity ya presente en `develop` (`Usuario`, `ApplicationDbContext`, `InitialIdentity`, JWT, seed de roles, `Database.Migrate()`).

---

## Entidades incluidas

### Dominio (alcance mínimo)

- `Categoria`
- `Curso`
- `Leccion`
- `Inscripcion`

### Catálogos (definidos en `DB/Script.sql`)

- `Nivel`
- `EstadoCurso`
- `TipoLeccion`
- `EstadoInscripcion`

### Fuera de alcance (esta migración)

- `progreso_lecciones`
- `permisos` / `rol_permisos`
- Controllers, servicios, repositorios, DTOs

---

## Tablas creadas

| Tabla | Origen |
| --- | --- |
| `categorias` | Catálogo |
| `niveles` | Catálogo |
| `estados_curso` | Catálogo |
| `tipos_leccion` | Catálogo |
| `estados_inscripcion` | Catálogo |
| `cursos` | Dominio |
| `lecciones` | Dominio |
| `inscripciones` | Dominio |

No se recrean ni modifican tablas `AspNet*`.

---

## Relaciones

| Relación | FK | DeleteBehavior |
| --- | --- | --- |
| `Curso` → `Usuario` (instructor) | `id_instructor` → `AspNetUsers.Id` | `Restrict` |
| `Inscripcion` → `Usuario` (estudiante) | `id_estudiante` → `AspNetUsers.Id` | `Restrict` |
| `Curso` → `Categoria` | `id_categoria` | `Restrict` |
| `Curso` → `Nivel` | `id_nivel` | `Restrict` |
| `Curso` → `EstadoCurso` | `id_estado_curso` | `Restrict` |
| `Leccion` → `Curso` | `id_curso` | `Restrict` |
| `Leccion` → `TipoLeccion` | `id_tipo_leccion` | `Restrict` |
| `Inscripcion` → `Curso` | `id_curso` | `Restrict` |
| `Inscripcion` → `EstadoInscripcion` | `id_estado_inscripcion` | `Restrict` |

Claves de usuario: `string` (acuerdo L7).

---

## Índices únicos

| Tabla | Índice | Columnas |
| --- | --- | --- |
| `inscripciones` | `IX_inscripciones_id_estudiante_id_curso` | `(id_estudiante, id_curso)` |
| `lecciones` | `IX_lecciones_id_curso_orden` | `(id_curso, orden)` |
| `categorias` | `IX_categorias_nombre` / `IX_categorias_slug` | `nombre` / `slug` |
| `cursos` | `IX_cursos_slug` | `slug` |
| Catálogos | `IX_*_nombre` | `nombre` |

---

## Comando utilizado

```bash
dotnet ef migrations add AddDomainEntities --project Vita.Api --startup-project Vita.Api
```

---

## Resultado de `dotnet build`

```text
Compilación correcta.
0 Errores
```

(Advertencia preexistente NU1903 sobre `Microsoft.OpenApi` 2.3.0; no introducida por esta migración.)

---

## Resultado de `dotnet ef migrations list`

```text
20260810155411_InitialIdentity (Pending)
20260810211253_AddDomainEntities (Pending)
```

Ambas aparecen como `Pending` en la BD local actual porque el historial local aún registra la migración antigua `CrearModeloInicialVita` (rama `feature/migrations`), no la cadena `InitialIdentity` → `AddDomainEntities` de `develop`.

---

## Resultado de `dotnet ef database update`

**No aplicado** sobre `academia_cursos` por conflicto de historial local:

```text
Applying migration '20260810155411_InitialIdentity'
42P07: la relación «AspNetRoles» ya existe
```

Causa: la BD local ya tiene Identity + dominio creados por `20260810163913_CrearModeloInicialVita`, mientras el código de esta rama espera `InitialIdentity` + `AddDomainEntities`.

Intentos de verificación alternativa:

- Crear BD `academia_cursos_domain_verify`: denegado (`42501` sin permiso CREATE DATABASE).
- No se reescribió `__EFMigrationsHistory` (evitar mutación destructiva del historial sin aprobación explícita).
- No se ejecutó `database drop` ni `DROP TABLE`.

### Verificación de esquema (solo lectura)

Las tablas de dominio e Identity **sí existen** en `academia_cursos` y el esquema de dominio coincide con `AddDomainEntities` (columnas snake_case, FKs a usuarios, índices únicos requeridos).

---

## Verificación de `__EFMigrationsHistory` (estado local actual)

```text
20260810163913_CrearModeloInicialVita | 10.0.0
```

Estado esperado tras alinear historial o recrear BD limpia:

```text
20260810155411_InitialIdentity
20260810211253_AddDomainEntities
```

---

## Resultado de `dotnet run`

Falla al arrancar por el mismo conflicto: `Database.Migrate()` intenta aplicar `InitialIdentity` y choca con tablas `AspNet*` existentes.

Identity, JWT, seed de roles y `Database.Migrate()` **permanecen en el código** sin cambios.

---

## Revisión de `Up()` / `Down()`

| Revisión | Resultado |
| --- | --- |
| `Up()` crea solo tablas de dominio/catálogos | OK |
| `Up()` no crea `AspNetUsers` / `AspNetRoles` | OK |
| `Up()` no altera Identity | OK |
| FKs a `AspNetUsers` con `Restrict` | OK |
| Índices únicos de lecciones e inscripciones | OK |
| `Down()` solo elimina tablas de dominio/catálogos | OK |
| `Down()` no elimina tablas Identity | OK |

---

## Estado final

| Ítem | Estado |
| --- | --- |
| Rama `feature/add-domain-entities` desde `origin/develop` | Listo |
| Entidades + Fluent API | Listo |
| Migración `AddDomainEntities` generada | Listo |
| `InitialIdentity` sin modificar | Confirmado |
| Compilación | OK |
| Aplicación local de migraciones en `academia_cursos` | Bloqueada por historial divergente |
| Commit / push / PR | Pendiente del desarrollador |

---

## Riesgos o decisiones pendientes

1. **Historial local divergente:** `academia_cursos` tiene `CrearModeloInicialVita`; esta rama usa `InitialIdentity` + `AddDomainEntities`. Hay que alinear (recrear BD limpia o baselinear historial con aprobación del equipo) antes de que `Database.Migrate()` arranque en local.
2. **`progreso_lecciones`:** existe en la BD local antigua; no forma parte de esta migración (queda para una migración posterior).
3. **Tipos Identity en snapshot:** `InitialIdentity` de `develop` modela `AspNetUsers.Id` como `text`; el esquema local antiguo usa `varchar(450)`. No se tocó `InitialIdentity`.
4. No se incluye seed de catálogos (niveles, estados, tipos); solo el esquema.
