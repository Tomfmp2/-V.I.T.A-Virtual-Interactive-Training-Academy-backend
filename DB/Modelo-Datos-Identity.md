# Modelo de datos VITA — Identity + EF Core

Documentación del esquema lógico definido en `Script.sql` (formato **DBML**).

| | |
| --- | --- |
| **Archivo de modelo** | `DB/Script.sql` |
| **Base de datos local** | `academia_cursos` |
| **Stack** | ASP.NET Core Identity · Entity Framework Core · PostgreSQL |
| **Uso del script** | Referencia / diagrama — **no** mecanismo diario de despliegue |

---

## 1. Para qué sirve este script

`Script.sql` describe el **modelo conceptual** de la base (tablas, columnas y relaciones) en sintaxis DBML, pensada para visualizar en herramientas como [dbdiagram.io](https://dbdiagram.io).

No sustituye a las migraciones de EF Core. El equipo acordó:

| Responsable | Acuerdo |
| --- | --- |
| Santiago (L4) | Migración inicial + `Database.Migrate()` al arrancar el API |
| Equipo | No usar este script (ni un `ScriptSQL_VITA.sql`) como forma diaria de montar o actualizar la BD |
| Daniel (L7) | Usar `string` en entidades y FK que apunten a `AspNetUsers.Id` |

**Flujo correcto:** diseñar en este DBML → mapear a entidades C# → generar migración EF → aplicar con `Database.Migrate()`.

---

## 2. Decisión de arquitectura (por qué está así)

### 2.1 Identity es la fuente de verdad

ASP.NET Core Identity gestiona **usuarios** y **roles**. Por eso:

- No hay tabla manual `usuarios` → se usa `AspNetUsers`
- No hay tabla manual `roles` → se usa `AspNetRoles`
- La relación usuario–rol es `AspNetUserRoles`

### 2.2 Identificador de usuario = `string` (GUID)

Identity genera el `Id` como **string** (GUID). Todas las FK del dominio VITA que apuntan a un usuario deben usar el mismo tipo:

| Tabla | Columna FK | Apunta a |
| --- | --- | --- |
| `cursos` | `id_instructor` | `AspNetUsers.Id` |
| `inscripciones` | `id_estudiante` | `AspNetUsers.Id` |
| `progreso_lecciones` | `id_estudiante` | `AspNetUsers.Id` |

En el DBML se modelan como `varchar(450)` (convención típica de Identity en SQL). En C# / EF: tipo `string`.

### 2.3 Perfil VITA dentro de `AspNetUsers`

Los datos de negocio del usuario no van a una tabla paralela. Son propiedades de `ApplicationUser` (clase que hereda de `IdentityUser`) y se persisten en `AspNetUsers`:

| Campo | Uso |
| --- | --- |
| `Nombre` | Nombre del usuario |
| `Apellido` | Apellido |
| `FotoUrl` | Avatar / foto de perfil |
| `Biografia` | Texto libre |
| `Activo` | Soft-flag de habilitación |
| `CreatedAt` | Alta del registro |

El resto de columnas de `AspNetUsers` (`UserName`, `Email`, `PasswordHash`, lockout, etc.) son el esquema estándar de Identity.

### 2.4 Roles vs permisos granulares

| Capa | Tablas | Rol en el producto |
| --- | --- | --- |
| Identity (obligatoria) | `AspNetRoles`, `AspNetUserRoles` | Roles de producto: `Admin`, `Instructor`, `Estudiante` |
| VITA (opcional) | `permisos`, `rol_permisos` | Permisos finos **solo si** hacen falta; **no** reemplazan Identity |

Si en la primera iteración basta con los tres roles, `permisos` / `rol_permisos` pueden aplazarse a una migración posterior sin romper el contrato Auth.

### 2.5 Tablas Identity adicionales

EF Core / Identity también crean (en migraciones) tablas como `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens` y `AspNetRoleClaims`.  
No están en este DBML a propósito: el diagrama se centra en el dominio VITA + el núcleo usuario/rol. La migración de Identity las añadirá automáticamente.

---

## 3. Mapa del esquema

```text
AspNetUsers ──┬── AspNetUserRoles ── AspNetRoles ── rol_permisos ── permisos
              │
              ├── cursos (id_instructor)
              │      ├── categorias / niveles / estados_curso
              │      └── lecciones ── tipos_leccion
              │             └── progreso_lecciones (id_estudiante)
              │
              ├── inscripciones (id_estudiante) ── estados_inscripcion
              │         └── cursos
              │
              └── progreso_lecciones (id_estudiante)
```

### Bloques del archivo

| Sección en `Script.sql` | Contenido |
| --- | --- |
| 1. Identity | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` |
| 2. Permisos VITA | `permisos`, `rol_permisos` |
| 3. Catálogos | `categorias`, `niveles`, `estados_curso`, `tipos_leccion`, `estados_inscripcion` |
| 4–7. Dominio | `cursos`, `lecciones`, `inscripciones`, `progreso_lecciones` |
| Relaciones A–C | Identity, VITA→Identity, VITA interno |

---

## 4. Lógica de negocio reflejada en el modelo

| Regla | Cómo se modela |
| --- | --- |
| Un curso tiene un instructor (usuario Identity) | `cursos.id_instructor` → `AspNetUsers.Id` |
| Un estudiante se inscribe a un curso una sola vez | Unique `(id_estudiante, id_curso)` en `inscripciones` |
| El orden de lecciones es único por curso | Unique `(id_curso, orden)` en `lecciones` |
| Progreso: un registro por estudiante y lección | Unique `(id_estudiante, id_leccion)` en `progreso_lecciones` |
| Estados y tipos controlados | Catálogos (`estados_curso`, `tipos_leccion`, etc.) |

---

## 5. Cómo se conecta con el código ASP.NET

| Concepto en BD | Concepto en backend |
| --- | --- |
| `AspNetUsers` (+ campos VITA) | `ApplicationUser : IdentityUser` |
| `AspNetRoles` | `IdentityRole` (o rol tipado del proyecto) |
| `AspNetUserRoles` | Gestión vía `UserManager` / `RoleManager` |
| Tablas `cursos`, `lecciones`, … | Entidades EF + `DbSet<>` en `DbContext` |
| FK `string` a usuarios | Propiedades `string` en entidades (acuerdo L7) |
| Creación del esquema | Migraciones EF + `Database.Migrate()` (acuerdo L4) |

Autenticación API (JWT Bearer) y roles del contrato (`Admin`, `Instructor`, `Estudiante`) se apoyan en estas tablas Identity; el frontend no habla con la BD directamente.

---

## 6. Notas para el equipo

1. **Editar tablas/columnas en este DBML** solo como diseño; el cambio real va en entidades + migración.
2. **No cambiar** FKs a `AspNetUsers.Id` a `int`: rompería Identity.
3. Visualizar: copiar `Script.sql` en dbdiagram.io.
4. Documentación API relacionada: contrato compartido del líder (`VITA-Contrato-API-Compartido.md` en la carpeta del monorepo / coordinación).

---

*Carpeta `DB/` · Referencia de modelo VITA alineada a ASP.NET Core Identity*
