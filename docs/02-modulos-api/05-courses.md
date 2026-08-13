# Módulo 5 — Courses

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

✅ **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/courses` | Bearer · cualquier rol | Listar cursos (filtrado por rol en el servicio) |
| `GET` | `/api/courses/me` | Bearer · Instructor | Ver solo los cursos propios del instructor |
| `GET` | `/api/courses/{id}` | Bearer · cualquier rol | Ver detalle de un curso |
| `POST` | `/api/courses` | Bearer · Instructor | Crear nuevo curso |
| `PUT` | `/api/courses/{id}` | Bearer · Instructor (dueño) | Actualizar curso |
| `PATCH` | `/api/courses/{id}/status` | Bearer · Instructor (dueño) | Cambiar estado (borrador/publicado) |
| `DELETE` | `/api/courses/{id}` | Bearer · Instructor (dueño) o Admin | Eliminar curso |

> El `IdInstructor` del curso se extrae del token, no del body.  
> `GET /api/courses/me` está declarado **antes** de `GET /api/courses/{id}` en el router para evitar conflictos.

---

## Request / Response

### `GET /api/courses`

**Response 200:**
```json
[
  {
    "id": 1,
    "titulo": "Introducción a Python",
    "slug": "introduccion-a-python",
    "descripcionCorta": "Aprende Python desde cero.",
    "categoriaNombre": "Programación",
    "nivelNombre": "Principiante",
    "estado": "Publicado",
    "instructorNombre": "Instructor Demo"
  }
]
```

---

### `POST /api/courses`

**Request:**
```json
{
  "titulo": "Introducción a Python",
  "idCategoria": 1,
  "idNivel": 1,
  "descripcionCorta": "Aprende Python desde cero.",
  "descripcionLarga": "Curso completo de Python para principiantes...",
  "imagenPortadaUrl": "https://ejemplo.com/imagen.jpg",
  "duracionEstimadaMin": 300
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `titulo` | `string` | Requerido, 5–200 caracteres |
| `idCategoria` | `int` | Requerido, debe existir y estar activa |
| `idNivel` | `int` | Requerido, debe existir |
| `descripcionCorta` | `string?` | Opcional, máx 300 caracteres |
| `descripcionLarga` | `string?` | Opcional, sin límite |
| `imagenPortadaUrl` | `string?` | Opcional, máx 255 caracteres |
| `duracionEstimadaMin` | `int?` | Opcional, 1–100000 |

**Response 201:** `CourseResponse` completo.

---

### `GET /api/courses/{id}` — Response 200

```json
{
  "id": 1,
  "titulo": "Introducción a Python",
  "slug": "introduccion-a-python",
  "descripcionCorta": "Aprende Python desde cero.",
  "descripcionLarga": "Curso completo...",
  "imagenPortadaUrl": null,
  "duracionEstimadaMin": 300,
  "idCategoria": 1,
  "categoriaNombre": "Programación",
  "idNivel": 1,
  "nivelNombre": "Principiante",
  "estado": "Borrador",
  "idInstructor": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "instructorNombre": "Instructor Demo",
  "createdAt": "2026-08-13T00:00:00Z",
  "updateAt": null
}
```

---

### `PATCH /api/courses/{id}/status`

**Request:**
```json
{
  "estado": "publicado"
}
```

Valores permitidos: `borrador`, `publicado` (insensible a mayúsculas).

**Response 200:** `CourseResponse` con estado actualizado.

---

## Catálogos de referencia (seed)

### Niveles (`idNivel`)

| ID | Nombre |
| --- | --- |
| 1 | Principiante |
| 2 | Intermedio |
| 3 | Avanzado |

### Estados de curso

| Valor para `status` | Nombre visible |
| --- | --- |
| `borrador` | Borrador |
| `publicado` | Publicado |

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `400` | Categoría no existe o inactiva | `"La categoría no existe o está inactiva."` |
| `400` | Nivel no existe | `"El nivel no existe."` |
| `400` | Estado inválido | `"Estado inválido. Valores permitidos: borrador, publicado."` |
| `403` | Instructor intenta modificar curso ajeno | `"No tienes permiso para modificar este curso."` |
| `404` | Curso no encontrado | `"Curso no encontrado."` |
| `409` | Título duplicado para ese instructor | `"Ya tienes un curso con ese título."` |
| `409` | Eliminar curso con estudiantes inscritos | `"No se puede eliminar: el curso tiene estudiantes inscritos."` |
| `409` | Eliminar curso con lecciones | `"No se puede eliminar: el curso tiene lecciones asociadas."` |

---

## Reglas de negocio

- El slug se genera automáticamente desde el título al crear el curso.
- Solo el Instructor **dueño** del curso puede editarlo, cambiar estado o eliminarlo.
- El Admin puede eliminar cualquier curso aunque no sea el dueño.
- Un Instructor no puede tener dos cursos con el mismo título.
- No se puede eliminar un curso que tenga inscripciones activas o lecciones.
- El listado de `GET /api/courses` puede filtrar según el rol del usuario (implementado en el servicio).

---

## Cómo probar en Swagger

1. Login con `instructor@vita.local`, autorizar.
2. `POST /api/courses` → body con título, idCategoria (1), idNivel (1) → `201`.
3. `GET /api/courses/me` → ver tus cursos.
4. `PATCH /api/courses/{id}/status` → `{ "estado": "publicado" }` → `200`.
5. `GET /api/courses` → ver todos los cursos (cualquier rol).

---

## Fuente en código

- Controller: `Vita.Api/Controllers/CoursesController.cs`
- Service: `Vita.Api/Services/CourseService.cs` / `ICourseService.cs`
- Ownership: `Vita.Api/Services/CourseOwnershipService.cs`
- DTOs: `Vita.Api/Dtos/Courses/` (`CourseCreateRequest`, `CourseUpdateRequest`, `CourseStatusRequest`, `CourseResponse`, `CourseListItemResponse`)
