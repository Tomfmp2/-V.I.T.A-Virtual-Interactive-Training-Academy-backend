# Modelo de datos — Identity + dominio VITA

> **Basado en rama `develop`** · **Fecha:** 13/08/2026  
> Resumen del esquema lógico de la base de datos `academia_cursos`.  
> Referencia completa: [`DB/Modelo-Datos-Identity.md`](../../DB/Modelo-Datos-Identity.md)

---

## Stack

| Capa | Tecnología |
| --- | --- |
| ORM | Entity Framework Core |
| BD | PostgreSQL (`academia_cursos`) |
| Gestión de usuarios | ASP.NET Core Identity |

---

## Decisiones de arquitectura clave

### Identity es la fuente de verdad de usuarios y roles

No hay tabla manual `usuarios` ni `roles`:

| Concepto | Tabla de Identity |
| --- | --- |
| Usuarios | `AspNetUsers` |
| Roles | `AspNetRoles` |
| Relación usuario–rol | `AspNetUserRoles` |

### Perfil VITA extendido en `AspNetUsers`

La entidad `Usuario` hereda de `IdentityUser` y agrega campos propios:

| Campo | Uso |
| --- | --- |
| `Nombre` | Nombre del usuario |
| `Apellido` | Apellido |
| `FotoUrl` | Avatar / foto de perfil |
| `Activo` | Borrado lógico (`false` = inactivo) |
| `CreatedAt` | Fecha de alta |

### FK hacia usuarios = `string` (GUID)

Identity genera el `Id` como `string` (GUID). Todas las FK de dominio que apuntan a un usuario usan el mismo tipo:

| Tabla | FK | Apunta a |
| --- | --- | --- |
| `cursos` | `id_instructor` | `AspNetUsers.Id` |
| `inscripciones` | `id_estudiante` | `AspNetUsers.Id` |

---

## Mapa del esquema

```
AspNetUsers ──┬── AspNetUserRoles ── AspNetRoles
              │
              ├── cursos (id_instructor)
              │      ├── categorias
              │      ├── niveles
              │      ├── estados_curso
              │      └── lecciones ── tipos_leccion
              │
              └── inscripciones (id_estudiante)
                     ├── cursos
                     └── estados_inscripcion
```

---

## Tablas de dominio VITA

| Tabla | Descripción |
| --- | --- |
| `cursos` | Cursos con FK a instructor, categoría, nivel y estado |
| `lecciones` | Lecciones con FK a curso y tipo de lección |
| `inscripciones` | Inscripción estudiante–curso con estado |
| `categorias` | Catálogo de categorías (con slug y flag activo) |
| `niveles` | Catálogo: Principiante, Intermedio, Avanzado |
| `estados_curso` | Catálogo: Borrador, Publicado |
| `tipos_leccion` | Catálogo: Video, Texto, Recurso |
| `estados_inscripcion` | Catálogo: Activa, Cancelada |

---

## Restricciones únicas relevantes

| Tabla | Columnas únicas | Regla |
| --- | --- | --- |
| `inscripciones` | `(id_estudiante, id_curso)` | Un estudiante no puede inscribirse dos veces al mismo curso |
| `lecciones` | `(id_curso, orden)` | El orden de lecciones es único por curso |

---

## Cómo se conecta con el backend

| BD | Código |
| --- | --- |
| `AspNetUsers` | `Vita.Api/Entities/Usuario.cs` (hereda `IdentityUser`) |
| `AspNetRoles` | `IdentityRole` (gestionado por `RoleManager`) |
| Tablas de dominio | `DbSet<T>` en `Vita.Api/Data/ApplicationDbContext.cs` |
| Creación del esquema | Migraciones EF Core + `Database.Migrate()` en `Program.cs` |

---

## Ver también

- Modelo completo: [`DB/Modelo-Datos-Identity.md`](../../DB/Modelo-Datos-Identity.md)
- Diagrama DBML: [`DB/Script.sql`](../../DB/Script.sql) (visualizar en [dbdiagram.io](https://dbdiagram.io))
- Flujo de migraciones: [`migraciones.md`](migraciones.md)
