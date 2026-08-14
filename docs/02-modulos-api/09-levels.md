# Módulo — Levels (`/api/levels`)

> **Basado en rama `feature/reports-api`** · **Fecha:** 13/08/2026
> La API de niveles ya existía en `develop`; esta ficha completa el entregable de documentación.

---

## Estado

 **Implementado** (código) ·  **Documentado**

---

## Rutas

| Método | Ruta | Auth / Rol | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/levels` | Bearer · cualquier rol | Listar niveles |
| `GET` | `/api/levels/{id}` | Bearer · cualquier rol | Ver nivel por ID |
| `POST` | `/api/levels` | Bearer · Admin | Crear nivel |
| `PUT` | `/api/levels/{id}` | Bearer · Admin | Actualizar nivel |
| `DELETE` | `/api/levels/{id}` | Bearer · Admin | Eliminar nivel (409 si está en uso) |

---

## Request / Response

### `GET /api/levels`

**Response 200:**
```json
[
  { "id": 1, "nombre": "Principiante" },
  { "id": 2, "nombre": "Intermedio" },
  { "id": 3, "nombre": "Avanzado" }
]
```

### `POST /api/levels` / `PUT /api/levels/{id}`

**Request:**
```json
{ "nombre": "Experto" }
```

**Response:** `201` (create) o `200` (update) con el objeto `LevelResponse`.

---

## Códigos de error relevantes

| Código | Cuándo |
| --- | --- |
| `404` | Nivel no encontrado |
| `409` | Nombre duplicado, o delete con nivel en uso por cursos |
| `401` / `403` | Auth / rol |

---

## Notas

- Los seeds de Development crean Principiante, Intermedio y Avanzado.
- Los cursos referencian niveles por `idNivel` en create/update de cursos.
