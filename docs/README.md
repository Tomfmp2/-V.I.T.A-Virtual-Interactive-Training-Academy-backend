# Documentación — VITA Backend

> **Rama de referencia:** `develop` · **Fecha:** 13/08/2026  
> Esta carpeta contiene la documentación operativa del backend. Si clonaste el repo y necesitas arrancar o consumir la API, empieza aquí.

---

## Empieza aquí según tu rol

### 🆕 Nuevo en el equipo
1. [`01-setup-local/configurar-env.md`](01-setup-local/configurar-env.md) — configurar `.env` local
2. [`01-setup-local/configurar-seeds.md`](01-setup-local/configurar-seeds.md) — passwords de los usuarios demo
3. [`01-setup-local/probar-swagger.md`](01-setup-local/probar-swagger.md) — validar que el API funciona

### 🖥️ Frontend
1. [`00-convenciones/contrato-api.md`](00-convenciones/contrato-api.md) — resumen ejecutivo de todos los endpoints
2. [`00-convenciones/patron-errores.md`](00-convenciones/patron-errores.md) — formato de errores
3. [`02-modulos-api/`](02-modulos-api/) — detalle de cada módulo

### 🔧 Backend
1. [`00-convenciones/patron-errores.md`](00-convenciones/patron-errores.md) — cómo retornar errores
2. [`00-convenciones/contrato-api.md`](00-convenciones/contrato-api.md) — endpoints y roles
3. [`03-base-datos/migraciones.md`](03-base-datos/migraciones.md) — flujo de migraciones EF Core

---

## Tabla de contenidos

### 00 — Convenciones

| Archivo | Descripción |
| --- | --- |
| [`contrato-api.md`](00-convenciones/contrato-api.md) | Todos los endpoints implementados, métodos, rutas, roles y estado |
| [`patron-errores.md`](00-convenciones/patron-errores.md) | Formato único de errores `{ "error": "...", "statusCode": N }` |

### 01 — Setup local

| Archivo | Descripción |
| --- | --- |
| [`configurar-env.md`](01-setup-local/configurar-env.md) | Copiar `.env.example` → `.env` y completar valores |
| [`configurar-seeds.md`](01-setup-local/configurar-seeds.md) | Passwords de cuentas demo (vía `.env` o User Secrets) |
| [`probar-swagger.md`](01-setup-local/probar-swagger.md) | Flujo register → login → Authorize → probar endpoints |

### 02 — Módulos API

| Archivo | Módulo | Estado |
| --- | --- | --- |
| [`01-auth.md`](02-modulos-api/01-auth.md) | Autenticación | ✅ Implementado |
| [`02-users.md`](02-modulos-api/02-users.md) | Usuarios (Admin) | ✅ Implementado |
| [`03-roles.md`](02-modulos-api/03-roles.md) | Roles | ✅ Implementado |
| [`04-categories.md`](02-modulos-api/04-categories.md) | Categorías | ✅ Implementado |
| [`05-courses.md`](02-modulos-api/05-courses.md) | Cursos | ✅ Implementado |
| [`06-lessons.md`](02-modulos-api/06-lessons.md) | Lecciones | ✅ Implementado |
| [`07-enrollments.md`](02-modulos-api/07-enrollments.md) | Inscripciones | ✅ Implementado |
| [`08-reports.md`](02-modulos-api/08-reports.md) | Reportes | ✅ Implementado |
| [`09-levels.md`](02-modulos-api/09-levels.md) | Niveles | ✅ Implementado |

### 03 — Base de datos

| Archivo | Descripción |
| --- | --- |
| [`modelo-identity.md`](03-base-datos/modelo-identity.md) | Esquema Identity + tablas de dominio VITA |
| [`migraciones.md`](03-base-datos/migraciones.md) | Flujo de migraciones EF Core, comandos y convenciones |
| [`script-sql.md`](03-base-datos/script-sql.md) | Qué es `DB/Script.sql` y para qué sirve (referencia, no despliegue) |

---

## Estado del API

| | |
| --- | --- |
| **Rama** | `feature/reports-api` (merge pendiente a `develop`) |
| **Fecha doc** | 13/08/2026 |
| **URL local** | `http://localhost:5044` |
| **Swagger** | `http://localhost:5044/swagger` |
| **Auth** | JWT Bearer |

## No implementado (fuera de alcance actual)

- **Progreso de lecciones** / completar lección (opcional en el PDF)
- `DELETE /api/enrollments/{id}` — cancelar inscripción
---

## Para contribuidores

> Guía para crear un módulo CRUD nuevo: `Documentaciones VITA → Guía para crear un módulo (CRUD) en Vita.Api.md` (Obsidian del equipo).
