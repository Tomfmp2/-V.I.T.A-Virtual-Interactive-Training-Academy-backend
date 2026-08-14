# Módulo 6 — Lessons

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

 **Implementado**

---

## Rutas

Base: `/api/courses/{courseId}/lessons`

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/courses/{courseId}/lessons` | Bearer · cualquier rol | Listar lecciones del curso (ordenadas por `orden`) |
| `GET` | `/api/courses/{courseId}/lessons/{id}` | Bearer · cualquier rol | Ver detalle de una lección |
| `POST` | `/api/courses/{courseId}/lessons` | Bearer · Instructor (dueño) o Admin | Crear lección en el curso |
| `PUT` | `/api/courses/{courseId}/lessons/{id}` | Bearer · Instructor (dueño) o Admin | Actualizar lección |
| `DELETE` | `/api/courses/{courseId}/lessons/{id}` | Bearer · Instructor (dueño) o Admin | Eliminar lección |

> Las mutaciones las puede hacer el **Instructor dueño** del curso o un **Admin**.
> Publicar el curso sigue siendo exclusivo del instructor asignado (ver módulo Cursos).

---

## Request / Response

### `GET /api/courses/{courseId}/lessons`

**Response 200:**
```json
[
  {
    "id": 1,
    "cursoId": 5,
    "titulo": "Introducción al curso",
    "descripcion": "Bienvenida y objetivos.",
    "recurso": "https://ejemplo.com/video1.mp4",
    "orden": 1
  },
  {
    "id": 2,
    "cursoId": 5,
    "titulo": "Instalación del entorno",
    "descripcion": null,
    "recurso": null,
    "orden": 2
  }
]
```

---

### `POST /api/courses/{courseId}/lessons`

**Request:**
```json
{
  "titulo": "Introducción al curso",
  "descripcion": "Bienvenida y objetivos del módulo.",
  "recurso": "https://ejemplo.com/video1.mp4",
  "orden": 1
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `titulo` | `string` | Requerido, 3–200 caracteres |
| `descripcion` | `string?` | Opcional |
| `recurso` | `string?` | Opcional, URL válida, máx 255 caracteres |
| `orden` | `int` | Requerido, mínimo 1 |

**Response 201:** `LessonResponse` de la lección creada.

---

### `PUT /api/courses/{courseId}/lessons/{id}`

Mismo body que `POST`. **Response 200:** `LessonResponse` actualizado.

---

### `DELETE /api/courses/{courseId}/lessons/{id}`

No requiere body. **Response 204** en caso de éxito.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `403` | Instructor no dueño (sin rol Admin) | `"Solo el instructor dueño del curso puede gestionar sus lecciones."` |
| `404` | Curso no encontrado | `"Curso no encontrado."` |
| `404` | Lección no encontrada | `"Lección no encontrada."` |

---

## Reglas de negocio

- Las lecciones se devuelven **ordenadas por el campo `orden`** (ascendente).
- El campo `orden` define la posición de la lección dentro del curso. El equipo debe gestionar el orden manualmente (no hay reordenamiento automático).
- El campo `recurso` es una URL libre (video, PDF, enlace externo). No hay validación de tipo de contenido.
- Solo el Instructor dueño del curso o un Admin puede crear, editar o eliminar lecciones.
- El userId se extrae del **token**, no del body.

---

## Cómo probar en Swagger

1. Login con `instructor@vita.local`, autorizar.
2. Crear un curso primero (`POST /api/courses`)  obtener su `id`.
3. `POST /api/courses/{courseId}/lessons`  body con título, orden 1  `201`.
4. `GET /api/courses/{courseId}/lessons`  ver lista ordenada  `200`.
5. `PUT /api/courses/{courseId}/lessons/{id}`  actualizar título  `200`.
6. `DELETE /api/courses/{courseId}/lessons/{id}`  `204`.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/LessonsController.cs`
- Service: `Vita.Api/Services/LessonService.cs` / `ILessonService.cs`
- DTOs: `Vita.Api/Dtos/Lessons/` (`LessonRequest`, `LessonResponse`)
