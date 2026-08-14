# Módulo 7 — Enrollments

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

 **Implementado** (parcial — cancelar inscripción no implementado)

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `POST` | `/api/enrollments` | Bearer · cualquier rol | Inscribirse en un curso publicado |
| `GET` | `/api/enrollments/me` | Bearer · cualquier rol | Ver mis inscripciones |
| `DELETE` | `/api/enrollments/{id}` | — | No implementado |

> El `estudianteId` se extrae del token, no se envía en el body.

---

## Request / Response

### `POST /api/enrollments`

**Request:**
```json
{
  "cursoId": 5
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `cursoId` | `int` | Requerido, mínimo 1 |

**Response 201:**
```json
{
  "id": 12,
  "cursoId": 5,
  "cursoTitulo": "Introducción a Python",
  "fechaInscripcion": "2026-08-13T17:30:00Z",
  "estado": "Activa"
}
```

---

### `GET /api/enrollments/me`

**Response 200:**
```json
[
  {
    "id": 12,
    "cursoId": 5,
    "cursoTitulo": "Introducción a Python",
    "fechaInscripcion": "2026-08-13T17:30:00Z",
    "estado": "Activa"
  }
]
```

---

### `DELETE /api/enrollments/{id}`

>  **No implementado.** El endpoint no existe en la versión actual del API.
> Está planeado para una iteración futura (cancelar inscripción).

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Curso no está publicado | `"El curso no está publicado."` |
| `404` | Curso no encontrado | `"Curso no encontrado."` |
| `409` | Ya inscrito en ese curso | `"Ya estás inscrito en este curso."` |

---

## Reglas de negocio

- Solo se puede inscribir en cursos con estado **Publicado**.
- El `estudianteId` se toma del claim `sub` del token JWT (no se puede inscribir en nombre de otro usuario).
- No se puede inscribir dos veces al mismo curso (`409` si ya existe inscripción).
- El estado de inscripción es `Activa` al crear (catalogo del seed: `Activa`, `Cancelada`).
- Cancelar inscripción (`DELETE /api/enrollments/{id}`) **no está implementado**.

---

## Cómo probar en Swagger

1. Login con `estudiante@vita.local`, autorizar.
2. Asegúrate de que exista un curso publicado (con estado `publicado`).
3. `POST /api/enrollments` → body `{ "cursoId": <id> }` → `201`.
4. `GET /api/enrollments/me` → ver inscripciones del estudiante autenticado → `200`.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/EnrollmentsController.cs`
- Service: `Vita.Api/Services/EnrollmentService.cs` / `IEnrollmentService.cs`
- DTOs: `Vita.Api/Dtos/Enrollments/` (`EnrollmentRequest`, `EnrollmentResponse`)
