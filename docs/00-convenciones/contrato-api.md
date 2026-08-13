# Contrato API — VITA Backend

> **Basado en rama `develop`** · **Fecha:** 13/08/2026  
> Este documento es la vista ejecutiva de todos los endpoints implementados.  
> Para detalle de request/response y ejemplos JSON, ver cada módulo en [`02-modulos-api/`](../02-modulos-api/).  
> Swagger (`http://localhost:5044/swagger`) muestra el contrato interactivo completo.

---

## Convenciones globales

| Ítem | Valor |
| --- | --- |
| **Prefijo** | `/api` |
| **Autenticación** | `Authorization: Bearer <token>` (JWT) |
| **Formato error** | `{ "error": "mensaje", "statusCode": N }` |
| **URL local** | `http://localhost:5044` |
| **Swagger** | `http://localhost:5044/swagger` |

### Roles disponibles

| Rol | Descripción |
| --- | --- |
| `Admin` | Acceso total: gestión de usuarios, roles, categorías, eliminación de cursos |
| `Instructor` | Crea y gestiona sus propios cursos y lecciones |
| `Estudiante` | Consulta cursos publicados, se inscribe y ve sus inscripciones |

---

## Módulo 1 — Auth · `/api/auth`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Público | ✅ Implementado |
| `POST` | `/api/auth/login` | Público | ✅ Implementado |
| `GET` | `/api/auth/me` | Bearer (cualquier rol) | ✅ Implementado |
| `POST` | `/api/auth/logout` | Bearer (cualquier rol) | ✅ Implementado |

> **Nota:** Logout es *stateless*. El servidor responde 200 y el cliente debe borrar el token localmente.

---

## Módulo 2 — Users · `/api/users`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `GET` | `/api/users` | Bearer · Admin | ✅ Implementado |
| `GET` | `/api/users/{id}` | Bearer · Admin | ✅ Implementado |
| `POST` | `/api/users` | Bearer · Admin | ✅ Implementado |
| `PUT` | `/api/users/{id}` | Bearer · Admin | ✅ Implementado |
| `PATCH` | `/api/users/{id}/status` | Bearer · Admin | ✅ Implementado |
| `PATCH` | `/api/users/{id}/role` | Bearer · Admin | ✅ Implementado |

---

## Módulo 3 — Roles · `/api/roles`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `GET` | `/api/roles` | Bearer · Admin | ✅ Implementado |

---

## Módulo 4 — Categories · `/api/categories`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `GET` | `/api/categories` | Bearer (cualquier rol) | ✅ Implementado |
| `GET` | `/api/categories/{id}` | Bearer (cualquier rol) | ✅ Implementado |
| `POST` | `/api/categories` | Bearer · Admin | ✅ Implementado |
| `PUT` | `/api/categories/{id}` | Bearer · Admin | ✅ Implementado |
| `DELETE` | `/api/categories/{id}` | Bearer · Admin | ✅ Implementado |

---

## Módulo 5 — Courses · `/api/courses`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `GET` | `/api/courses` | Bearer (cualquier rol) | ✅ Implementado |
| `GET` | `/api/courses/me` | Bearer · Instructor | ✅ Implementado |
| `GET` | `/api/courses/{id}` | Bearer (cualquier rol) | ✅ Implementado |
| `POST` | `/api/courses` | Bearer · Instructor | ✅ Implementado |
| `PUT` | `/api/courses/{id}` | Bearer · Instructor (dueño) | ✅ Implementado |
| `PATCH` | `/api/courses/{id}/status` | Bearer · Instructor (dueño) | ✅ Implementado |
| `DELETE` | `/api/courses/{id}` | Bearer · Instructor (dueño) o Admin | ✅ Implementado |

---

## Módulo 6 — Lessons · `/api/courses/{courseId}/lessons`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `GET` | `/api/courses/{courseId}/lessons` | Bearer (cualquier rol) | ✅ Implementado |
| `GET` | `/api/courses/{courseId}/lessons/{id}` | Bearer (cualquier rol) | ✅ Implementado |
| `POST` | `/api/courses/{courseId}/lessons` | Bearer · Instructor (dueño del curso) | ✅ Implementado |
| `PUT` | `/api/courses/{courseId}/lessons/{id}` | Bearer · Instructor (dueño del curso) | ✅ Implementado |
| `DELETE` | `/api/courses/{courseId}/lessons/{id}` | Bearer · Instructor (dueño del curso) | ✅ Implementado |

---

## Módulo 7 — Enrollments · `/api/enrollments`

| Método | Ruta | Auth / Rol | Estado |
| --- | --- | --- | --- |
| `POST` | `/api/enrollments` | Bearer (cualquier rol) | ✅ Implementado |
| `GET` | `/api/enrollments/me` | Bearer (cualquier rol) | ✅ Implementado |
| `DELETE` | `/api/enrollments/{id}` | — | ❌ No implementado |

> El `estudianteId` se extrae del token, no del body.

---

## Pendientes / No implementados

| Módulo | Descripción | Estado |
| --- | --- | --- |
| Reportes | Estadísticas, dashboards | ❌ No implementado |
| Progreso de lecciones | Completar lección, % avance | ❌ No implementado |
| `DELETE /api/enrollments/{id}` | Cancelar inscripción | ❌ No implementado |
