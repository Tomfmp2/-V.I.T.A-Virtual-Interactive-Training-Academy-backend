// =============================================================================
// VITA — Modelo de datos (DBML)
// Plataforma de cursos · ASP.NET Core Identity + Entity Framework Core
// =============================================================================
//
// Propósito
//   Diagrama / referencia del esquema lógico. NO es el mecanismo diario para
//   crear o actualizar la base de datos.
//
// Fuente de verdad en runtime
//   Migraciones de Entity Framework Core + Database.Migrate() al iniciar el API.
//
// Base de datos local
//   academia_cursos
//
// Decisión de arquitectura
//   - Identity es la fuente de verdad de usuarios y roles (AspNet*).
//   - No existen tablas manuales "usuarios" ni "roles".
//   - AspNetUsers.Id es string (GUID de Identity). Las FK a usuarios usan el
//     mismo tipo (varchar(450) en este modelo).
//   - Campos de perfil VITA viven como propiedades de ApplicationUser en
//     AspNetUsers.
//   - permisos / rol_permisos son opcionales (permisos granulares). No
//     reemplazan AspNetRoles ni AspNetUserRoles.
//
// Herramienta de visualización sugerida
//   https://dbdiagram.io  (pegar este archivo)
//
// =============================================================================


// -----------------------------------------------------------------------------
// 1. IDENTITY — Usuarios, roles y asignación usuario–rol
//    Generado / gestionado por ASP.NET Core Identity.
//    ApplicationUser extiende IdentityUser con los campos VITA al final.
// -----------------------------------------------------------------------------

Table AspNetUsers {
  Id varchar(450) [pk]

  UserName varchar(256)
  NormalizedUserName varchar(256)

  Email varchar(256)
  NormalizedEmail varchar(256)

  EmailConfirmed boolean
  PasswordHash text

  SecurityStamp text
  ConcurrencyStamp text

  PhoneNumber varchar(50)
  PhoneNumberConfirmed boolean

  TwoFactorEnabled boolean

  LockoutEnd datetime
  LockoutEnabled boolean

  AccessFailedCount int

  // --- Extensión ApplicationUser (dominio VITA) ---
  Nombre varchar(100)
  Apellido varchar(100)
  FotoUrl varchar(255)
  Biografia text
  Activo boolean
  CreatedAt datetime
}


Table AspNetRoles {
  Id varchar(450) [pk]
  Name varchar(256)
  NormalizedName varchar(256)
  ConcurrencyStamp text
}


Table AspNetUserRoles {
  UserId varchar(450) [not null]
  RoleId varchar(450) [not null]

  indexes {
    (UserId, RoleId) [pk]
  }
}


// -----------------------------------------------------------------------------
// 2. PERMISOS VITA (opcionales / granulares)
//    Complementan Identity; no sustituyen AspNetRoles / AspNetUserRoles.
//    Incluir en migración solo si el producto exige permisos más finos que
//    Admin | Instructor | Estudiante.
// -----------------------------------------------------------------------------

Table permisos {
  id_permiso int [pk, increment]
  codigo varchar(80) [not null, unique]
  descripcion varchar(255)
}


Table rol_permisos {
  RoleId varchar(450) [not null]
  id_permiso int [not null]

  indexes {
    (RoleId, id_permiso) [pk]
  }
}


// -----------------------------------------------------------------------------
// 3. CATÁLOGOS (lookups)
//    Tablas de referencia para cursos, lecciones e inscripciones.
// -----------------------------------------------------------------------------

Table categorias {
  id_categoria int [pk, increment]
  nombre varchar(100) [not null, unique]
  slug varchar(120) [not null, unique]
  descripcion varchar(255)
  icono_url varchar(255)
  activo boolean [not null, default: true]
}


Table niveles {
  id_nivel int [pk, increment]
  nombre varchar(50) [not null, unique]

  Note: 'principiante | intermedio | avanzado'
}


Table estados_curso {
  id_estado_curso int [pk, increment]
  nombre varchar(50) [not null, unique]

  Note: 'borrador | publicado'
}


Table tipos_leccion {
  id_tipo_leccion int [pk, increment]
  nombre varchar(50) [not null, unique]

  Note: 'video | texto | recurso'
}


Table estados_inscripcion {
  id_estado_inscripcion int [pk, increment]
  nombre varchar(50) [not null, unique]

  Note: 'activa | cancelada'
}


// -----------------------------------------------------------------------------
// 4. CURSOS
//    id_instructor → AspNetUsers.Id (string / GUID Identity)
// -----------------------------------------------------------------------------

Table cursos {
  id_curso int [pk, increment]

  // FK → AspNetUsers.Id
  id_instructor varchar(450) [not null]

  id_categoria int [not null]
  id_nivel int [not null]
  id_estado_curso int [not null]

  titulo varchar(200) [not null]
  slug varchar(220) [not null, unique]
  descripcion_corta varchar(300)
  descripcion_larga text
  imagen_portada_url varchar(255)
  duracion_estimada_min int

  created_at datetime [not null]
  update_at datetime
}


// -----------------------------------------------------------------------------
// 5. LECCIONES
//    Pertenecen a un curso; orden único por curso.
// -----------------------------------------------------------------------------

Table lecciones {
  id_leccion int [pk, increment]

  id_curso int [not null]
  id_tipo_leccion int [not null]

  titulo varchar(200) [not null]
  resumen text
  contenido text
  recurso_url varchar(255)

  orden int [not null, default: 1]
  duracion_min int
  creado_en datetime [not null]

  indexes {
    (id_curso, orden) [unique]
  }
}


// -----------------------------------------------------------------------------
// 6. INSCRIPCIONES
//    id_estudiante → AspNetUsers.Id (string / GUID Identity)
//    Un estudiante no puede inscribirse dos veces al mismo curso.
// -----------------------------------------------------------------------------

Table inscripciones {
  id_inscripcion int [pk, increment]

  // FK → AspNetUsers.Id
  id_estudiante varchar(450) [not null]

  id_curso int [not null]
  id_estado_inscripcion int [not null]

  fecha_inscripcion datetime [not null]

  indexes {
    (id_estudiante, id_curso) [unique]
  }
}


// -----------------------------------------------------------------------------
// 7. PROGRESO DE LECCIONES
//    id_estudiante → AspNetUsers.Id (string / GUID Identity)
//    Un registro por estudiante y lección.
// -----------------------------------------------------------------------------

Table progreso_lecciones {
  id_progreso int [pk, increment]

  // FK → AspNetUsers.Id
  id_estudiante varchar(450) [not null]

  id_leccion int [not null]

  completada boolean [not null, default: true]
  visto_en datetime [not null]

  indexes {
    (id_estudiante, id_leccion) [unique]
  }
}


// =============================================================================
// RELACIONES
// =============================================================================


// -----------------------------------------------------------------------------
// A. Identity (usuarios ↔ roles) + permisos granulares VITA
// -----------------------------------------------------------------------------

Ref: AspNetUserRoles.UserId > AspNetUsers.Id
Ref: AspNetUserRoles.RoleId > AspNetRoles.Id

Ref: rol_permisos.RoleId > AspNetRoles.Id
Ref: rol_permisos.id_permiso > permisos.id_permiso


// -----------------------------------------------------------------------------
// B. Dominio VITA → Identity (FK string hacia AspNetUsers.Id)
// -----------------------------------------------------------------------------

Ref: cursos.id_instructor > AspNetUsers.Id

Ref: inscripciones.id_estudiante > AspNetUsers.Id

Ref: progreso_lecciones.id_estudiante > AspNetUsers.Id


// -----------------------------------------------------------------------------
// C. Dominio VITA (catálogos, cursos, lecciones, inscripciones, progreso)
// -----------------------------------------------------------------------------

Ref: cursos.id_categoria > categorias.id_categoria
Ref: cursos.id_nivel > niveles.id_nivel
Ref: cursos.id_estado_curso > estados_curso.id_estado_curso

Ref: lecciones.id_curso > cursos.id_curso
Ref: lecciones.id_tipo_leccion > tipos_leccion.id_tipo_leccion

Ref: inscripciones.id_curso > cursos.id_curso
Ref: inscripciones.id_estado_inscripcion > estados_inscripcion.id_estado_inscripcion

Ref: progreso_lecciones.id_leccion > lecciones.id_leccion
