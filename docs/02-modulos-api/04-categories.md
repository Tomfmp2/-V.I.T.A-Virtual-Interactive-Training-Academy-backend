# Módulo 4 — Categories

> **Basado en rama `develop`** · **Fecha:** 13/08/2026

---

## Estado

✅ **Implementado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/categories` | Bearer · cualquier rol | Listar todas las categorías activas |
| `GET` | `/api/categories/{id}` | Bearer · cualquier rol | Ver categoría por ID |
| `POST` | `/api/categories` | Bearer · Admin | Crear nueva categoría |
| `PUT` | `/api/categories/{id}` | Bearer · Admin | Actualizar categoría |
| `DELETE` | `/api/categories/{id}` | Bearer · Admin | Eliminar categoría (bloqueado si está en uso) |

> Todos los endpoints requieren autenticación. Lectura: cualquier rol. Escritura: solo Admin.

---

## Request / Response

### `GET /api/categories`

**Response 200:**
```json
[
  {
    "id": 1,
    "nombre": "Programación",
    "slug": "programacion",
    "activo": true
  },
  {
    "id": 2,
    "nombre": "Diseño",
    "slug": "diseno",
    "activo": true
  }
]
```

---

### `POST /api/categories`

**Request:**
```json
{
  "nombre": "Marketing Digital"
}
```

| Campo | Tipo | Validación |
| --- | --- | --- |
| `nombre` | `string` | Requerido, debe ser único |

**Response 201:** objeto categoría creada.

---

### `PUT /api/categories/{id}`

**Request:**
```json
{
  "nombre": "Marketing y Publicidad"
}
```

**Response 200:** objeto categoría actualizada.

---

### `DELETE /api/categories/{id}`

No requiere body.

**Response 204** en caso de éxito.

---

## Códigos de error relevantes

| Código | Cuándo | Mensaje típico |
| --- | --- | --- |
| `404` | Categoría no encontrada | `"Categoría no encontrada."` |
| `409` | Nombre de categoría ya existe | `"Ya existe una categoría con ese nombre."` |
| `409` | Categoría en uso por cursos | `"No se puede eliminar: la categoría está en uso por cursos."` |

---

## Reglas de negocio

- El slug se genera automáticamente a partir del nombre (sin tildes, minúsculas, guiones).
- No se puede eliminar una categoría que tenga cursos asociados (`409`).
- Las categorías del seed inicial son: `Programación`, `Diseño`, `Marketing Digital`.
- El campo `activo` indica si la categoría está disponible; solo las activas aparecen en la lista.

---

## Cómo probar en Swagger

1. **Lectura (cualquier rol):** hacer login, autorizar, `GET /api/categories` → `200`.
2. **Escritura (Admin):** hacer login con `admin@vita.local`, autorizar.
3. `POST /api/categories` → body `{ "nombre": "Idiomas" }` → `201`.
4. `PUT /api/categories/{id}` → body `{ "nombre": "Idiomas Internacionales" }` → `200`.
5. `DELETE /api/categories/{id}` → `204` si no tiene cursos, `409` si tiene.

---

## Fuente en código

- Controller: `Vita.Api/Controllers/CategoriesController.cs`
- Service: `Vita.Api/Services/CategoryService.cs` / `ICategoryService.cs`
- DTOs: `Vita.Api/Dtos/Categories/` (`CategoryRequest`)
